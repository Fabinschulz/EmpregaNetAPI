# ADR 0020: Layout do Terraform da EmpregaNet, herdado do IntegradorAnymarket

## Status

Aceite. Implementa a seção "Terraform" do [ADR 0018](0018-infra-ecs-fargate-com-auto-scaling.md). O código está em
[`infra/`](../../../infra/README.md) e **não foi aplicado**.

## Contexto

O ADR 0018 fixou a arquitetura (ECS Fargate, ALB, RDS, janela 07h-00h) e pediu Terraform; o ADR 0019 deixou a
aplicação pronta para várias réplicas. Faltava a forma de organizar o código. O projeto IntegradorAnymarket já
aplica, em produção, um layout de Terraform validado; adotá-lo evita inventar convenções e mantém uma só maneira de
operar infraestrutura nos dois projetos.

## Decisão

1. **Mesmas convenções do Integrador:** `infra/` com `providers.tf`, `variables.tf`, `resource.tf`, `output.tf` na
   raiz e um módulo por assunto em `modules/<nome>/{resource,variables,outputs}.tf`; provider AWS fixado em `6.66.0`
   com `.terraform.lock.hcl` versionado por pasta; **workspace = ambiente** com guarda que aborta no `default`; estado
   no S3 com `use_lockfile`, bucket criado por um `bootstrap/` com estado local; `allowed_account_ids`; tags comuns;
   regras de security group como recursos próprios; `ignore_changes` na task definition; circuit breaker; tarefas em
   subnet pública com IP público, sem NAT; CI com `fmt -check` e `validate` sem credenciais.
2. **Diferenças justificadas:**
   - **Backend parcial** (`bucket` e região em `backend.hcl`): a conta dedicada ainda não existe, e o Integrador
     grava o ID da conta no código.
   - **Rede própria** (`modules/network`): o Integrador reaproveita uma VPC existente; aqui o ADR 0018 pede VPC
     dedicada com subnets privadas isoladas para o RDS.
   - **Dois serviços atrás de um ALB** com roteamento por caminho (`/api/*` para a API, resto para o Next), em vez de
     um serviço só; a mesma origem preserva o cookie host-only.
   - **Segredos:** Secrets Manager com chaves JSON (`valueFrom` com `:chave::`), como no Integrador, no lugar dos
     parâmetros SSM que o plano cogitava: SSM não seleciona chave dentro de um valor e custa a mesma ordem de grandeza
     (US$ 0,40 por segredo).
   - **Senha do banco estática e write-only** (`password_wo` com variável efêmera): não entra no state nem no plan.
     O `manage_master_user_password` foi descartado: o RDS rotaciona a senha a cada 7 dias por padrão e a connection
     string da aplicação, copiada uma vez para o segredo, ficaria inválida (a revisão do código achou isso; o
     desligamento da rotação num segredo gerenciado não está confirmado na documentação). A troca de senha é
     manual: atualizar o segredo e aumentar `db_master_password_version`. Usuário de aplicação separado do mestre
     fica adiado (exigiria rodar SQL de dentro da VPC).
   - **O `desired_count` também é ignorado:** o Application Auto Scaling é dono dele, inclusive o zero da madrugada.
   - **Migração fora do boot** (ADR 0019): uma task definition `migrate` (mesma imagem da API, `--migrate-only`)
     que a CI executa antes de cada deploy; as réplicas sobem com `Startup__RunMigrations=false` e `RunSeed=false`.
   - **Agenda** por Application Auto Scaling (serviços) e EventBridge Scheduler (RDS e alarmes), conforme o ADR 0018.
3. **Verificação estática própria:** `infra/scripts/check-open-ingress.sh` falha se um security group aceitar entrada
   de `0.0.0.0/0` fora do módulo `alb` ou usar bloco `ingress` inline (tarefa 4.7), sem depender de ferramenta
   externa; provado com um caso que viola a regra.
4. **O frontend chama a API pelo hostname público** (`API_INTERNAL_BASE_URL`), pelo ALB, sem Cloud Map: um serviço a
   menos a pagar. O SSR se identifica pelo segredo `X-Internal-Caller` (ADR 0019), então o salto extra não o limita
   por IP.

## Consequências

- Operar a infraestrutura é igual nos dois projetos: mesmo fluxo de `workspace`, `plan -detailed-exitcode`, `-target`
  na primeira subida.
- O primeiro apply exige uma ordem (base, segredo, imagens, migração, restante), porque a API se recusa a subir com
  schema atrasado. Está descrita no README; um `apply` único criaria serviços que falham até a migração rodar.
- O tráfego do SSR atravessa o ALB (LCU e latência a mais); aceito até haver medição.
- Confiar nas subnets públicas inteiras em `ForwardedHeaders` é mais largo que confiar só no ALB, mas as tarefas só
  aceitam entrada do security group do ALB e o limite mínimo /16 do ADR 0019 continua valendo.
- Validado só estaticamente: `plan` e `apply` dependem de conta, credenciais e certificado.

## Pendências e fora de escopo

- Conta AWS dedicada, região (LGPD), domínio, certificado ACM e identidade do SES (decisões pendentes do ADR 0018).
- Workflows de CD para ECS (`cd-image` e `cd-ecs`): substituir o `cd-ec2`, rodar a tarefa de migração e atualizar os
  serviços. O `main-deploy.yml` mantém imagem e deploy desligados.
- Lambda de conferência do RDS (06:45), ação do Budgets que nega aumento de capacidade (90 %) e `plan` agendado com
  papel somente leitura (tarefa 4.8).
- Sentry, WAF, Redis, NAT e Multi-AZ, com os gatilhos do ADR 0018.
