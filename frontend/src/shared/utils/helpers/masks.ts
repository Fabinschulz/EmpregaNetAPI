export const phoneMask = ['(', /[1-9]/, /\d/, ')', ' ', /\d/, /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];

export const cnpjMask = [
  /\d/,
  /\d/,
  '.',
  /\d/,
  /\d/,
  /\d/,
  '.',
  /\d/,
  /\d/,
  /\d/,
  '/',
  /\d/,
  /\d/,
  /\d/,
  /\d/,
  '-',
  /\d/,
  /\d/
];

export const cpfMask = [/\d/, /\d/, /\d/, '.', /\d/, /\d/, /\d/, '.', /\d/, /\d/, /\d/, '-', /\d/, /\d/];

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
