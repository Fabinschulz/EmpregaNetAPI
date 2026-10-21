"""Testes do ecs_deploy.py com uma AWS falsa: nenhuma chamada real. Rode: python -m unittest discover -s .github/scripts/tests"""

import copy
import json
import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

import ecs_deploy  # noqa: E402
from ecs_deploy import DeployError, Deployer, render_task_definition  # noqa: E402

FIXTURE = os.path.join(os.path.dirname(__file__), "fixtures", "api-taskdef.json")
API_TAG = "a" * 40
WEB_TAG = "b" * 40
NETWORK = {"awsvpcConfiguration": {"subnets": ["subnet-1"], "securityGroups": ["sg-api"], "assignPublicIp": "ENABLED"}}


def load_fixture() -> dict:
    with open(FIXTURE, encoding="utf-8") as handle:
        return json.load(handle)


class FakeAws:
    """Responde no formato da AWS CLI e registra cada chamada como (serviço, operação, args)."""

    def __init__(self, *, migration_exit=0, rds_status="available", desired=1, image_exists=True, rollout=("COMPLETED",)):
        self.calls: list[tuple[str, str, tuple]] = []
        self.migration_exit = migration_exit
        self.rds_status = rds_status
        self.desired = desired
        self.image_exists = image_exists
        self.rollout = list(rollout)
        self.registered = 0

    def operations(self) -> list[str]:
        return [f"{service}.{operation}" for service, operation, _ in self.calls]

    def args_of(self, operation: str) -> list[tuple]:
        return [args for _, op, args in self.calls if op == operation]

    def service(self, name: str, **extra) -> dict:
        return {
            "serviceName": name, "status": "ACTIVE", "desiredCount": self.desired, "runningCount": self.desired,
            "taskDefinition": f"arn:aws:ecs:sa-east-1:1:task-definition/{name}:7",
            "networkConfiguration": NETWORK,
            "deployments": [{"status": "PRIMARY", "rolloutState": extra.get("state", "COMPLETED")}],
        }

    def __call__(self, *args: str):
        service, operation = args[0], args[1]
        self.calls.append((service, operation, args[2:]))

        if (service, operation) == ("ecr", "describe-images"):
            if not self.image_exists:
                raise DeployError("ImageNotFoundException")
            return {"imageDetails": [{}]}
        if (service, operation) == ("ecr", "describe-repositories"):
            return {"repositories": [{"repositoryUri": f"1.dkr.ecr.sa-east-1.amazonaws.com/{args[3]}"}]}
        if (service, operation) == ("rds", "describe-db-instances"):
            return {"DBInstances": [{"DBInstanceStatus": self.rds_status}]}
        if (service, operation) == ("ecs", "describe-services"):
            names = [a for a in args[args.index("--services") + 1:]]
            if self.rollout and operation == "describe-services" and self._is_verify():
                state = self.rollout.pop(0) if len(self.rollout) > 1 else self.rollout[0]
            else:
                state = "COMPLETED"
            return {"services": [self.service(n, state=state) for n in names]}
        if (service, operation) == ("ecs", "describe-task-definition"):
            described = load_fixture()
            family = args[3].rsplit("/", 1)[-1].split(":")[0]
            described["taskDefinition"]["family"] = family
            if family.endswith("-migrate"):
                described["taskDefinition"]["containerDefinitions"][0]["name"] = "migrate"
            if family.endswith("-web"):
                described["taskDefinition"]["containerDefinitions"][0]["name"] = "web"
            return described
        if (service, operation) == ("ecs", "register-task-definition"):
            self.registered += 1
            family = json.loads(args[3])["family"]
            return {"taskDefinition": {"taskDefinitionArn": f"arn:aws:ecs:sa-east-1:1:task-definition/{family}:{100 + self.registered}"}}
        if (service, operation) == ("ecs", "run-task"):
            return {"tasks": [{"taskArn": "arn:aws:ecs:sa-east-1:1:task/empreganet-homolog/abc123"}], "failures": []}
        if (service, operation) == ("ecs", "describe-tasks"):
            return {"tasks": [{"lastStatus": "STOPPED", "stoppedReason": "Essential container exited",
                               "containers": [{"exitCode": self.migration_exit}]}]}
        if (service, operation) == ("logs", "get-log-events"):
            return {"events": [{"message": "Inicialização concluída.\n"}]}
        if (service, operation) == ("ecs", "update-service"):
            return {"service": {}}
        raise AssertionError(f"chamada inesperada: {args}")

    def _is_verify(self) -> bool:
        return "ecs.update-service" in self.operations()


def make(aws: FakeAws, tags: dict[str, str] | None = None, **kwargs) -> Deployer:
    clock = iter(range(0, 10_000, 10))
    return Deployer(
        "homolog", tags if tags is not None else {"api": API_TAG, "web": WEB_TAG},
        aws=aws, sleep=lambda _: None, clock=lambda: next(clock), log=lambda _: None, timeout=100, poll=1, **kwargs,
    )


