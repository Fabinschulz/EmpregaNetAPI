---
name: e2e-qa-skill
description: >-
  Metodologia completa para regressão E2E (End-to-End) navegando a aplicação real via Browser tool:
  mapeamento de módulos/rotas, matriz de cenários priorizada, execução incremental com validação
  efetiva de resultado (não só "a página carregou"), templates de bug e de relatório final.
  Use ao planejar ou executar uma regressão E2E, validar um fluxo crítico ponta-a-ponta ou investigar
  um bug reproduzindo-o na interface.
---

# QA End-to-End (E2E) — monorepo EmpregaNet

Processo reutilizável para testar a aplicação **como usuário real**, através da interface visual — não pela leitura de código. Complementa, não substitui, os testes unitários/integração e os cenários BDD já existentes ([`docs/skills/frontend-skill/SKILL.md`](../frontend-skill/SKILL.md) §10).

---

## 1. Quando aplicar

| Situação | Usar esta skill |
| -------- | ---------------- |
| Regressão completa antes de release | Sim |
| Validar uma feature nova ponta-a-ponta (acesso → operação → confirmação) | Sim |
| Reproduzir um bug relatado, navegando o fluxo real | Sim |
| Escrever/rodar teste unitário ou de integração | Não — `docs/agents/test-engineer.md` |
| Revisar apenas um diff/PR | Não — `docs/agents/code-reviewer.md` |

---

## 2. Ligações obrigatórias

| Recurso | Path |
| ------- | ---- |
| Perfil do agente (comportamento, ferramentas, tom) | [`docs/agents/e2e-qa-engineer.md`](../../agents/e2e-qa-engineer.md) |
| Convenções de frontend (rotas, Server/Client, auth/RBAC) | [`docs/skills/frontend-skill/SKILL.md`](../frontend-skill/SKILL.md) |
| Infra de testes automatizados já existente | [`docs/agents/test-engineer.md`](../../agents/test-engineer.md) |
| Mapa do monorepo e comandos de build | [`docs/README.md`](../../README.md) |

---

## 3. Pré-requisitos de ambiente

| Serviço | Comando | Porta | Observação |
| ------- | ------- | ----- | ---------- |
| Frontend | `pnpm dev` (em `frontend/`) | `3000` | Use `preview_start({ name: "frontend" })` — config em `.claude/launch.json` |
| API (.NET) | conforme `docs/README.md` | `5225` | Precisa estar no ar para qualquer fluxo autenticado ou com dado real; `frontend/.env.example` define `NEXT_PUBLIC_API_BASE_URL` |

Se a API não estiver acessível, **não** prossiga assumindo comportamento — pare e registre isso como risco/bloqueio no relatório final (§10). Testar só o frontend contra uma API fora do ar produz falsos positivos e negativos.

Se a porta `3000` já estiver em uso por um processo fora do preview (o usuário costuma já ter o dev server rodando em outro terminal), **não mate o processo** — anexe diretamente com `preview_start({ url: "http://localhost:3000" })` em vez de `{ name: "frontend" }`.

---

## 4. Ferramentas de Browser e quando usar cada uma

| Ferramenta | Quando usar |
| ---------- | ----------- |
| `preview_start` / `navigate` | Abrir a app e trocar de rota |
| `computer screenshot` | Estado visual antes de agir e como evidência de falha |
| `computer left_click` / `type` / `scroll` / `key` / `hover` | Interação real (clique, digitação, navegação por teclado, hover em tooltip) |
| `read_page` + `find` | Localizar elemento por `ref_N` quando coordenadas de screenshot não bastam |
| `form_input` | Preencher select/checkbox/campos complexos com precisão |
| `get_page_text` | Confirmar texto exato de mensagem, toast, label, contagem de resultados |
| `read_console_messages` | Erros JS silenciosos — sempre checar após uma ação relevante |
| `read_network_requests` | Status HTTP real da chamada — um toast genérico pode esconder um 500 |
| `resize_window` | Responsividade (`mobile`/`tablet`/`desktop`); recarregar após trocar preset |

---

## 4.1 Cadência de checagem

Em toda ação que dispara uma chamada de rede (submit, exclusão, filtro), confira **depois** de `computer screenshot`: `read_console_messages` (erros novos) e, se o feedback visual for ambíguo, `read_network_requests` para o status real. Um teste que só olha a tela pode aprovar um fluxo que na verdade recebeu 500 e mostrou um toast genérico de erro incorreto.

---

## 5. Fase 1 — Mapeamento

Antes de qualquer execução, percorra a árvore de rotas real (`frontend/src/app/`) e liste módulos + fluxos críticos. Ponto de partida conhecido (confirme — o mapa evolui):

| Grupo | Rotas | Natureza |
| ----- | ----- | -------- |
| `(auth)` | login, register, forgot-password, reset-password, confirm-email, resend-confirmation | Público, sem sessão |
| `(public)` | `/vagas`, `/vagas/[id]` | Público, SSR, indexável |
| `(main)` | dashboard, conta/perfil, conta/seguranca, candidaturas, recrutamento/vagas, recrutamento/candidatos, recrutamento/candidaturas, admin/usuarios, admin/empresas | Autenticado, RBAC por papel |
| `(status)` | nao-autorizado | Redirecionamento de guard |

Para cada rota, identifique: exige autenticação? exige papel específico? é CRUD? tem formulário? tem filtro/busca/paginação?

---

## 6. Fase 2 — Matriz de cenários

Formato mínimo por cenário:

