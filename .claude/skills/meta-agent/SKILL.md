---
name: meta-agent
description: Roteia um pedido de desenvolvimento para o especialista certo do EmpregaNet e encadeia trabalho multipasso, delegando via Agent tool e devolvendo uma resposta consolidada. Use quando o pedido for vago, amplo, ou misturar preocupações (desenho + implementação + testes + performance), ou quando o utilizador pedir explicitamente o melhor encaixe de expertise. Não use quando o domínio já é óbvio e estreito — nesse caso invoque o agent diretamente, sem passar por aqui.
---

# Roteador de especialistas — EmpregaNet

Orquestra: decide o caminho de especialista **mais curto e efectivo**, delega via **Agent tool**, funde as saídas
numa resposta coesa. Vive como skill (não como agent) por um motivo funcional: a orquestração precisa da
Agent tool, disponível na thread principal e não dentro de um subagent.

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

## 4. Processo

1. **Decompor** o pedido em passos ordenados; cada passo tem **um** especialista principal.
2. **Executar a cadeia mínima** — sem agents extra "por cobertura". Passos independentes podem correr em paralelo numa só mensagem; passos dependentes esperam.
3. **Passar contexto por referência**, não por cópia: indicar ficheiros, `docs/features/<id>/` e a skill que o agent deve ler. Não recopiar convenções no prompt — cada agent já carrega a sua skill.
4. **Fundir** as saídas numa resposta única: remover duplicação; resolver contradições em favor do especialista cujo domínio corresponde ao conflito.

---

## 5. Regras

- Não substituir um especialista por conselho genérico quando a delegação melhoraria materialmente o resultado.
- Não empilhar agents em tarefas de uma frase.
- Não delegar duas vezes a mesma pergunta a agents diferentes para "comparar".
- Nunca apresentar handoffs em bruto: o utilizador recebe o resultado integrado.

---

## 6. Formato de saída

1. **Roteamento** — uma linha: qual/quais especialistas e porquê. Omitir se for handoff trivial de um só agent.
2. **Resultado** — a entrega principal, já fundida e deduplicada.
3. **Notas** — só trade-offs, riscos ou próximos passos não óbvios; poucos bullets.

---

## 7. Idioma

Português (Brasil).

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 2.0.0 | Convertido de agent para skill: como agent não tinha acesso à Agent tool e só podia recomendar, não delegar. Tabela de roteamento completada com `e2e-qa-skill` (antes ausente) e com a regra de validar na UI após mudança de frontend |
| 1.0.0 | Versão agent (`docs/agents/meta-agent.md`) |
