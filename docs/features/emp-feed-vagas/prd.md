---
version: 1.0.0
date: 2026-08-02
---

# PRD — Feed de Vagas (`emp-feed-vagas`)

## 1. Problema e motivação

O catálogo público de vagas hoje é uma listagem paginada que mostra **apenas o título da vaga e um
botão "Ver detalhes"**. Para decidir se uma oportunidade interessa, o candidato precisa abrir cada
vaga individualmente e voltar — um ciclo de navegação por vaga.

Três falhas sistémicas decorrem disso:

1. **Custo cognitivo por oportunidade.** Nenhum dos critérios que um candidato usa para triar
   (salário, cidade, modalidade, senioridade, empresa) está visível na listagem. A triagem só
   acontece dentro da página de detalhe, uma vaga de cada vez.
2. **Descoberta pobre.** A única forma de reduzir o conjunto é uma busca textual por título ou
   descrição. Não há como responder "vagas remotas de desenvolvimento acima de R$ 6.000 publicadas
   esta semana" — nem combinando filtros, nem manualmente, porque a informação não é exibida.
3. **Resultados não são compartilháveis.** O estado da listagem vive em `localStorage`. Enviar uma
   busca a alguém, ou voltar a ela depois, exige refazer a interação à mão.

O produto compete por atenção com LinkedIn, Indeed e Glassdoor, onde a triagem acontece no próprio
fluxo de scroll. Enquanto a listagem exigir um clique por vaga para revelar informação básica, o
candidato desiste antes de chegar às vagas relevantes.

## 2. Personas e RBAC

| Persona | Acesso ao feed | Pode |
| ------- | -------------- | ---- |
| **Visitante anónimo** | Total (leitura) | Ver o feed, buscar, filtrar, ordenar, abrir o detalhe. Vê o número de candidatos por vaga. Não vê estado pessoal. |
| **Candidato autenticado** | Total (leitura) + candidatura | Tudo do anónimo, mais: ver quais vagas já se candidatou e candidatar-se a partir do feed. |
| **Recrutador / Gestor** | Total (leitura) | Vê o feed como qualquer utilizador. A gestão das próprias vagas continua em `/recrutamento/vagas` — o feed **não** ganha ações de gestão. |
| **Admin** | Total (leitura) | Idem recrutador. |

Regras de visibilidade herdadas e mantidas:

- O feed público mostra apenas vagas **ativas** e **não excluídas**. Perfis de recrutamento não
  ganham visibilidade extra *no feed* — para ver encerradas usam a tela de recrutamento.
- A candidatura exige sessão. Sem sessão, a ação convida a entrar em vez de falhar.

## 3. Workflows

### W1 — Descobrir vagas por scroll

O candidato abre `/vagas` e vê um feed de cartões ordenado por mais recentes. Cada cartão traz, sem
nenhum clique: empresa, cargo, cidade/UF, modalidade, tipo de vínculo, faixa salarial, senioridade,
tempo desde a publicação, resumo curto, principais tecnologias e benefícios. Ao aproximar-se do fim
da lista, o lote seguinte carrega sozinho.

### W2 — Buscar por palavra-chave

O candidato digita no campo de busca. Após uma pausa curta na digitação, o feed atualiza sozinho —
não há botão "Pesquisar". A busca cobre cargo, empresa, tecnologia, benefício, cidade e o texto da
vaga, e é insensível a acentos.

### W3 — Filtrar e combinar

O candidato aplica filtros de localização, modalidade, faixa salarial, tipo de vínculo, data de
publicação, área, senioridade, tecnologias, benefícios e empresa. Os filtros são **combináveis** e o
conjunto ativo fica visível como chips removíveis. No desktop o painel fica ao lado do feed; no
mobile abre como gaveta.

### W4 — Ordenar

O candidato escolhe entre mais recentes, maior salário, relevância (só faz sentido com busca ativa),
empresa (A–Z) e localização (A–Z).

### W5 — Retomar e compartilhar

Todo o estado (busca, filtros, ordenação) vive na URL. Recarregar a página, navegar para o detalhe e
voltar, ou enviar o link a outra pessoa reproduz exatamente o mesmo conjunto de resultados.

### W6 — Candidatar-se a partir do feed

O candidato autenticado envia a candidatura direto do cartão. O cartão passa a indicar "Candidatura
enviada" e a ação não se repete. O anónimo vê o convite para entrar.

## 4. Critérios de aceite

