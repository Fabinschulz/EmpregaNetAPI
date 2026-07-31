import { onlyDigits } from '@/utils';
import { z } from 'zod';

const VIA_CEP_BASE_URL = 'https://viacep.com.br/ws';

/**
 * Resposta do ViaCEP. Todos os campos são opcionais de propósito: o serviço é externo e
 * fora do nosso controle, então tratamos qualquer campo ausente como vazio em vez de
 * derrubar o formulário.
 */
const viaCepResponseSchema = z.object({
  cep: z.string().optional(),
  logradouro: z.string().optional(),
  complemento: z.string().optional(),
  bairro: z.string().optional(),
  localidade: z.string().optional(),
  uf: z.string().optional(),
  erro: z.union([z.boolean(), z.string()]).optional()
});

export type ZipCodeAddress = {
  street: string;
  neighborhood: string;
  city: string;
  state: string;
};

export class ZipCodeLookupError extends Error {
  constructor(message = 'Não foi possível consultar o CEP.') {
    super(message);
    this.name = 'ZipCodeLookupError';
  }
}

function isNotFound(erro: boolean | string | undefined): boolean {
  return erro === true || erro === 'true';
}

/**
 * Consulta um CEP no ViaCEP e devolve o endereço, ou `null` quando o CEP não existe.
 *
 * @param zipCode CEP com ou sem máscara (só os dígitos são usados).
 * @param signal Permite cancelar a consulta anterior quando o usuário continua digitando.
 * @throws {ZipCodeLookupError} Falha de rede ou resposta inesperada do serviço.
 */
export async function lookupAddressByZipCode(zipCode: string, signal?: AbortSignal): Promise<ZipCodeAddress | null> {
  const digits = onlyDigits(zipCode);

  if (digits.length !== 8) {
    return null;
  }

  let response: Response;
  try {
    response = await fetch(`${VIA_CEP_BASE_URL}/${digits}/json/`, {
      signal,
      headers: { Accept: 'application/json' }
    });
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError') throw err;
    throw new ZipCodeLookupError();
  }

  if (response.status === 400) return null;
  if (!response.ok) throw new ZipCodeLookupError();

  const parsed = viaCepResponseSchema.safeParse(await response.json());
  if (!parsed.success) throw new ZipCodeLookupError();
  if (isNotFound(parsed.data.erro)) return null;

  return {
    street: parsed.data.logradouro?.trim() ?? '',
    neighborhood: parsed.data.bairro?.trim() ?? '',
    city: parsed.data.localidade?.trim() ?? '',
    state: parsed.data.uf?.trim().toUpperCase() ?? ''
  };
}
