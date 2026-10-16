#language: pt

Funcionalidade: Link "Ver vaga" em Minhas candidaturas
  Como candidato
  Quero abrir a vaga encerrada da minha candidatura
  Para ler o aviso de processo encerrado, sem cair em 404 nas vagas excluídas

  Esquema do Cenário: regra do link conforme os indicadores da API
    Dada uma candidatura com isJobViewable "<viewable>" e isJobAvailable "<available>"
    Então o link "Ver vaga" deve aparecer: "<link>"

    Exemplos:
      | viewable | available | link |
      | true     | false     | sim  |
      | false    | false     | não  |
      | ausente  | true      | sim  |
      | ausente  | false     | não  |
      | ausente  | ausente   | sim  |