| # | Critério |
| - | -------- |
| CA-01 | O cartão exibe empresa, cargo, localização, modalidade, tipo de vínculo, faixa salarial, senioridade, tempo desde a publicação e resumo sem nenhuma interação. |
| CA-02 | Cargo e empresa têm o maior peso visual do cartão; informação secundária tem peso menor. |
| CA-03 | Ao chegar perto do fim da lista, o lote seguinte carrega automaticamente. Não existe controlo de paginação numerada no feed. |
| CA-04 | Enquanto um lote carrega, aparecem placeholders com a forma do cartão — a página não salta. |
| CA-05 | A busca dispara sozinha após pausa na digitação; digitar uma palavra não gera uma requisição por tecla. |
| CA-06 | A busca encontra por cargo, empresa, tecnologia, cidade e palavra do texto, ignorando acentos. |
| CA-07 | Filtros são combináveis; o resultado respeita a interseção de todos os filtros ativos. |
| CA-08 | Filtros com múltipla escolha (tecnologias, benefícios, modalidade…) aceitam vários valores, aplicados como "qualquer um dos selecionados". |
| CA-09 | Cada filtro ativo aparece como chip removível; remover o chip atualiza o feed. |
| CA-10 | Recarregar a página preserva busca, filtros e ordenação. |
| CA-11 | Abrir a URL noutro navegador reproduz o mesmo conjunto de resultados. |
| CA-12 | Os filtros de primeira ordem (turno, faixa salarial, experiência, área, benefícios, publicação e PcD) ficam em pills no topo, iguais em mobile e desktop; os restantes abrem na gaveta "Todos os filtros", disponível em qualquer largura. Os campos da gaveta são os mesmos do vocabulário usado pelas pills. |
| CA-13 | Sem resultados, o feed explica o motivo e oferece limpar os filtros. |
| CA-14 | Utilizador autenticado vê "Candidatura enviada" nas vagas a que já se candidatou; a ação de candidatar-se não fica disponível de novo. |
| CA-15 | Utilizador anónimo vê convite para entrar em vez da ação de candidatura, e o feed continua totalmente navegável sem sessão. |
| CA-16 | O cartão mostra o número de candidatos quando houver ao menos uma candidatura. |
| CA-17 | Todo o feed é operável só com teclado, incluindo abrir filtros, aplicar, remover chips e candidatar-se. |
| CA-18 | Leitor de tela anuncia o carregamento de novos lotes e a posição de cada vaga no conjunto. |
| CA-19 | Com `prefers-reduced-motion`, nenhuma animação de entrada ou hover é executada. |
| CA-20 | O HTML inicial de `/vagas` já contém a primeira página de vagas (indexável por buscadores). |
| CA-21 | Vagas existentes antes desta feature continuam visíveis no feed, com localização preenchida a partir do endereço da empresa. |

## 5. Non-goals

Explicitamente **fora** desta entrega:

- **Vaga salva / favoritos.** Exigiria uma entidade de relacionamento utilizador–vaga que não
  existe. Nenhum affordance de salvar aparece na UI — não se desenha botão para função inexistente.
- **Compatibilidade / match da vaga com o candidato.** Não há perfil de competências do candidato
  no modelo; qualquer percentagem exibida hoje seria inventada.
- **Alertas de vaga por e-mail** a partir de uma busca salva.
- **Recomendações personalizadas** ou ordenação por histórico do utilizador.
- **Upload de logo da empresa.** O contrato já prevê o campo, mas a UI usa avatar com iniciais
  enquanto não houver fluxo de upload.
- **Contagem por opção de filtro** ("Remoto (23)"). Depende de agregação adicional; fica para
  evolução.
- Alterações na tela de gestão `/recrutamento/vagas` além dos campos novos no formulário.

## 6. Dependências e restrições

- O feed depende de campos que **não existem** hoje no domínio (localização, modalidade, senioridade,
  área, faixa salarial, tecnologias, benefícios, resumo). Sem enriquecer o modelo, os filtros
  correspondentes não têm o que filtrar.
- Como consequência direta: o **formulário de publicação de vaga** precisa oferecer esses campos.
  Caso contrário nada os preenche e os filtros retornam sempre vazio.
- Vagas anteriores à feature ficam sem os campos novos. Mitigação obrigatória em CA-21 para
  localização; os demais campos ficam opcionais e a vaga continua listável sem eles.

## 7. Referências

- [`design.md`](design.md) — contratos, HTTP e infraestrutura
- [ADR 0006](../../sdd/adrs/0006-agregado-job-enriquecido.md) — enriquecimento do agregado `Job`
- [ADR 0007](../../sdd/adrs/0007-endpoint-de-feed-dedicado.md) — endpoint de feed dedicado
