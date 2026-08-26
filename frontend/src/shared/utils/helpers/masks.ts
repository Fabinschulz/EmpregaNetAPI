/** Mantém apenas os dígitos de um texto. */
export function onlyDigits(value: string | null | undefined): string {
  return (value ?? '').replace(/\D/g, '');
}

/**
 * Aplica a máscara de telefone brasileiro conforme o usuário digita:
 * fixo `(00) 0000-0000` (10 dígitos) ou celular `(00) 00000-0000` (11 dígitos).
 */
export function maskBrazilPhone(value: string | null | undefined): string {
  const d = onlyDigits(value).slice(0, 11);
  if (d.length === 0) return '';
  if (d.length <= 2) return `(${d}`;
  if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`;
  if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`;
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
}

/** Aplica a máscara de CEP conforme o usuário digita: `00000-000`. */
export function maskZipCode(value: string | null | undefined): string {
  const d = onlyDigits(value).slice(0, 8);
  return d.length <= 5 ? d : `${d.slice(0, 5)}-${d.slice(5)}`;
}

export function isCompleteZipCode(value: string | null | undefined): boolean {
  return onlyDigits(value).length === 8;
}

/**
 * Valida um telefone brasileiro (fixo com 10 dígitos ou celular com 11 dígitos).
 * Rejeita DDD iniciado em 0, dígitos todos iguais e celular sem o 9 na terceira posição.
 */
export function isValidBrazilPhone(value: string | null | undefined): boolean {
  const d = onlyDigits(value);
  if (d.length !== 10 && d.length !== 11) return false;
  if (/^(\d)\1+$/.test(d)) return false;
  if (d[0] === '0') return false;
  if (d.length === 11 && d[2] !== '9') return false;
  return true;
}

/**
 * Valida um celular brasileiro (DDD + 9 dígitos), espelhando a regra
 * `IsBrazilianCellPhone` do backend para contas de usuário.
 */
export function isValidBrazilCellPhone(value: string | null | undefined): boolean {
  const d = onlyDigits(value);
  return d.length === 11 && isValidBrazilPhone(d);
}

/** Quantidade de dígitos de um CPF. */
const CPF_LENGTH = 11;

/** Aplica a máscara de CPF conforme o usuário digita: `000.000.000-00`. */
export function maskCpf(value: string | null | undefined): string {
  const d = onlyDigits(value).slice(0, CPF_LENGTH);
  if (d.length <= 3) return d;
  if (d.length <= 6) return `${d.slice(0, 3)}.${d.slice(3)}`;
  if (d.length <= 9) return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6)}`;
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`;
}

/**
 * Valida um CPF pelos dígitos verificadores, espelhando `BrazilianDocument.IsValidCpf` do backend.
 *
 * Conferir o comprimento não bastaria: `111.111.111-11` satisfaz a aritmética do módulo 11, por isso
 * a sequência de dígitos repetidos é recusada explicitamente — igual à guarda do backend.
 */
export function isValidCpf(value: string | null | undefined): boolean {
  const d = onlyDigits(value);
  if (d.length !== CPF_LENGTH) return false;
  if (/^(\d)\1+$/.test(d)) return false;

  const checkDigit = (length: number): number => {
    let sum = 0;
    for (let i = 0; i < length; i += 1) {
      sum += Number(d[i]) * (length + 1 - i);
    }
    const remainder = (sum * 10) % 11;
    return remainder === 10 ? 0 : remainder;
  };

  return Number(d[9]) === checkDigit(9) && Number(d[10]) === checkDigit(10);
}

/** Forma de e-mail: um único `@`, com conteúdo dos dois lados e sem espaços. */
export function isEmailShaped(value: string | null | undefined): boolean {
  const text = (value ?? '').trim();
  const at = text.indexOf('@');
  return at > 0 && at === text.lastIndexOf('@') && at < text.length - 1 && !/\s/.test(text);
}

/**
 * Identificador aceito no login: CPF **ou** e-mail, nunca nome de usuário.
 *
 * O critério é o mesmo do `LoginUserCommandValidator` do backend — a presença de `@` decide qual das
 * duas regras se aplica — para que a mensagem de erro apareça no formulário e não só na resposta HTTP.
 */
export function isValidLoginIdentifier(value: string | null | undefined): boolean {
  const text = (value ?? '').trim();
  if (text.length === 0) return false;
  return text.includes('@') ? isEmailShaped(text) : isValidCpf(text);
}
