---
name: meta-agent
description: Roteia um pedido de desenvolvimento para o especialista certo do EmpregaNet e encadeia trabalho multipasso, delegando via Agent tool e devolvendo uma resposta consolidada. Use quando o pedido for vago, amplo, ou misturar preocupações (desenho + implementação + testes + performance), ou quando o utilizador pedir explicitamente o melhor encaixe de expertise. Não use quando o domínio já é óbvio e estreito — nesse caso invoque o agent diretamente, sem passar por aqui.
---

# Roteador de especialistas — EmpregaNet

Orquestra: decide o caminho de especialista **mais curto e efectivo**, delega via **Agent tool**, sintetiza as
saídas e decide se avança. Vive como skill (não como agent) por um motivo funcional: a orquestração precisa da
Agent tool, disponível na thread principal e não dentro de um subagent.

O contrato de circulação de informação — hierarquia de fonte de verdade, escala de confiança, bloco de saída
dos agentes, Working Context, orçamento por etapa — está na [`harness-contract`](../harness-contract/SKILL.md).
Esta skill **aplica-o**; não o repete.

---

## 1. Quando aplicar

| Situação | Aplicar |
| -------- | ------- |
| Pedido vago, amplo, ou que mistura desenho + código + testes + performance | Sim |
| Utilizador quer o "melhor encaixe" de expertise ou um resultado multipasso | Sim |
| Pedido estreito e claramente de um domínio ("rever só este diff") | Não — invocar o agent directamente |
| Feature nova com contrato a fixar antes de código | Não — [`sdd-orchestrator`](../sdd-orchestrator/SKILL.md) |
| Tarefa de uma frase que já se resolve sem delegar | Não — responder directamente |

Encaminhar **não** é obrigatório: se um único agent basta, delegar uma vez e parar.

---

## 2. Tabela de roteamento

| Preocupação | Especialista | Gatilhos típicos |
| ----------- | ------------ | ---------------- |
| Localizar código ou responder "onde vive X?" antes de rotear | agent `Explore` (read-only) | alvo do trabalho ainda não identificado |
| Arquitectura, layering, forma da API, estrutura greenfield | agent [`dotnet-architect`](../../agents/dotnet-architect.md) | "como estruturar isto?", refactor que muda fronteiras |
| Código .NET concreto (handlers, EF, endpoints) | agent [`dotnet-implementer`](../../agents/dotnet-implementer.md) | implementar, ligar, migrar |
| UI Next.js/React (componentes, estado, a11y) | agent [`frontend-engineer`](../../agents/frontend-engineer.md) | telas, estilos, comportamento no cliente |
| Qualidade de PR/diff, smells, pronto-para-merge | agent [`code-reviewer`](../../agents/code-reviewer.md) | diffs, revisão pré-merge |
| Testes automatizados (unit, integração, Cucumber) | agent [`test-engineer`](../../agents/test-engineer.md) | "adicionar testes", teste instável, lacuna de cobertura |
| Bug, regressão, causa raiz | agent [`debug-specialist`](../../agents/debug-specialist.md) | stack trace, teste a falhar, comportamento errado |
| Performance, gargalo, escala | agent [`performance-optimizer`](../../agents/performance-optimizer.md) | endpoint lento, memória, plano de query |
| Validar o comportamento real navegando a UI | skill [`e2e-qa-skill`](../e2e-qa-skill/SKILL.md) → agent `e2e-qa-engineer` | "testa o frontend", "roda regressão", reproduzir bug na tela |
| Feature nova com spec formal antes de código | skill [`sdd-orchestrator`](../sdd-orchestrator/SKILL.md) | "PRD primeiro", incremento grande |

---

## 3. Regras de sobreposição

