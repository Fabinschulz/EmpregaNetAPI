/**
 * Unidades federativas do Brasil - fonte única.
 *
 * Vivia em `features/admin/empresas/service`; passou para cá quando o terceiro consumidor
 * apareceu (formulário de empresa, formulário de vaga e filtros do feed). Duas listas de UF
 * divergindo é o tipo de defeito que só aparece quando um estado some de um filtro.
 */

/** Ordem do enum `UF` do backend (índice 0 = NaoSelecionado), para converter inteiro em nome. */
const UF_ORDER = [
  'NaoSelecionado',
  'AC',
  'AL',
  'AP',
  'AM',
  'BA',
  'CE',
  'DF',
  'ES',
  'GO',
  'MA',
  'MT',
  'MS',
  'MG',
  'PA',
  'PB',
  'PR',
  'PE',
  'PI',
  'RJ',
  'RN',
  'RS',
  'RO',
  'RR',
  'SC',
  'SP',
  'SE',
  'TO'
] as const;

export const UF_OPTIONS = [
  { value: 'AC', label: 'Acre' },
  { value: 'AL', label: 'Alagoas' },
  { value: 'AP', label: 'Amapá' },
  { value: 'AM', label: 'Amazonas' },
  { value: 'BA', label: 'Bahia' },
  { value: 'CE', label: 'Ceará' },
  { value: 'DF', label: 'Distrito Federal' },
  { value: 'ES', label: 'Espírito Santo' },
  { value: 'GO', label: 'Goiás' },
  { value: 'MA', label: 'Maranhão' },
  { value: 'MT', label: 'Mato Grosso' },
  { value: 'MS', label: 'Mato Grosso do Sul' },
  { value: 'MG', label: 'Minas Gerais' },
  { value: 'PA', label: 'Pará' },
  { value: 'PB', label: 'Paraíba' },
  { value: 'PR', label: 'Paraná' },
  { value: 'PE', label: 'Pernambuco' },
  { value: 'PI', label: 'Piauí' },
  { value: 'RJ', label: 'Rio de Janeiro' },
  { value: 'RN', label: 'Rio Grande do Norte' },
  { value: 'RS', label: 'Rio Grande do Sul' },
  { value: 'RO', label: 'Rondônia' },
  { value: 'RR', label: 'Roraima' },
  { value: 'SC', label: 'Santa Catarina' },
  { value: 'SP', label: 'São Paulo' },
  { value: 'SE', label: 'Sergipe' },
  { value: 'TO', label: 'Tocantins' }
] as const;

export type UfValue = (typeof UF_OPTIONS)[number]['value'];

/**
 * Opções para seletores: sigla + nome (`MG - Minas Gerais`).
 *
 * Fonte única do rótulo composto. O formulário de vaga e o painel de filtros do feed montavam
 * cada um a sua versão da mesma expressão, e duas grafias do mesmo estado num select e num
 * filtro é confusão gratuita para quem usa.
 */
export const UF_SELECT_OPTIONS = UF_OPTIONS.map((uf) => ({
  value: uf.value,
  label: `${uf.value} - ${uf.label}`
}));

export const UF_VALUE_SET = new Set<string>(UF_OPTIONS.map((o) => o.value));

export function isUfValue(value: string): value is UfValue {
  return UF_VALUE_SET.has(value);
}

/** Rótulo curto usado em cartões e chips: `MG` em vez de `Minas Gerais`. */
export function ufLabel(value: string): string {
  return isUfValue(value) ? value : '';
}

/** Nome por extenso, para seletores de formulário. */
export function ufFullLabel(value: string): string {
  return UF_OPTIONS.find((o) => o.value === value)?.label ?? '';
}

/**
 * Aceita o nome (`"MG"`) ou o índice do enum do backend e devolve sempre o nome.
 * O contrato de leitura ainda varia entre endpoints antigos (inteiro) e novos (nome).
 */
export function normalizeUf(input: string | number | null | undefined): string {
  if (input == null) return '';
  if (typeof input === 'number') return UF_ORDER[input] ?? '';

  const trimmed = input.trim().toUpperCase();
  if (UF_VALUE_SET.has(trimmed)) return trimmed;

  const asIndex = Number(trimmed);
  if (Number.isInteger(asIndex) && asIndex > 0) return UF_ORDER[asIndex] ?? '';
  return '';
}
