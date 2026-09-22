---
name: harness-contract
description: Contrato transversal do Harness do EmpregaNet — hierarquia de fonte de verdade, classificação de afirmações (facto/inferência/suposição/desconhecido), escala de confiança, bloco de saída estruturado dos agentes, estrutura do Working Context, orçamento de contexto por etapa e decaimento de informação. Use ao delegar a um subagente, ao consolidar saídas de vários agentes, ao decidir se há evidência suficiente para avançar, e sempre que um agente precise de declarar confiança ou resolver conflito entre fontes. Não use como fonte de convenções de código — isso é backend-skill e frontend-skill; não use em tarefa de um passo que não envolve delegação nem julgamento de evidência.
---

# Contrato do Harness — EmpregaNet

Regras de **como o trabalho circula entre agentes**: o que conta como verdade, o que conta como evidência,
quanto contexto cada etapa recebe e o que se descarta. As convenções do *código* vivem noutro lado
([`backend-skill`](../backend-skill/SKILL.md), [`frontend-skill`](../frontend-skill/SKILL.md)) — este ficheiro
não as repete.

Existe por um motivo concreto: sem uma fonte única, cada um dos oito agentes reinventaria a sua própria
escala de confiança e a sua própria ordem de fontes, e o orquestrador teria de reconciliar vocabulários
diferentes a cada passo. Uma definição, oito leitores.

---

## 1. Quando aplicar

| Situação | Aplicar |
| -------- | ------- |
| Delegar a um subagente ou consolidar a saída de vários | Sim |
| Declarar confiança, evidência ou assunção num output | Sim |
| Fontes em conflito (código vs ADR vs spec vs memória) | Sim |
| Decidir se há evidência suficiente para avançar de etapa | Sim |
| Convenção de camada, EF Core, SCSS, testes | Não — `backend-skill` / `frontend-skill` |
| Fluxo de fases PRD → design → spec | Não — [`sdd-orchestrator`](../sdd-orchestrator/SKILL.md) |
| Tarefa de um passo, sem delegação nem julgamento de evidência | Não — responder directamente |

## 2. Ligações

| Documento | Papel |
| --------- | ----- |
| [`meta-agent`](../meta-agent/SKILL.md) | Aplica este contrato ao rotear, sintetizar e replanear |
| [`sdd-orchestrator`](../sdd-orchestrator/SKILL.md) | Aplica-o nos gates de fase e na verificação contra `spec.md` |
| [`docs/agents/README.md`](../../../docs/agents/README.md) | Padrão de escrita dos agentes que consomem este contrato |

---

## 3. Hierarquia de fonte de verdade

Duas perguntas diferentes têm hierarquias diferentes. Confundi-las é a origem mais comum de conclusão
errada com confiança alta.

**Eixo A — "o que o sistema faz hoje?"** (depuração, QA, diagnóstico, revisão)

1. Execução observada — output de comando, log, resposta HTTP real, tela navegada
2. Código actual em `backend/src`, `Bff/`, `frontend/src`
3. Testes que passam hoje
4. Documentação (descreve a intenção, não garante o comportamento)

**Eixo B — "o que o sistema deve fazer?"** (implementação, desenho, revisão de conformidade)

1. ADRs em `docs/sdd/adrs/` — decisões estruturais duradouras
2. `design.md` / `spec.md` da feature activa
3. Skills de convenção (`backend-skill`, `frontend-skill`)
4. `prd.md` e documentos de governo em `docs/sdd/`
5. Memória de sessões anteriores (`memory/`) — contexto histórico, não norma
6. Inferência do próprio agente — sempre a última, sempre rotulada

**Quando os eixos divergem** — o código faz X, o ADR manda Y — isso **não** se funde nem se resolve em
silêncio: é um achado. Reportar como divergência, dizer qual eixo se está a seguir e porquê, e devolver
a decisão ao humano quando a correcção não for óbvia.

**Regra de idade:** um facto de `memory/` ou de um documento antigo descreve o que era verdade quando foi
escrito. Antes de agir sobre ele, confirmar contra o eixo A. Facto que envelheceu engana com confiança.

---

## 4. Classificação de afirmações

Toda afirmação que sustente uma decisão cai numa destas quatro categorias:

