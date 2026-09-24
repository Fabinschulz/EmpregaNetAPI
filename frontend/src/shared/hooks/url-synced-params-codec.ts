import type { ZodType } from 'zod';

/** Valor de um campo de filtro que pode viver na query string. */
export type UrlSyncedValue = string | boolean;

/** Valores de um formulário de filtro serializável na URL (um valor por chave). */
export type UrlSyncedValues = Record<string, UrlSyncedValue>;
export type SearchParamsReader = { get(name: string): string | null };

function readRawValue(raw: string, fallback: UrlSyncedValue): UrlSyncedValue | undefined {
  if (typeof fallback !== 'boolean') return raw;
  if (raw === 'true') return true;
  if (raw === 'false') return false;
  return undefined;
}

function toUrlValue(value: UrlSyncedValue): string {
  return typeof value === 'string' ? value.trim() : String(value);
}

/**
 * Lê da query string os valores de um formulário de filtro, campo a campo.
 *
 * Só as chaves presentes em `defaults` são lidas; as demais são ignoradas. Cada valor da URL é
 * validado pelo `schema` do próprio formulário, trocando só aquele campo nos defaults: valor fora
 * do enum, boolean ilegível ou texto que a regra do campo recusa cai no default, sem invalidar
 * os outros campos.
 *
 * @param searchParams Query string atual.
 * @param defaults Valores padrão do formulário, definem as chaves e o tipo de cada campo.
 * @param schema Schema Zod do formulário (o mesmo do `FormProvider`).
 * @returns Valores prontos para `defaultValues` do formulário.
 */
export function parseUrlSyncedParams<TValues extends UrlSyncedValues>(
  searchParams: SearchParamsReader,
  defaults: TValues,
  schema: ZodType<TValues>
): TValues {
  const values: TValues = { ...defaults };

  for (const key of Object.keys(defaults) as (keyof TValues & string)[]) {
    const raw = searchParams.get(key);
    if (raw === null) continue;

    const candidate = readRawValue(raw, defaults[key]);
    if (candidate === undefined) continue;

    const parsed = schema.safeParse({ ...defaults, [key]: candidate });
    if (parsed.success) values[key] = parsed.data[key];
  }

  return values;
}

/**
 * Serializa os valores de um formulário de filtro para a query string.
 */
export function serializeUrlSyncedParams<TValues extends UrlSyncedValues>(
  values: TValues,
  defaults: TValues
): URLSearchParams {
  const params = new URLSearchParams();

  for (const key of Object.keys(defaults)) {
    const value = toUrlValue(values[key] ?? defaults[key]);
    if (value === '' || value === toUrlValue(defaults[key])) continue;
    params.set(key, value);
  }

  return params;
}

export function hasActiveUrlSyncedParams<TValues extends UrlSyncedValues>(values: TValues, defaults: TValues): boolean {
  return serializeUrlSyncedParams(values, defaults).toString() !== '';
}
