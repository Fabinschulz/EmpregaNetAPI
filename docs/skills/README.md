## Skills de conhecimento

Fixam os **fatos e convenções** do projecto. São lidas pelas duas vias: pela thread principal (carregamento
automático ou `/<nome>`) e pelos agents, que as leem com `Read` no arranque.

| Skill | Domínio |
| ----- | ------- |
| [`backend-skill`](../../.claude/skills/backend-skill/SKILL.md) | Camadas .NET, mediator interno, EF Core, contrato HTTP, testes xUnit |
| [`frontend-skill`](../../.claude/skills/frontend-skill/SKILL.md) | Next.js App Router com `cacheComponents`, SCSS Modules, Zod, auth/RBAC, loading canónico, Cucumber |

## Skills de orquestração

Correm na **thread principal** — que tem a ferramenta Agent — e delegam a execução aos agents.

| Skill | Fluxo |
| ----- | ----- |
| [`meta-agent`](../../.claude/skills/meta-agent/SKILL.md) | Roteia um pedido vago ou multi-domínio para o especialista certo e funde as saídas |
| [`sdd-orchestrator`](../../.claude/skills/sdd-orchestrator/SKILL.md) | Conduz `prd.md` → `design.md` → `spec.md`/`tasks.md` com gate humano por fase |
| [`e2e-qa-skill`](../../.claude/skills/e2e-qa-skill/SKILL.md) | Regressão E2E pela UI real; contém a metodologia e delega ao agent `e2e-qa-engineer` |

O governo do SDD (fases A–E, regras, conteúdo mínimo de cada artefacto) continua em
[`../sdd/`](../sdd/) — documentos aprovados e versionados. A skill **aplica-os**, não os reescreve.

---

## Padrão obrigatório de uma skill

```yaml
---
name: <kebab-case, igual ao nome da pasta>
description: <o que faz · quando usar (gatilhos concretos) · quando NÃO usar e a alternativa>
---
```

A `description` é o que decide se a skill é carregada — deve conter os termos que o utilizador usaria
de facto, e a fronteira negativa. Sem `tools` nem `model`: skills não os consomem.

Corpo, conforme aplicável:

| Secção | Conteúdo |
| ------ | -------- |
| `## Quando aplicar` | Tabela Sim/Não, com o destino de cada "Não". |
| `## Ligações` | Documentos e agents relacionados. |
| Conteúdo de domínio | Fatos, regras, armadilhas já pagas — o valor real da skill. |
| `## Validação` | Comandos reais de verificação. |
| `## Checklist de entrega` | Itens verificáveis. |
| `## Anti-padrões` | O que está bloqueado e **porquê**. |
| `## Histórico` | Versão e o que mudou. |

Regras:

- **A skill descreve o projecto; o agent descreve o comportamento.** Não misturar: persona, tom e formato de output pertencem ao agent.
- Cada fato deve ser **verificável no repositório**. Fato que envelheceu é pior que fato ausente — ele engana com confiança.
- Referenciar por título de secção, não por número.
- Não declarar como existente uma ferramenta que não está instalada (ex.: Jest, Playwright).

**Ao adicionar uma skill:** criar em `.claude/skills/<nome>/SKILL.md`, seguir o padrão, e acrescentar linha
neste índice, na tabela de roteamento do `meta-agent` e no [`CLAUDE.md`](../../CLAUDE.md).