| Rótulo | Definição | Exige |
| ------ | --------- | ----- |
| **FACTO** | Verificado directamente nesta execução | Referência: `ficheiro:símbolo`, comando + output, ou observação na UI |
| **INFERÊNCIA** | Deduzido de factos, com passo lógico explícito | Nomear os factos de partida e o salto feito |
| **SUPOSIÇÃO** | Assumido por ausência de informação | Ficar explícito no output; nunca sustentar sozinho uma decisão irreversível |
| **DESCONHECIDO** | Não sabido e não deduzível com o contexto disponível | Nomear o que falta e como se obteria |

**Proibições absolutas:**

- Promover SUPOSIÇÃO ou INFERÊNCIA a FACTO por repetição, por plausibilidade ou por pressão de avançar.
- Apresentar DESCONHECIDO como ausência de problema ("não encontrei nada" ≠ "não existe").
- Descrever um comando como executado quando não foi — se não pôde correr no ambiente, é DESCONHECIDO
  com o comando exacto que o humano deve correr.

Vocabulário já existente no Harness que mapeia para aqui: o rótulo **suspeita** do
[`code-reviewer`](../../agents/code-reviewer.md) e do [`performance-optimizer`](../../agents/performance-optimizer.md)
é a expressão de domínio de uma INFERÊNCIA sem medição — manter o termo, é mais legível no relatório.

---

## 5. Escala de confiança

Critério objectivo, não sensação. O valor entra no bloco de saída (§6) e governa o gate (§7).

| Nível | Critérios — todos têm de valer |
| ----- | ------------------------------ |
| **HIGH** | Fonte primária lida directamente; validação executada com output real; cada critério de aceite verificado um a um; nenhuma contradição entre fontes |
| **MEDIUM** | Evidência parcial: inferência a partir de padrão vizinho, validação que não pôde correr no ambiente, ou um critério sem verificação directa. A incerteza está nomeada e delimitada |
| **LOW** | Informação insuficiente, fontes em conflito, hipótese não confirmada, ou uma SUPOSIÇÃO a sustentar a conclusão principal |

Um único critério de aceite por verificar baixa a confiança para MEDIUM, mesmo que tudo o resto esteja provado.
Confiança é do elo mais fraco, não da média.

---

## 6. Bloco de saída estruturado

Todo agente abre o seu output com este bloco, **antes** do relatório legível — que continua a ser a entrega
principal para o humano. O bloco existe para o orquestrador sintetizar sem reler prosa.

```yaml
confidence: HIGH | MEDIUM | LOW
evidence:
  - "backend/src/EmpregaNet.Application/Jobs/Commands/Create/CreateJobHandler.cs:48 — validação de posições lida"
  - "dotnet test backend/tests/tests.csproj — 134 passed, 0 failed"
assumptions:
  - "nenhuma"          # ou a suposição, com o que a confirmaria
open_questions:
  - "nenhuma"          # ou a pergunta que bloqueia, dirigida a quem a pode responder
blocked_by: null        # ou a causa nomeada, quando a etapa não pôde concluir
```

Regras do bloco:

- `evidence` aponta para **fonte verificável** — caminho com símbolo ou linha, comando com resultado, rota navegada.
  Frase de opinião não é evidência e não entra aqui.
- `assumptions` vazio significa *nenhuma suposição foi feita*, não *não verifiquei*. Na dúvida, escrever a suposição.
- `blocked_by` é o que alimenta o replaneamento com causa (§8) — texto específico, não "falhou".
- O bloco **não substitui** o `## Formato de saída` de cada agente; precede-o.

---

## 7. Working Context

O que circula entre etapas. Substitui "passar a conversa toda": o executor recebe isto e o pedido, não o
histórico da investigação que o produziu.

```text
TASK                  # uma frase: o que se está a fazer
GOAL                  # o resultado observável que encerra a tarefa
CONSTRAINTS           # Clean Architecture, mediator interno, YAGNI, RBAC, stack fechada
ACCEPTANCE_CRITERIA   # da spec.md quando existir; senão, derivados e confirmados com o humano
FACTS                 # §4, cada um com a sua referência
EVIDENCE              # comandos corridos e resultados
DECISIONS             # o que já foi decidido e não se reabre sem motivo novo
ASSUMPTIONS           # §4 — explícitas, nunca promovidas
OPEN_QUESTIONS        # o que ainda bloqueia ou condiciona
RISKS                 # o que pode correr mal e o sinal que o denuncia
RELEVANT_FILES        # caminhos — nunca conteúdo colado
RELEVANT_HISTORY      # só o que muda a decisão actual; o resto fica no transcript
```

