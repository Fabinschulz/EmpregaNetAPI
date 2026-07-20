#language: pt

Funcionalidade: Fluxo completo do formulário de vaga (leitura da API até reenvio)
  Como sistema
  Quero que companyId e salary cheguem à API como números, não como texto
  Para que o backend não rejeite a criação/edição de vaga por tipo de dado incorreto

  Cenário: reabrir uma vaga cadastrada e reenviar sem alterações deve reproduzir os mesmos dados como números
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 3,
        "title": "Desenvolvedor(a) Full Stack",
        "description": "Vaga para atuar no time de plataforma.",
        "companyId": 42,
        "salary": 5000,
        "jobType": "FullTime"
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada
    Então o payload de reenvio da vaga deve conter:
      | campo       | valor                                   |
      | companyId   | 42                                      |
      | title       | Desenvolvedor(a) Full Stack             |
      | description | Vaga para atuar no time de plataforma.  |
      | jobType     | FullTime                                |
      | salary      | 5000                                    |
    E o campo "companyId" do payload de reenvio deve ser do tipo número
    E o campo "salary" do payload de reenvio deve ser do tipo número

  Cenário: tipo de vaga devolvido pela API como índice numérico do enum deve virar o nome do enum no reenvio
    # Índice 3 do enum JobTypeEnum do backend = Internship (0=NaoSelecionado, 1=FullTime, 2=PartTime, 3=Internship...)
    Dado que a API devolveu os dados desta vaga cadastrada:
      """
      {
        "id": 4,
        "title": "Estagiário(a) de Recrutamento",
        "description": "Vaga para o time de RH.",
        "companyId": 10,
        "salary": 3000,
        "jobType": 3
      }
      """
    Quando eu carrego esses dados no formulário de edição de vaga
    E eu monto o payload de reenvio da vaga a partir do formulário, sem alterar nada
    Então o payload de reenvio da vaga deve conter:
      | campo   | valor      |
      | jobType | Internship |