- **Especificar → construir:** `sdd-orchestrator` até artefactos aprovados; depois `dotnet-architect` → `dotnet-implementer` e/ou `frontend-engineer`.
- **Desenhar → construir:** `dotnet-architect` → `dotnet-implementer` só quando a arquitectura ainda não está decidida. Se já estiver, saltar o arquitecto.
- **Construir → verificar:** `dotnet-implementer` ou `frontend-engineer` → `test-engineer` quando faltarem testes para comportamento novo.
- **Construir → validar na UI:** mudança em `frontend/src/app/**` ou `frontend/src/features/**` → `e2e-qa-skill` antes de dar a tarefa por concluída.
- **Implementação + revisão:** `dotnet-implementer` → `code-reviewer` quando pedirem implementação **e** passagem de qualidade.
- **Performance vs bug:** se a corretude estiver em dúvida, `debug-specialist` primeiro; `performance-optimizer` só quando o problema é claramente latência/throughput/recursos.
- **Suspeita de performance levantada numa revisão:** `code-reviewer` marca como suspeita e encaminha; não é o `code-reviewer` que afina.

---

## 4. Processo — o ciclo

```text
PLANEAR → RECUPERAR → EXECUTAR → SINTETIZAR → VERIFICAR → DECIDIR
                ↑                                            │
                └────────── replanear com causa ─────────────┘
```

Etapas sem trabalho a fazer saltam-se: pedido claro não precisa de RECUPERAR, mudança trivial não precisa de
VERIFICAR por agente. O ciclo é a ordem, não uma quota de passos.

### 4.1 Planear

Decompor em passos ordenados, **um** especialista principal por passo, e escrever um **Task Brief** curto antes
de delegar — ele é a entrada do executor e a base do Working Context:

```text
OBJECTIVO        # o resultado observável que encerra a tarefa
ESCOPO           # o que entra e, explicitamente, o que fica de fora
RESTRIÇÕES       # arquitectura, stack fechada, RBAC, YAGNI, gate SDD
CRITÉRIOS        # como se sabe que está feito — verificáveis
FALTA SABER      # o que precisa de resposta antes de executar
NÃO É PRECISO    # informação disponível que este passo não deve arrastar
```

A linha **NÃO É PRECISO** não é decoração: é onde se corta o contexto que existiria por inércia
("toda a conversa anterior", "o relatório do arquitecto", "os outros módulos da feature").

### 4.2 Recuperar

Só quando o alvo do trabalho não está identificado. Delegar ao agent `Explore` uma pergunta delimitada;
o retorno é **lista de caminhos**, não conteúdo. Cada especialista faz a sua própria leitura profunda dentro
do seu contexto isolado — recuperar aqui é decidir *para onde apontá-lo*, não pré-digerir o trabalho dele.

### 4.3 Executar

Executar a cadeia mínima — sem agents extra "por cobertura". Passos independentes correm em paralelo numa só
mensagem; passos dependentes esperam.

O prompt de delegação leva **apenas**: Task Brief, Working Context, caminhos relevantes, e a skill a ler.

Nunca leva: o histórico da conversa, relatórios brutos de agentes anteriores, convenções recopiadas
(cada agent carrega a sua skill), nem o raciocínio de outro agente sobre o mesmo artefacto.

### 4.4 Sintetizar

Depois de cada passo, **extrair — não concatenar**. Do relatório do agente sai a actualização do Working Context:

1. Ler o **bloco de contrato** do agente (`confidence` / `evidence` / `assumptions` / `open_questions` / `blocked_by`).
2. Promover a `FACTS`/`DECISIONS` o que tem evidência; manter `ASSUMPTIONS` como suposição — nunca promover.
3. Remover o que foi superado: pergunta respondida sai de `OPEN_QUESTIONS`; risco que não se aplica sai.
4. Resolver conflitos pela hierarquia da `harness-contract` — e quando o conflito é entre eixos
   (o código faz X, o ADR manda Y), registá-lo como achado em vez de escolher em silêncio.
5. **Descartar o texto bruto.** A partir daqui o relatório é referenciado, não recolado.

