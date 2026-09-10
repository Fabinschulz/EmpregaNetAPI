#language: pt

# PRD §1.2: ao candidatar-se, a interface dizia "A empresa foi notificada da sua candidatura" e
# nenhuma notificação era disparada — promessa falsa, e um dos quatro problemas que motivaram esta
# feature. A notificação que o Bloco 4 entrega vai para o **candidato**, não para a empresa, por
# isso a correção não é trocar o destinatário da frase: é dizer só o que o produto garante.

Funcionalidade: Feedback de candidatura enviada
  Como candidato
  Quero que a confirmação diga o que realmente acontece depois de me candidatar
  Para não esperar um contato que ninguém prometeu de facto

  Cenário: o feedback confirma o registro e diz onde acompanhar
    Quando eu leio o feedback de candidatura enviada
    Então o título do feedback deve ser "Candidatura enviada"
    E o feedback deve indicar onde acompanhar a candidatura

  # A regressão que se quer impedir é a volta de qualquer versão da promessa de um ato de
  # terceiros: "a empresa foi notificada", "avisamos o recrutador", "a empresa já recebeu".
  #
  # A lista para nos atos, e **não** banemos a palavra "e-mail": hoje a redação não menciona canal
  # porque N1 ainda não existe, mas depois do Bloco 4 "enviamos um e-mail de confirmação" passa a
  # ser verdade — e um teste que a reprovasse estaria a travar a redação, não a proteger o
  # utilizador. O que nunca volta a ser verdade é o produto afirmar um ato de outra pessoa.
  Esquema do Cenário: o feedback não promete ação de terceiros
    Quando eu leio o feedback de candidatura enviada
    Então o feedback não deve prometer "<termo>"

    Exemplos:
      | termo      |
      | notificad  |
      | avisad     |
      | comunicad  |
      | empresa    |
      | recrutador |

  # Enquanto N1 não existir, prometer o e-mail repetiria o defeito que a feature corrige. Este
  # cenário fixa a decisão da redação atual; quando o Bloco 4 entrar, é ele que se remove — e a
  # remoção é deliberada, não um teste a falhar por surpresa.
  Cenário: enquanto a notificação não existir, a redação não promete canal nenhum
    Quando eu leio o feedback de candidatura enviada
    Então o feedback não deve prometer "e-mail"
    E o feedback não deve prometer "email"
