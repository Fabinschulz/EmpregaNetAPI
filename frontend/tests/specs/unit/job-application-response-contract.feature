#language: pt

# `JobApplication` é raiz de agregado e conhece o candidato só por id, então a API resolve o nome
# do lado da leitura (`JobApplicationProjection`) — em todas as rotas, inclusive na resposta da
# troca de status. Estes cenários existem para que "todas" continue sendo todas.

Funcionalidade: Contrato de resposta de candidatura
  Como desenvolvedor do frontend
  Quero que o candidato seja obrigatório no contrato de leitura
  Para que a coluna "Candidato" nunca mais fique vazia sem ninguém perceber

  Cenário: ler uma candidatura com o candidato resolvido
    Quando eu leio a resposta de candidatura padrão
    Então a leitura da candidatura deve ter sucesso
    E o candidato lido deve se chamar "ana.souza"

  # Regressão: o contrato declarava `candidateId` opcional, a API enviava outra coisa, o parse
  # passava e a coluna exibia "-" em toda linha. Campo obrigatório transforma isso em erro.
  Cenário: resposta sem o candidato deve ser recusada
    Quando eu leio uma resposta de candidatura sem "candidate"
    Então a leitura da candidatura deve falhar no campo "candidate"

  Cenário: resposta com o antigo candidateId deve ser recusada
    Quando eu leio uma resposta de candidatura no formato antigo, com "candidateId"
    Então a leitura da candidatura deve falhar no campo "candidate"

  # O join com o usuário é LEFT de propósito: uma linha órfã aparece na listagem com nome vazio,
  # em vez de sumir dela. Vazio é um problema visível; ausente não era.
  Cenário: candidato sem nome cai para o identificador na célula
    Quando eu leio uma resposta de candidatura com o nome do candidato vazio
    Então a leitura da candidatura deve ter sucesso
    E o rótulo do candidato na célula deve ser "#5"

  Cenário: candidato com conta encerrada continua no histórico
    Quando eu leio uma resposta de candidatura de um candidato com a conta encerrada
    Então a leitura da candidatura deve ter sucesso
    E o candidato lido deve estar marcado como excluído

  Cenário: uma listagem não deve quebrar por causa de um candidato sem nome
    Quando eu leio uma listagem de candidaturas em que a segunda não tem nome de candidato
    Então a leitura da candidatura deve ter sucesso
    E a listagem de candidaturas lida deve conter 3 itens
