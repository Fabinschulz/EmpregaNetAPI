#!/usr/bin/env python3
"""Deploy da EmpregaNet no ECS Fargate.

Para cada serviço com imagem nova: registra uma revisão da task definition vigente com a imagem trocada. Quando a API
muda, antes disso roda a tarefa de migração (`--migrate-only`) com a mesma imagem e aborta se ela falhar: as réplicas
sobem com Startup__RunMigrations=false e se recusam a iniciar com schema atrasado (ADR 0019). Só então atualiza os
serviços e espera o rollout; o circuit breaker do ECS reverte uma revisão que não fica saudável.

Não usa a ação `amazon-ecs-deploy-task-definition`: a migração precisa acontecer entre o registro e a atualização.
Depende só da biblioteca padrão e da AWS CLI. Os nomes seguem o Terraform (infra/): cluster e serviços
`empreganet-<ambiente>[-api|-web]`, repositórios `empreganet-<ambiente>-<serviço>`, task definition `-migrate`.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import time
from typing import Any, Callable

TAG_PATTERN = re.compile(r"^[0-9a-f]{40}$")
ENVIRONMENT_PATTERN = re.compile(r"^[a-z0-9-]{1,17}$")

# Campos que o describe devolve e o register recusa.
READ_ONLY_FIELDS = (
    "taskDefinitionArn",
    "revision",
    "status",
    "requiresAttributes",
    "compatibilities",
    "registeredAt",
    "registeredBy",
    "deregisteredAt",
)

SERVICES = ("api", "web")


class DeployError(Exception):
    """Falha que deve derrubar o job, com uma mensagem acionável."""


AwsRunner = Callable[..., Any]


def run_aws(*args: str) -> Any:
    """Executa a AWS CLI e devolve o JSON da saída (ou None quando não há saída)."""
    result = subprocess.run(["aws", *args, "--output", "json"], capture_output=True, text=True, check=False)
    if result.returncode != 0:
        raise DeployError(f"aws {' '.join(args[:2])} falhou: {result.stderr.strip() or result.stdout.strip()}")
    return json.loads(result.stdout) if result.stdout.strip() else None


def render_task_definition(described: dict, container: str, image: str) -> tuple[dict, list[dict]]:
    """Devolve a entrada do register-task-definition com a imagem do contêiner trocada, e as tags da revisão atual."""
    definition = dict(described["taskDefinition"])
    containers = definition["containerDefinitions"]

    if not any(c["name"] == container for c in containers):
        raise DeployError(f"O contêiner '{container}' não existe na task definition {definition['family']}.")

    definition["containerDefinitions"] = [
        {**c, "image": image} if c["name"] == container else c for c in containers
    ]
    for field in READ_ONLY_FIELDS:
        definition.pop(field, None)

    return definition, described.get("tags") or []


class Deployer:
    def __init__(
        self,
        environment: str,
        image_tags: dict[str, str],
        *,
        allow_off_window: bool = False,
        aws: AwsRunner = run_aws,
        sleep: Callable[[float], None] = time.sleep,
        clock: Callable[[], float] = time.monotonic,
        log: Callable[[str], None] = print,
        timeout: int = 900,
        poll: int = 15,
    ) -> None:
        if not ENVIRONMENT_PATTERN.match(environment):
            raise DeployError(f"Ambiente inválido: '{environment}'.")

        self.environment = environment
        self.prefix = f"empreganet-{environment}"
        self.cluster = self.prefix
        self.image_tags = {name: tag for name, tag in image_tags.items() if tag}
        self.allow_off_window = allow_off_window
        self.aws = aws
        self.sleep = sleep
        self.clock = clock
        self.log = log
        self.timeout = timeout
        self.poll = poll
        self.summary: list[str] = []

    # ------------------------- Validação ------------------------------
    def validate_inputs(self) -> None:
        if not self.image_tags:
            raise DeployError("Nenhuma imagem para implantar: informe a tag da API, a do frontend ou as duas.")
        for name, tag in self.image_tags.items():
            if not TAG_PATTERN.match(tag):
                raise DeployError(f"Tag da imagem {name} inválida: use o SHA completo do commit (40 hexadecimais).")

    def image_uri(self, service: str) -> str:
        repository = f"{self.prefix}-{service}"
        tag = self.image_tags[service]
        try:
            self.aws("ecr", "describe-images", "--repository-name", repository, "--image-ids", f"imageTag={tag}")
        except DeployError as error:
            raise DeployError(f"A imagem {repository}:{tag} não existe no ECR (publique-a antes): {error}") from error
        repositories = self.aws("ecr", "describe-repositories", "--repository-names", repository)
        return f"{repositories['repositories'][0]['repositoryUri']}:{tag}"

    def describe_services(self) -> dict[str, dict]:
        names = [f"{self.prefix}-{service}" for service in SERVICES]
        response = self.aws("ecs", "describe-services", "--cluster", self.cluster, "--services", *names)
        found = {s["serviceName"]: s for s in response["services"] if s.get("status") == "ACTIVE"}
        missing = [n for n in names if n not in found]
        if missing:
            raise DeployError(f"Serviço(s) ECS ausente(s) ou inativo(s) em {self.cluster}: {', '.join(missing)}.")
        return {service: found[f"{self.prefix}-{service}"] for service in SERVICES}

    def check_window(self, services: dict[str, dict]) -> None:
        # Fora da janela 07h-00h as tarefas estão zeradas e o RDS parado: um deploy não valida nada.
        for service in self.image_tags:
            if services[service]["desiredCount"] == 0 and not self.allow_off_window:
                raise DeployError(
                    f"O serviço {service} está com 0 tarefas (fora da janela 07h-00h): o deploy não validaria nada. "
                    "Rode dentro da janela ou marque allow_off_window."
                )

    def check_database(self) -> None:
        response = self.aws("rds", "describe-db-instances", "--db-instance-identifier", self.prefix)
        status = response["DBInstances"][0]["DBInstanceStatus"]
        if status != "available":
            raise DeployError(f"O RDS {self.prefix} está '{status}': a migração não consegue conectar. Aguarde 'available'.")

    # ------------------------- Task definitions -----------------------
    def register(self, source: str, container: str, image: str) -> str:
        """Registra uma revisão de `source` (família ou ARN) com a imagem trocada; devolve o ARN novo."""
        described = self.aws("ecs", "describe-task-definition", "--task-definition", source, "--include", "TAGS")
        definition, tags = render_task_definition(described, container, image)

        args = ["ecs", "register-task-definition", "--cli-input-json", json.dumps(definition)]
        if tags:
            args += ["--tags", json.dumps(tags)]
        arn = self.aws(*args)["taskDefinition"]["taskDefinitionArn"]
        self.log(f"Registrada {arn}")
        return arn

    # ------------------------- Migração -------------------------------
    def run_migration(self, task_definition_arn: str, network_configuration: dict) -> None:
        response = self.aws(
            "ecs", "run-task",
            "--cluster", self.cluster,
            "--task-definition", task_definition_arn,
            "--launch-type", "FARGATE",
            "--network-configuration", json.dumps(network_configuration),
            "--started-by", f"gha-{os.environ.get('GITHUB_RUN_ID', 'local')}",
        )
        if response.get("failures"):
            raise DeployError(f"O ECS não iniciou a tarefa de migração: {json.dumps(response['failures'])}")

        task_arn = response["tasks"][0]["taskArn"]
        self.log(f"Migração iniciada: {task_arn}")
        task = self.wait_task_stopped(task_arn)

        exit_code = task["containers"][0].get("exitCode")
        self.print_migration_logs(task_arn)
        if exit_code != 0:
            reason = task.get("stoppedReason", "sem motivo informado")
            raise DeployError(f"A migração terminou com código {exit_code} ({reason}). Nenhum serviço foi atualizado.")

        self.log("Migração concluída com código 0.")

    def wait_task_stopped(self, task_arn: str) -> dict:
        deadline = self.clock() + self.timeout
        while True:
            task = self.aws("ecs", "describe-tasks", "--cluster", self.cluster, "--tasks", task_arn)["tasks"][0]
            if task["lastStatus"] == "STOPPED":
                return task
            if self.clock() >= deadline:
                raise DeployError(f"A migração não terminou em {self.timeout} s (status {task['lastStatus']}).")
            self.sleep(self.poll)

    def print_migration_logs(self, task_arn: str) -> None:
        task_id = task_arn.rsplit("/", 1)[-1]
        try:
            events = self.aws(
                "logs", "get-log-events",
                "--log-group-name", f"/ecs/{self.prefix}/migrate",
                "--log-stream-name", f"migrate/migrate/{task_id}",
                "--limit", "60",
            )
        except DeployError as error:  # os logs ajudam, mas não decidem o resultado
            self.log(f"Não foi possível ler os logs da migração: {error}")
            return
        self.log("--- últimas linhas do log da migração ---")
        for event in events.get("events", []):
            self.log(event["message"].rstrip())

    # ------------------------- Rollout --------------------------------
    def update_service(self, service: str, task_definition_arn: str) -> None:
        self.aws(
            "ecs", "update-service",
            "--cluster", self.cluster,
            "--service", f"{self.prefix}-{service}",
            "--task-definition", task_definition_arn,
        )
        self.log(f"Serviço {service} atualizado para {task_definition_arn.rsplit('/', 1)[-1]}")

    def verify_rollout(self, services: list[str]) -> None:
        pending = {f"{self.prefix}-{service}" for service in services}
        deadline = self.clock() + self.timeout

        while pending:
            response = self.aws("ecs", "describe-services", "--cluster", self.cluster, "--services", *sorted(pending))
            for service in response["services"]:
                name = service["serviceName"]
                primary = next(d for d in service["deployments"] if d["status"] == "PRIMARY")
                state = primary.get("rolloutState")

                if state == "FAILED":
                    raise DeployError(
                        f"O rollout de {name} falhou: {primary.get('rolloutStateReason', 'sem motivo informado')}. "
                        "O circuit breaker reverteu para a revisão anterior."
                    )
                if state == "COMPLETED" and len(service["deployments"]) == 1 and service["runningCount"] == service["desiredCount"]:
                    self.log(f"{name}: rollout concluído ({service['runningCount']} tarefa(s) rodando).")
                    pending.discard(name)

            if not pending:
                return
            if self.clock() >= deadline:
                raise DeployError(f"Rollout sem conclusão em {self.timeout} s: {', '.join(sorted(pending))}.")
            self.sleep(self.poll)

    # ------------------------- Orquestração ---------------------------
    def deploy(self) -> None:
        self.validate_inputs()
        services = self.describe_services()
        self.check_window(services)
        images = {service: self.image_uri(service) for service in self.image_tags}

        new_revisions: dict[str, str] = {}
        if "api" in images:
            self.check_database()
            migrate_arn = self.register(f"{self.prefix}-migrate", "migrate", images["api"])
            new_revisions["api"] = self.register(services["api"]["taskDefinition"], "api", images["api"])
            # A tarefa de migração usa a rede e o security group do serviço da API (o único que alcança o RDS).
            self.run_migration(migrate_arn, services["api"]["networkConfiguration"])
            self.summary.append(f"| Migração | `{migrate_arn.rsplit('/', 1)[-1]}` concluída |")

        if "web" in images:
            new_revisions["web"] = self.register(services["web"]["taskDefinition"], "web", images["web"])

        for service, arn in new_revisions.items():
            self.update_service(service, arn)
            self.summary.append(f"| {service} | `{arn.rsplit('/', 1)[-1]}` → `{self.image_tags[service][:12]}` |")

        self.verify_rollout(list(new_revisions))


def write_summary(environment: str, lines: list[str]) -> None:
    path = os.environ.get("GITHUB_STEP_SUMMARY")
    if not path:
        return
    with open(path, "a", encoding="utf-8") as handle:
        handle.write(f"### Deploy ECS - {environment}\n\n| Item | Resultado |\n| --- | --- |\n")
        handle.write("\n".join(lines) + "\n")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--environment", required=True, help="Nome do ambiente (workspace do Terraform), ex.: homolog")
    parser.add_argument("--api-tag", default="", help="SHA da imagem da API; vazio mantém a revisão atual")
    parser.add_argument("--web-tag", default="", help="SHA da imagem do frontend; vazio mantém a revisão atual")
    parser.add_argument("--allow-off-window", action="store_true", help="Permite implantar com os serviços zerados")
    parser.add_argument("--timeout", type=int, default=900, help="Segundos de espera da migração e do rollout")
    args = parser.parse_args(argv)

    deployer = Deployer(
        args.environment,
        {"api": args.api_tag, "web": args.web_tag},
        allow_off_window=args.allow_off_window,
        timeout=args.timeout,
    )
    try:
        deployer.deploy()
    except DeployError as error:
        print(f"::error title=Deploy ECS::{error}")
        return 1

    write_summary(args.environment, deployer.summary)
    return 0


if __name__ == "__main__":
    sys.exit(main())