Sem este passo, cada delegação seguinte herda tudo o que veio antes — que é exactamente como a janela degrada.

### 4.5 Verificar

O verificador (`code-reviewer`, `dotnet-architect`, `e2e-qa-skill`) recebe o artefacto, o Working Context e os
critérios de aceite — **não** recebe o raciocínio de quem executou. Verificador que lê a narrativa do executor
tende a validar a narrativa.

### 4.6 Decidir

Aplicar o gate (§5) e fundir numa resposta única, deduplicada: o utilizador recebe o resultado integrado,
nunca os handoffs em bruto.

---

## 5. Gate de confiança e replaneamento

Decisão baseada no `confidence` do bloco de contrato e no resultado da verificação:

| Situação | Decisão |
| -------- | ------- |
| `HIGH`, sem bloqueante | Avançar ou finalizar |
| `MEDIUM` | Avançar **nomeando** a incerteza no output; se ela toca custo assimétrico (contrato HTTP, migration destrutiva, autorização, captura de dados), tratar como `LOW` |
| `LOW`, ou verificação reprovada, ou `blocked_by` preenchido | **Não avançar.** Recuperar a evidência em falta → verificar → replanear |

**Replanear é diferente de repetir.** Cada nova tentativa corrige uma causa nomeada:

1. Extrair a causa concreta de `blocked_by` ou do achado da verificação.
2. Recuperar só o que falta (`Explore`, ficheiro específico, ADR, ou pergunta ao humano).
3. Actualizar o Task Brief com a correcção — e devolver **só o delta** ao executor, não a tarefa inteira.
4. Reexecutar apenas o passo afectado.

Reenviar o mesmo prompt após falha é proibido: sem causa identificada, a segunda tentativa tem a mesma
probabilidade de falhar que a primeira, e o dobro do contexto.

**Decisão que é do humano continua a ser do humano:** conflito entre fontes sem resolução óbvia, custo
assimétrico em zona cinzenta, ou `LOW` que persiste depois de recuperar — parar e perguntar, não insistir.

---

## 6. Regras

- Não substituir um especialista por conselho genérico quando a delegação melhoraria materialmente o resultado.
- Não empilhar agents em tarefas de uma frase.
- Não delegar duas vezes a mesma pergunta a agents diferentes para "comparar".
- Nunca apresentar handoffs em bruto: o utilizador recebe o resultado integrado.
- Não carregar o Working Context com informação que não muda nenhuma decisão do passo seguinte.
- Em sessão longa e multi-fase, marcar transição de fase (capítulo) em vez de deixar a sessão crescer como bloco único.

---

## 7. Formato de saída

1. **Roteamento** — uma linha: qual/quais especialistas e porquê. Omitir se for handoff trivial de um só agent.
2. **Resultado** — a entrega principal, já fundida e deduplicada.
3. **Confiança** — o nível agregado (o do elo mais fraco) e, quando não for `HIGH`, a incerteza concreta.
4. **Notas** — só trade-offs, riscos ou próximos passos não óbvios; poucos bullets.

---

## 8. Idioma

Português (Brasil).

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 3.0.0 | Ciclo explícito PLANEAR → RECUPERAR → EXECUTAR → SINTETIZAR → VERIFICAR → DECIDIR. Separa o planeamento (Task Brief, com a linha "não é preciso") da síntese (extracção para Working Context e descarte do texto bruto), que antes estavam fundidos num passo de "fundir saídas". Acrescenta `Explore` como recuperação de pré-rota, gate de confiança com replaneamento por causa nomeada, independência do verificador, e a proibição de reenviar o mesmo prompt após falha |
| 2.0.0 | Convertido de agent para skill: como agent não tinha acesso à Agent tool e só podia recomendar, não delegar. Tabela de roteamento completada com `e2e-qa-skill` (antes ausente) e com a regra de validar na UI após mudança de frontend |
| 1.0.0 | Versão agent (`docs/agents/meta-agent.md`) |
