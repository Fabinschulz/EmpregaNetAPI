## Agentes

| Agente | Responsabilidade | Escrita? |
| ------ | ---------------- | -------- |
| [`dotnet-architect`](../../.claude/agents/dotnet-architect.md) | Fronteiras de camada, forma da API, estrutura de solução | Não — read-only |
| [`dotnet-implementer`](../../.claude/agents/dotnet-implementer.md) | Código .NET de produção, com build e testes | Sim |
| [`frontend-engineer`](../../.claude/agents/frontend-engineer.md) | UI Next.js, com lint, testes e build | Sim |
| [`test-engineer`](../../.claude/agents/test-engineer.md) | Testes automatizados (xUnit, Cucumber) | Sim, só testes |
| [`code-reviewer`](../../.claude/agents/code-reviewer.md) | Revisão de diff: corretude, segurança, fronteiras | Não — read-only |
| [`debug-specialist`](../../.claude/agents/debug-specialist.md) | Causa raiz e correcção mínima verificada | Sim |
| [`performance-optimizer`](../../.claude/agents/performance-optimizer.md) | Gargalos medidos e optimização verificada | Sim |
| [`e2e-qa-engineer`](../../.claude/agents/e2e-qa-engineer.md) | Regressão pela UI real, via Browser pane | Não altera código |

Orquestração **não** é agente: um subagent não tem acesso à ferramenta Agent e por isso só conseguiria
recomendar, não delegar. Vive como skill — ver [`../skills/README.md`](../skills/README.md).

---

## Separação de responsabilidades

| Preocupação | Onde vive |
| ----------- | --------- |
| **Orquestração** — rotear e encadear | skills `meta-agent`, `sdd-orchestrator` (thread principal, tem a Agent tool) |
| **Conhecimento** — convenções e fatos do projecto | skills `backend-skill`, `frontend-skill`, `e2e-qa-skill` |
| **Execução** — produzir a mudança | agents `dotnet-implementer`, `frontend-engineer`, `test-engineer`, `debug-specialist`, `performance-optimizer` |
| **Validação** — julgar sem alterar | agents `code-reviewer`, `dotnet-architect`, `e2e-qa-engineer` (read-only ou sem escrita em código) |

**Regra de ouro:** um agent nunca repete no seu prompt o que uma skill já fixa.
Cada agent declara `## Contexto obrigatório` com o caminho da skill e **lê** esse ficheiro no arranque —
subagents não têm a ferramenta Skill, mas têm `Read`. Uma fonte, dois caminhos de acesso.

---

## Padrão obrigatório de um agent

Frontmatter — apenas campos que o Claude Code consome:

```yaml
---
name: <kebab-case, igual ao nome do ficheiro>
description: <o que faz · quando usar · quando NÃO usar e para quem encaminhar>
tools: <allowlist explícita — a ausência de Edit/Write é o que garante "read-only">
model: inherit | sonnet | opus
---
```

Corpo, nesta ordem, sem secções vazias:

| Secção | Conteúdo |
| ------ | -------- |
| `## Papel` | 2–3 linhas. Quem é e o que entrega. |
| `## Use quando` / `## Não use quando` | Gatilhos, e tabela de encaminhamento para os casos que não são dele. |
| `## Contexto obrigatório` | Skills a ler no arranque. Não recopiar o conteúdo delas. |
| `## Entradas necessárias` | O que precisa; o que fazer quando falta (perguntar vs assumir e declarar). |
| `## Processo` | Passos numerados e determinísticos. |
| `## Regras invioláveis` | Restrições duras, redigidas como proibições verificáveis. |
| `## Validação` | Como prova o próprio resultado — **comandos reais**, não boas intenções. |
| `## Falhas e escalonamento` | O que fazer quando bloqueia, e para quem passa. |
| `## Formato de saída` | Contrato de output. |

Anti-padrões ao escrever um agent:

- Repetir convenções que já estão numa skill.
- Métricas fabricadas (score 0–100, notas A–F) sem critério objectivo por trás.
- Secções "opcionais" que o próprio texto diz não ser obrigatório usar.
- Campos de frontmatter inventados que nada consome.
- Dar `Edit`/`Write` a um agent cuja função é julgar.
- Referenciar uma regra que não está definida em lado nenhum.
- Referência **entre ficheiros** por número de secção (`SKILL.md §10`) — usar o título entre aspas, que sobrevive a uma reordenação da skill. Dentro do mesmo ficheiro, o número é aceitável.

---

## Convenções

- **Idioma:** respostas e artefactos em português (Brasil); identificadores de código em inglês.
- **Modelo:** `inherit` por omissão, para respeitar a escolha do utilizador. Fixar um modelo só com motivo (o `e2e-qa-engineer` usa `sonnet` por ser execução longa e mecânica).
- **Ao adicionar um agent:** criar em `.claude/agents/`, seguir o padrão acima, e acrescentar linha na tabela deste índice, na de [`../skills/README.md`](../skills/README.md) se houver skill associada, na tabela de roteamento do `meta-agent` e no [`CLAUDE.md`](../../.claude/CLAUDE.md).