Regras:

- **Nada entra sem justificar a presença.** Se remover a linha não muda nenhuma decisão do executor, a linha sai.
- **Caminhos, não conteúdo.** O agente tem `Read`; colar ficheiro no contexto é pagar duas vezes pela mesma informação.
- **Não cresce monotonicamente.** A cada síntese, o que foi superado sai: um `OPEN_QUESTIONS` respondido vira
  `FACTS` ou `DECISIONS`; um `RISKS` que não se materializou e já não se aplica desaparece.

---

## 8. Orçamento de contexto por etapa

A janela grande não é licença para a encher. Orçamento por papel:

| Etapa | Recebe | Nunca recebe |
| ----- | ------ | ------------ |
| **Planeamento** | Pedido do utilizador + Working Context anterior, se houver | A sessão inteira |
| **Recuperação** (`Explore`) | Uma pergunta delimitada | Contexto da execução que a motivou |
| **Execução** (implementer, engineer, test, debug, performance) | Task Brief + Working Context + caminhos + a sua skill | Relatórios brutos de agentes anteriores |
| **Síntese** | Só o relatório da etapa que acabou de correr | Relatórios já sintetizados |
| **Verificação** (reviewer, architect, e2e) | Diff/resultado + Working Context + critérios de aceite | A investigação que levou ao resultado |

**Independência do verificador:** o verificador não recebe o raciocínio do executor. Se receber, tende a
validar a narrativa em vez do artefacto — e o valor da segunda opinião desaparece.

---

## 9. Decaimento de contexto

| Camada | O que é | Onde vive hoje |
| ------ | ------- | -------------- |
| **HOT** | Directamente ligado à tarefa em curso | Working Context, `CLAUDE.md`, ficheiros em edição |
| **WARM** | Relevante ao domínio, carregado ao entrar nele | `backend-skill`, `frontend-skill`, este contrato |
| **COLD** | Recuperável sob demanda, nunca automático | ADRs, `docs/features/<id>/`, histórico Git |
| **ARCHIVED** | Só por busca explícita | Features encerradas, sessões antigas, relatórios de QA entregues no chat |

Disciplina de manutenção:

- `CLAUDE.md` é HOT permanente: cada linha acrescentada é paga em **todas** as tarefas futuras. Acrescentar
  só o que é sempre verdade e sempre relevante; o resto é WARM (skill) ou COLD (documento).
- `MEMORY.md` é índice, não memória — uma linha por entrada. Quando o índice crescer ao ponto de a maioria
  das entradas não ser relevante à tarefa típica, consolidar as relacionadas numa entrada mais densa.
- Um relatório de agente já sintetizado é COLD: referenciar pelo que ficou no Working Context, nunca recolar.

---

## 10. Anti-padrões

| Bloqueado | Porquê |
| --------- | ------ |
| Colar o conteúdo de um ficheiro no prompt de delegação | O agente tem `Read`; duplica o custo e desactualiza-se |
| Reencaminhar o relatório bruto de um agente para o seguinte | É o mecanismo pelo qual o contexto cresce sem limite |
| `confidence: HIGH` com validação que não correu | Confunde intenção com prova — o caso que mais alucinação produz |
| Reexecutar a mesma delegação sem mudar nada após falha | Retry sem causa é ruído; ver §8 do `meta-agent` |
| Resolver conflito entre fontes fundindo as duas versões | Produz um terceiro estado que não existe em lado nenhum |
| Criar um agente novo para um papel que um existente já cobre | Cada agente é superfície de manutenção; ver `docs/agents/README.md` |

---

## 11. Idioma

Português (Brasil); identificadores, rótulos do bloco estruturado (`confidence`, `evidence`, …) e níveis
(`HIGH`/`MEDIUM`/`LOW`) em inglês, por serem chaves de contrato.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 1.0.0 | Contrato inicial: hierarquia de fonte de verdade em dois eixos, classificação de afirmações, escala de confiança, bloco de saída estruturado, Working Context, orçamento por etapa e decaimento. Generaliza padrões que já existiam dispersos em `debug-specialist` (confiança declarada), `dotnet-architect` (assunções explícitas) e `code-reviewer`/`performance-optimizer` (rótulo *suspeita*) |
