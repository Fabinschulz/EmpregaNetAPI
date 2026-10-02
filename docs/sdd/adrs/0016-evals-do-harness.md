# ADR 0016: Evals do Harness — determinísticos e offline primeiro; execução ao vivo adiada com gatilho

## Status

Aceite.

## Contexto

O único componente probabilístico do EmpregaNet em uso é o **Harness** (`.claude/`: 8 agentes, 6 skills,
~2 400 linhas de prompt), que gera código, revê diffs, roteia trabalho e aplica os gates do SDD. O
`EmpregaNet.AI` é scaffold: regista o cliente OpenAI, mas não faz nenhuma chamada a modelo, e
`Agents/`, `Prompts/` e `Skills/` estão vazios.

O Harness muda com frequência (nove commits em seis semanas) e não tinha nenhuma verificação. Uma regressão
num prompt não falha teste nenhum, e o mesmo vale para uma troca de modelo (7 dos 8 agentes usam
`model: inherit`) ou para uma `description` de skill reescrita a ponto de a skill deixar de carregar sozinha.

Foi desenhado um framework completo (runner ao vivo sobre o Claude Agent SDK, juiz LLM, repetição
estatística, baseline × candidato). Antes de o construir, os graders determinísticos desse desenho foram
aplicados, offline e sem custo de LLM, às **89 execuções reais de subagentes** que o Claude Code já tinha
gravado:

| Grader | Resultado |
| ------ | --------- |
| Agente que julga a alterar o repositório | 0 — o `Bash` dos read-only foi usado só para ler e para reproduções no scratchpad |
| `confidence: HIGH` com edição e sem validação executada | 0 |
| Evidência `ficheiro:linha` que não existe | 0 em 115 |
| Delegação com ficheiro colado | 0 em 99 (mediana de 2,7 mil caracteres por prompt) |
| Bloco de contrato fora do padrão | **6 em 38** — `alta`/`alto` em vez de `HIGH`, negrito em vez de YAML, evidência em prosa |
| Credencial em prompt de delegação | **22 em 99**, quase todas para o `e2e-qa-engineer` |

E o check estático encontrou duas referências entre ficheiros por número de secção, proibidas por
[`docs/agents/README.md`](../../agents/README.md); uma delas apontava para a secção errada.

Conclusão: as regras de comportamento caras de verificar estão a ser cumpridas. As violações reais são de
**forma** e de **higiene**, e graders determinísticos apanham-nas. O framework ao vivo, hoje, gastaria
tokens para confirmar o que a auditoria offline já mostra.

## Decisão

1. **Check estático na CI** — [`.claude/evals/check-harness.mjs`](../../../.claude/evals/check-harness.mjs),
   em Node sem dependências, corrido pelo workflow `CI - Harness` em PR que toque `.claude/**`,
   `docs/agents/**` ou `docs/skills/**`. Bloqueante.
2. **Auditoria offline local** — [`.claude/evals/audit-traces.mjs`](../../../.claude/evals/audit-traces.mjs)
   lê os transcripts em `~/.claude/projects/<projecto>/` e aplica os graders da tabela acima. Não corre na
   CI (os transcripts não estão lá), não grava ficheiro e não falha: o resultado vai para o terminal, como
   os relatórios de QA.
3. **Os evals só cobram regra já escrita.** Cada violação cita a fonte (`harness-contract`,
   `docs/agents/README.md`, …). Regra nova entra primeiro no documento dono; o eval vem depois. Assim o eval
   não vira uma segunda fonte de norma em conflito com as skills.
4. **Sem agente, skill ou linha nova no `CLAUDE.md`.** O eval é ferramenta, não papel; o `CLAUDE.md` é contexto
   pago em toda tarefa.

## Adiado — com gatilho de retorno

| Capacidade | Gatilho |
| ---------- | ------- |
| Runner ao vivo (Agent SDK, worktree num commit fixado) com casos históricos — ex.: reverter `5bf6eb8` e medir se o `code-reviewer` acha o defeito; dar o bug ao `dotnet-implementer` e correr os testes do próprio commit | Troca do modelo por omissão; reescrita de `meta-agent` ou `sdd-orchestrator`; ou a auditoria offline mostrar violação comportamental (não só de forma) |
| Juiz LLM com rubrica, calibrado contra ~30 rótulos humanos (κ ≥ 0,6) antes de ser gate | Existir o runner ao vivo e um critério que regex não decide (ex.: "o PRD contém solução técnica?") |
| Repetição estatística: n por caso, intervalo de Wilson, pass^k para crítico, bootstrap pareado baseline × candidato, teste A/A para fixar a margem | Existir o runner ao vivo |
| Evals da IA do produto (parsing de currículo, matching) | A primeira feature de IA entrar no SDD: a suíte de evals passa a ser critério de aceite do `spec.md` dela, com dataset sintético (nunca currículo real — LGPD), invariância contrafactual de atributos protegidos no ranking e injeção de prompt dentro do currículo |

## Consequências

- Mudança de forma no Harness (agente sem secção obrigatória, skill fora dos índices, ligação partida,
  referência por número de secção) falha a CI em vez de degradar em silêncio.
- A drift do bloco de contrato foi corrigida na fonte — `harness-contract` 1.1.0 fixa YAML literal com níveis
  em inglês — e a auditoria offline mostra se volta a acontecer.
- O `Bash` dos agentes read-only fica, porque o `code-reviewer` monta reproduções no scratchpad e isso tem
  valor. "Não alterar o repositório" passa a regra explícita nos prompts, e deixa de ser descrito como
  impossibilidade técnica. A auditoria vigia a regra.
- A auditoria compara evidência com a árvore **actual**: um ficheiro renomeado depois da execução aparece como
  "a conferir", não como alucinação.
- O custo recorrente é zero tokens. O primeiro gatilho da tabela acima é o que o muda.
