#language: pt

Funcionalidade: Fluxo completo do formulário de vaga (leitura da API até reenvio)
  Como sistema
  Quero que a vaga lida da API volte para ela com os tipos certos
  Para que o backend não rejeite a criação/edição por tipo de dado incorreto

  Cenário: reabrir uma vaga cadastrada e reenviar sem alterações deve reproduzir os mesmos dados
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 3,
        "title": "Operador(a) de Empilhadeira",
        "summary": "Centro de distribuição, 2º turno, com fretado.",
        "description": "Movimentação de cargas e conferência no armazém.",
        "companyId": 42,
        "salaryMin": 2300,
        "salaryMax": 2800,
        "salaryDisclosed": true,
        "jobType": "Clt",
        "workModel": "OnSite",
        "workShift": "SegundoTurno",
        "experienceLevel": "AteUmAno",
        "area": "Logistica",
        "isPcdFriendly": false,
        "city": "Extrema",
        "state": "MG",
        "country": "BR",
        "requirements": ["Ensino Médio completo", "Empilhadeira"],
        "benefits": ["Fretado", "Cesta Básica"],
        "isActive": true
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada
    Então o payload de reenvio da vaga deve conter:
      | campo           | valor                       |
      | companyId       | 42                          |
      | title           | Operador(a) de Empilhadeira |
      | jobType         | Clt                         |
      | workModel       | OnSite                      |
      | workShift       | SegundoTurno                |
      | experienceLevel | AteUmAno                    |
      | area            | Logistica                   |
      | city            | Extrema                     |
      | state           | MG                          |
      | salaryMin       | 2300                        |
      | salaryMax       | 2800                        |
      | isPcdFriendly   | false                       |
    E o campo "companyId" do payload de reenvio deve ser do tipo número
    E o campo "salaryMin" do payload de reenvio deve ser do tipo número
    E o campo "salaryMax" do payload de reenvio deve ser do tipo número
    E o payload de reenvio da vaga deve satisfazer o contrato de requisição

  # A API registra StringEnumConverter (ResponseJsonConfig), então enum sempre chega como nome.
  # O contrato aceitava `string | number` "por precaução" e, com isso, deixava passar em silêncio
  # exatamente o drift que ele existe para denunciar. A normalização por índice continua viva no
  # carregamento do formulário e tem cobertura unitária em job-vocabulary.feature e
  # brazilian-uf-and-activity.feature — o que não pode voltar é ela virar contrato de leitura.
  Cenário: enum devolvido como índice numérico deve ser recusado pelo contrato de leitura
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 4,
        "title": "Ajudante de Produção",
        "jobType": 3
      }
      """
    Quando eu valido esses dados contra o contrato de leitura de vaga
    Então a validação do contrato de vaga deve falhar no campo "jobType"

  # Vaga anterior ao redesenho: gravada antes de turno/experiência/área existirem, o enum ficou
  # em NaoSelecionado. O contrato de leitura tem de aceitá-la — ela existe no banco e a API a
  # serializa assim —, mas o formulário não pode deixá-la ser reenviada incompleta, senão ela
  # some dos filtros do feed em silêncio.
  Cenário: vaga legada sem turno é lida, mas barrada no reenvio
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 8,
        "title": "Vaga antiga",
        "description": "Publicada antes dos campos novos.",
        "companyId": 7,
        "salaryMin": 2100,
        "jobType": "Clt",
        "workShift": "NaoSelecionado",
        "experienceLevel": "NaoSelecionado",
        "area": "NaoSelecionado",
        "isActive": true
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu valido os dados carregados no formulário de vaga
    Então os dados carregados devem ser rejeitados pelo formulário

  Cenário: vaga afirmativa preserva a marcação no reenvio
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 10,
        "title": "Auxiliar Administrativo",
        "description": "Rotinas de apoio ao setor.",
        "companyId": 7,
        "salaryMin": 2000,
        "salaryDisclosed": true,
        "jobType": "Clt",
        "workModel": "OnSite",
        "workShift": "Administrativo",
        "experienceLevel": "SemExperiencia",
        "area": "Administrativo",
        "isPcdFriendly": true,
        "city": "Extrema",
        "state": "MG",
        "isActive": true
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada
    Então o payload de reenvio da vaga deve conter:
      | campo         | valor |
      | isPcdFriendly | true  |
    E o payload de reenvio da vaga deve satisfazer o contrato de requisição

  Cenário: salário a combinar não envia faixa no payload
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 9,
        "title": "Motorista Carreteiro",
        "description": "Rotas regionais.",
        "companyId": 7,
        "salaryDisclosed": false,
        "jobType": "Clt",
        "workModel": "OnSite",
        "workShift": "Escala6x1",
        "experienceLevel": "DeTresACincoAnos",
        "area": "Transporte",
        "city": "Extrema",
        "state": "MG",
        "isActive": true
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada
    Então o campo "salaryMin" do payload de reenvio deve estar ausente
    E o campo "salaryMax" do payload de reenvio deve estar ausente
    E o payload de reenvio da vaga deve satisfazer o contrato de requisição

  # Regressão: enquanto isActive era opcional no contrato, a API não o expunha, o parse
  # passava com undefined e a UI mostrava "Ativa" até para vaga encerrada. O campo
  # obrigatório faz esse drift falhar no parse em vez de virar um badge errado.
  Cenário: resposta de vaga sem isActive deve ser rejeitada pelo contrato de leitura
    Dado que a API devolveu esta resposta de vaga, exatamente como está:
      """
      {
        "id": 5,
        "title": "Analista de Qualidade",
        "description": "Vaga sem o campo de situação.",
        "companyId": 7,
        "salaryMin": 3500,
        "jobType": "Clt"
      }
      """
    Quando eu valido esses dados contra o contrato de leitura de vaga
    Então a validação do contrato de vaga deve falhar no campo "isActive"

  Cenário: vaga encerrada deve ser rotulada como Encerrada
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 6,
        "title": "Vaga já preenchida",
        "description": "Encerrada pela empresa.",
        "companyId": 7,
        "salaryMin": 2200,
        "jobType": "Clt",
        "isActive": false
      }
      """
    Então o rótulo de situação da vaga deve ser "Encerrada"

  Cenário: vaga aberta deve ser rotulada como Ativa
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 7,
        "title": "Vaga aberta",
        "description": "Recebendo candidaturas.",
        "companyId": 7,
        "salaryMin": 2200,
        "jobType": "Clt",
        "isActive": true
      }
      """
    Então o rótulo de situação da vaga deve ser "Ativa"