class RenderTests(unittest.TestCase):
    def test_troca_so_a_imagem_do_conteiner_pedido_e_remove_campos_somente_leitura(self):
        definition, tags = render_task_definition(load_fixture(), "api", "registry/repo:novo")

        self.assertEqual(definition["containerDefinitions"][0]["image"], "registry/repo:novo")
        for field in ecs_deploy.READ_ONLY_FIELDS:
            self.assertNotIn(field, definition)
        original = load_fixture()["taskDefinition"]["containerDefinitions"][0]
        self.assertEqual(definition["containerDefinitions"][0]["secrets"], original["secrets"])
        self.assertEqual(definition["containerDefinitions"][0]["environment"], original["environment"])
        self.assertEqual(definition["executionRoleArn"], load_fixture()["taskDefinition"]["executionRoleArn"])
        self.assertTrue(any(t["key"] == "Project" for t in tags))

    def test_nao_altera_o_dicionario_original(self):
        described = load_fixture()
        before = copy.deepcopy(described)
        render_task_definition(described, "api", "x:y")
        self.assertEqual(described, before)

    def test_contentor_inexistente_falha(self):
        with self.assertRaisesRegex(DeployError, "não existe"):
            render_task_definition(load_fixture(), "worker", "x:y")


class DeployTests(unittest.TestCase):
    def test_caminho_feliz_migra_antes_de_atualizar_e_atualiza_os_dois_servicos(self):
        aws = FakeAws()
        make(aws).deploy()

        operations = aws.operations()
        self.assertLess(operations.index("ecs.run-task"), operations.index("ecs.update-service"))
        updated = [args[args.index("--service") + 1] for args in aws.args_of("update-service")]
        self.assertEqual(updated, ["empreganet-homolog-api", "empreganet-homolog-web"])

    def test_migracao_usa_a_imagem_da_api_e_a_rede_do_servico_da_api(self):
        aws = FakeAws()
        make(aws).deploy()

        (run_args,) = aws.args_of("run-task")
        self.assertEqual(json.loads(run_args[run_args.index("--network-configuration") + 1]), NETWORK)
        registered = [json.loads(a[1]) for a in aws.args_of("register-task-definition")]
        migrate = next(r for r in registered if r["family"].endswith("-migrate"))
        self.assertTrue(migrate["containerDefinitions"][0]["image"].endswith(f"-api:{API_TAG}"))

    def test_migracao_com_falha_nao_atualiza_nenhum_servico(self):
        aws = FakeAws(migration_exit=1)

        with self.assertRaisesRegex(DeployError, "código 1"):
            make(aws).deploy()

        self.assertNotIn("ecs.update-service", aws.operations())

    def test_rds_parado_falha_antes_de_registrar_qualquer_coisa(self):
        aws = FakeAws(rds_status="stopped")

        with self.assertRaisesRegex(DeployError, "stopped"):
            make(aws).deploy()

        self.assertNotIn("ecs.register-task-definition", aws.operations())

    def test_so_o_frontend_nao_migra_nem_olha_o_banco(self):
        aws = FakeAws()
        make(aws, {"api": "", "web": WEB_TAG}).deploy()

        self.assertNotIn("ecs.run-task", aws.operations())
        self.assertNotIn("rds.describe-db-instances", aws.operations())
        updated = [args[args.index("--service") + 1] for args in aws.args_of("update-service")]
        self.assertEqual(updated, ["empreganet-homolog-web"])

    def test_tag_invalida_ou_ausente_falha_cedo(self):
        for tags in ({"api": "latest", "web": ""}, {"api": "", "web": ""}):
            aws = FakeAws()
            with self.assertRaises(DeployError):
                make(aws, tags).deploy()
            self.assertEqual(aws.calls, [])

    def test_imagem_ausente_no_ecr_falha(self):
        with self.assertRaisesRegex(DeployError, "não existe no ECR"):
            make(FakeAws(image_exists=False)).deploy()

    def test_servicos_zerados_fora_da_janela_falham_a_menos_que_permitido(self):
        with self.assertRaisesRegex(DeployError, "0 tarefas"):
            make(FakeAws(desired=0)).deploy()

        make(FakeAws(desired=0), allow_off_window=True).deploy()

    def test_ambiente_invalido_e_recusado(self):
        with self.assertRaises(DeployError):
            Deployer("Prod Uction", {"api": API_TAG})

    def test_rollout_que_falha_derruba_o_job_com_o_motivo(self):
        with self.assertRaisesRegex(DeployError, "falhou"):
            make(FakeAws(rollout=("FAILED",))).deploy()

    def test_rollout_que_nunca_conclui_estoura_o_prazo(self):
        with self.assertRaisesRegex(DeployError, "sem conclusão"):
            make(FakeAws(rollout=("IN_PROGRESS",))).deploy()


if __name__ == "__main__":
    unittest.main()
