# language: pt
Funcionalidade: Variáveis públicas do ambiente
  Como sistema
  Quero tratar variável vazia como variável ausente
  Para que uma imagem construída sem Google Sign-In (ARG vazio) não derrube o login

  Cenário: client id do Google ausente é aceito
    Dado que NEXT_PUBLIC_GOOGLE_CLIENT_ID não está definida
    Quando eu leio o ambiente público
    Então a leitura deve passar
    E o client id do Google deve ser ausente

  Cenário: client id do Google vazio vale como ausente
    # O build do contêiner embute NEXT_PUBLIC_GOOGLE_CLIENT_ID="" quando o ARG não é informado.
    Dado que NEXT_PUBLIC_GOOGLE_CLIENT_ID está definida como ""
    Quando eu leio o ambiente público
    Então a leitura deve passar
    E o client id do Google deve ser ausente

  Cenário: client id do Google informado é preservado
    Dado que NEXT_PUBLIC_GOOGLE_CLIENT_ID está definida como "abc.apps.googleusercontent.com"
    Quando eu leio o ambiente público
    Então a leitura deve passar
    E o client id do Google deve ser "abc.apps.googleusercontent.com"

  Cenário: URL da API vazia continua valendo como mesma origem
    Dado que NEXT_PUBLIC_API_BASE_URL está definida como ""
    Quando eu leio o ambiente público
    Então a leitura deve passar

  Cenário: URL da API inválida continua sendo recusada
    Dado que NEXT_PUBLIC_API_BASE_URL está definida como "não-é-url"
    Quando eu leio o ambiente público
    Então a leitura deve falhar com "NEXT_PUBLIC_API_BASE_URL"
