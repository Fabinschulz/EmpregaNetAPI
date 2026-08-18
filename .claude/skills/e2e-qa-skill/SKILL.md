---
name: e2e-qa-skill
description: Roda uma regressão E2E manual/exploratória do EmpregaNet (frontend) pela UI real (Browser), usando o agent e2e-qa-engineer. Use quando o usuário pedir para "testar o frontend", "rodar regressão", "validar essa tela/fluxo antes de mergear", "fazer QA de X", ou depois de uma mudança relevante em frontend/src/app ou frontend/src/features. Aceita escopo total ou um módulo específico (auth, vagas públicas, dashboard, conta, candidaturas, recrutamento, admin).
---

# E2E Regression — EmpregaNet (frontend)

> Fonte canônica da metodologia (matriz de cenários, severidade, templates completos): [`docs/skills/e2e-qa-skill/SKILL.md`](../../../docs/skills/e2e-qa-skill/SKILL.md) e [`docs/agents/e2e-qa-engineer.md`](../../../docs/agents/e2e-qa-engineer.md). Este arquivo é a orquestração específica do Claude Code (slash command + Agent tool) sobre a mesma metodologia.

Esta skill orquestra uma regressão end-to-end real (não estática) do frontend do EmpregaNet, delegando a execução ao subagente **e2e-qa-engineer** (`.claude/agents/e2e-qa-engineer.md`), que interage com a aplicação pela interface via Browser pane.

## Quando usar

- Pedido explícito: "testa o frontend", "roda uma regressão", "faz QA disso", "valida esse fluxo".
- Depois de implementar ou alterar algo em `frontend/src/app/**` ou `frontend/src/features/**`, antes de dar a tarefa por concluída.
- Para reproduzir um bug relatado pelo usuário em uma tela específica.

Argumento opcional (`args`) para escopo: `auth`, `vagas` (feed público), `dashboard`, `conta`, `candidaturas`, `recrutamento`, `admin`, ou vazio para regressão completa.

## Passo 0 — Pré-condições (sempre primeiro)

1. Garanta que o preview está de pé: `preview_start({name: "frontend"})` (`.claude/launch.json`). Se a porta 3000 já estiver em uso por um processo fora do preview (comum quando o usuário já tem o dev server rodando em outro terminal), **não mate o processo** — anexe com `preview_start({ url: "http://localhost:3000" })`.
2. Confirme que é o app certo: navegue e leia o título/conteúdo da página ("EmpregaUAI"). Se aparecer outra coisa, você pode estar batendo em outro projeto Next.js local — pare e avise o usuário.
3. Confirme que a API (.NET, porta `5225`) está acessível antes de testar qualquer fluxo autenticado ou com dado real. Se não estiver, **pare e registre isso como bloqueio de ambiente** — não prossiga assumindo comportamento.
4. Pergunte/confira se há alguma restrição de dados (ex.: "não crie candidaturas novas", "não mexa nas vagas existentes") antes de iniciar CRUDs.

## Passo 1 — Escopo e matriz de cenários

Determine os módulos a cobrir a partir do argumento recebido. Ordem de prioridade quando o escopo for completo:

1. **Auth/RBAC** (gate de rota, redirecionamentos `/login` e `/nao-autorizado`, validações client-side e server-side, mensagens de erro, fluxo de login/logout, reset de senha, confirmação de email).
2. **Feed público de vagas** (`(public)/vagas`, `/vagas/[id]`) — sem sessão, SEO/SSR
3. **Candidaturas** (fluxo do candidato)
4. **Recrutamento** (`recrutamento/vagas`, `recrutamento/candidatos`, `recrutamento/candidaturas`) — CRUD e RBAC de recrutador
5. **Admin** (`admin/usuarios`, `admin/empresas`) — CRUD e RBAC de admin
6. **Conta** (`conta/perfil`, `conta/seguranca`) e **Dashboard**

Para cada módulo em escopo, a matriz mínima é: acesso/navegação, listar (paginação, filtro, ordenação, estado vazio), criar, editar, excluir, validação de formulário (obrigatórios, formatos, máscaras — ex. telefone/documento), feedback visual (loading/sucesso/erro/toast/modal), negativos/extremos, permissões.

## Passo 2 — Execução

Invoque o subagente `e2e-qa-engineer` (Agent tool, subagent_type: `e2e-qa-engineer`) **sequencialmente**, um módulo por vez (não em paralelo — todos compartilham a mesma aba/sessão autenticada). Para cada módulo, no prompt:

- Informe o escopo exato daquele módulo e a matriz de cenários do Passo 1.
- Informe que a sessão já está autenticada e em qual `tabId`/URL continuar.
- Peça para retornar o relatório estruturado (formato definido no próprio agent).

Aguarde cada módulo terminar antes de iniciar o próximo (rodar em foreground).

## Passo 3 — Consolidação e relatório final

Depois de todos os módulos do escopo:

1. Consolide os relatórios individuais em um único relatório: cenários executados, aprovados, reprovados, bugs por severidade, evidências, fluxos ainda não testados, riscos identificados, recomendação final.
2. Salve o relatório em `docs/qa/e2e-regression-<YYYY-MM-DD-HHmm>.md` no repositório (crie `docs/qa/` se não existir).
3. Resuma o resultado para o usuário no chat — aponte o arquivo e destaque bugs críticos/altos primeiro.

## Notas permanentes

- Se o usuário pedir para gravar em issue tracker os bugs encontrados, isso exige confirmação explícita antes de criar/publicar — não crie issues sozinho.