| Campo | Descrição |
| ----- | --------- |
| ID | `E2E-<módulo>-<sequencial>` (ex.: `E2E-VAGAS-003`) |
| Módulo | Feature/rota |
| Cenário | Frase curta do que é testado |
| Tipo | Positivo / Negativo / Extremo |
| Prioridade | Crítica / Alta / Média / Baixa (§7) |
| Pré-condição | Sessão, papel, dado existente necessário |
| Passos | Sequência de ações na UI |
| Resultado esperado | O que deve ser observável na tela/rede ao final |

Cubra, por módulo aplicável:

- **CRUD**: criação, consulta (lista + detalhe), edição, exclusão — incluindo confirmação de exclusão e o item sumindo da lista depois.
- **Formulários**: campos obrigatórios, máscaras (telefone, documentos), validações client-side vs. mensagens vindas da API, submit duplicado (double-click/debounce).
- **Filtros/busca/paginação/ordenação**: resultado correto, estado vazio (zero resultados), navegação entre páginas, combinação de múltiplos filtros.
- **Navegação/RBAC**: redirecionamento de `(status)/nao-autorizado` para papel sem permissão, menus condicionais coerentes com o papel real do backend (não só escondidos no cliente).
- **Feedback visual**: loading (`LoadingState`/skeletons/`Spinner`), toast de sucesso/erro, modal de confirmação, `FormSubmitButton` desabilitando durante envio.
- **Responsividade**: ao menos os fluxos críticos em `mobile` e `desktop`.

---

## 7. Fase 3 — Priorização

| Critério | Peso |
| -------- | ---- |
| Fluxo gera receita ou é pré-requisito de outro fluxo (login, cadastro de vaga, candidatura) | Alto |
| Exposto publicamente sem autenticação (feed de vagas) | Alto |
| Envolve RBAC/dados sensíveis (admin, dados de candidato) | Alto |
| Alta frequência de uso pelo usuário final | Médio |
| Tela de configuração pouco acessada | Baixo |

Execute **Crítica → Alta → Média → Baixa**; se o tempo/ambiente limitar, documente em "Fluxos ainda não testados" (§9) — nunca omita silenciosamente.

---

## 8. Fase 4 — Execução

- **Incremental**: um cenário por vez; não acumule várias interações não relacionadas antes de validar.
- **Validação efetiva**: confira o dado/estado real (texto na tela via `get_page_text`, item na lista, status de rede) — carregamento de página sozinho não aprova o cenário.
- **Não ignore ruído**: um erro no console ou um 4xx/5xx que não impediu a navegação ainda é um bug — registre.
- **Evidência em toda falha**: `screenshot` ou `zoom` na região relevante antes de seguir para o próximo cenário.
- **Dados de teste**: prefira contas/dados já reservados para teste; se precisar criar, marque-os (`[QA]` no nome) para facilitar limpeza; nunca use dados reais de produção.
- **Ações irreversíveis**: siga as regras gerais de segurança da sessão — confirme com o usuário antes de excluir/alterar algo que não seja um dado de teste criado por você.

---

## 9. Severidade e reprodutibilidade

| Severidade | Critério |
| ---------- | -------- |
| **Bloqueante** | Impede o fluxo crítico de ser concluído (não é possível logar, não é possível candidatar-se) |
| **Crítica** | Fluxo conclui mas com dado incorreto/perdido, ou falha de segurança/RBAC |
| **Alta** | Funcionalidade quebrada mas com contorno, ou erro visível ao usuário sem explicação |
| **Média** | Comportamento incorreto sem bloquear o fluxo (mensagem errada, estado visual inconsistente) |
| **Baixa** | Cosmético, UX subótima, sem impacto funcional |

Reprodutibilidade: **Sempre** / **Intermitente** / **Uma vez** (registre os passos exatos mesmo quando intermitente).

Template de registro de defeito: ver [`docs/agents/e2e-qa-engineer.md`](../../agents/e2e-qa-engineer.md) §"Registro de defeitos".

---

## 10. Relatório final

Estrutura obrigatória ao final da execução:

1. **Resumo executivo** — total de cenários executados, aprovados, reprovados; recomendação final: **apto** / **apto com ressalvas** / **não apto** para release, com justificativa de 1–2 frases.
2. **Cenários executados** — tabela ID / Módulo / Resultado (Aprovado/Reprovado/Bloqueado).
3. **Bugs encontrados** — ordenados por severidade, no template de defeito.
4. **Fluxos ainda não testados** — o que ficou de fora e por quê (tempo, ambiente, dado indisponível).
5. **Riscos identificados** — o que a execução não conseguiu garantir (ex.: API instável durante o teste, dependência externa fora do ar).
6. **Evidências** — referência às capturas de tela relevantes, associadas ao ID do cenário/bug.

---

## 11. Fora de escopo

- Testes unitários/integração automatizados — `docs/agents/test-engineer.md`.
- Revisão estática de diff — `docs/agents/code-reviewer.md`.
- Pentest/segurança ofensiva, bypass de CAPTCHA, ataques de força bruta — fora do escopo desta skill.
- Alterar configurações de segurança, permissões ou dados de produção sem autorização explícita do usuário.

---

## 12. Idioma

Relatório e comunicação em português (Brasil); IDs de cenário e nomes técnicos em inglês/formato curto quando fizer sentido (ex.: `E2E-VAGAS-003`).

---

## Histórico versão skill

| Versão | Mudança |
| ------ | ------- |
| 1.0.0 | Criação inicial da metodologia E2E via Browser tool |
