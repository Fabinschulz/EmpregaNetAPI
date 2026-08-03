import {
  LIST_ORDER_BY_VALUES,
  UF_VALUE_SET,
  createPaginatedResponseSchema,
  normalizeUf,
  type CompaniesListQueryParams
} from '@/shared/schema';
import { maskBrazilPhone, onlyDigits } from '@/utils';
import { z } from 'zod';
export { UF_OPTIONS, normalizeUf } from '@/shared/schema';

export const TYPE_OF_ACTIVITY_OPTIONS = [
  { value: 'Industry', label: 'Indústria', description: 'Indústria' },
  { value: 'services', label: 'Serviços', description: 'Serviços' },
  { value: 'business', label: 'Comércio', description: 'Comércio' }
] as const;

export type TypeOfActivityValue = (typeof TYPE_OF_ACTIVITY_OPTIONS)[number]['value'];

const ACTIVITY_VALUE_SET = new Set<string>(TYPE_OF_ACTIVITY_OPTIONS.map((o) => o.value));

const ACTIVITY_LOOKUP = new Map<string, TypeOfActivityValue>(
  TYPE_OF_ACTIVITY_OPTIONS.flatMap((o) => [
    [o.value.toLowerCase(), o.value],
    [o.description.toLowerCase(), o.value]
  ])
);

export function normalizeTypeOfActivity(input: string | null | undefined): TypeOfActivityValue | '' {
  if (!input) return '';
  return ACTIVITY_LOOKUP.get(input.trim().toLowerCase()) ?? '';
}

const addressReadSchema = z.object({
  street: z.string().nullable().optional(),
  zipCode: z.string().nullable().optional(),
  city: z.string().nullable().optional(),
  state: z.union([z.string(), z.number()]).nullable().optional(),
  neighborhood: z.string().nullable().optional(),
  number: z.string().nullable().optional(),
  complement: z.string().nullable().optional()
});

export const companySchema = z
  .object({
    id: z.number().int(),
    companyName: z.string().nullable().optional(),
    registrationNumber: z.string().nullable().optional(),
    email: z.string().nullable().optional(),
    phone: z.string().nullable().optional(),
    typeOfActivity: z.string().nullable().optional(),
    address: addressReadSchema.nullable().optional(),
    createdAt: z.string().nullable().optional()
  })
  .transform((c) => ({
    id: c.id,
    name: c.companyName ?? '',
    documentNo: c.registrationNumber ?? null,
    email: c.email ?? null,
    phone: c.phone ?? null,
    typeOfActivity: c.typeOfActivity ?? null,
    address: c.address ?? null,
    createdAt: c.createdAt ?? null
  }));

export type CompanyDto = z.infer<typeof companySchema>;
export const companiesListResponseSchema = createPaginatedResponseSchema(companySchema);
export type CompaniesListResponseDto = z.infer<typeof companiesListResponseSchema>;

const addressFormSchema = z.object({
  street: z.string().trim().min(1, 'Informe o logradouro.').max(200, 'Máximo de 200 caracteres.'),
  number: z.string().trim().min(1, 'Informe o número.'),
  complement: z.string(),
  neighborhood: z.string().trim().min(1, 'Informe o bairro.'),
  city: z.string().trim().min(1, 'Informe a cidade.').max(100, 'Máximo de 100 caracteres.'),
  state: z.string().refine((s) => UF_VALUE_SET.has(s), { message: 'Selecione o estado.' }),
  zipCode: z
    .string()
    .trim()
    .refine((s) => /^\d{5}-?\d{3}$/.test(s), { message: 'CEP inválido (formato 00000-000).' })
});

export const companyFormSchema = z.object({
  companyName: z
    .string()
    .trim()
    .min(3, 'O nome deve ter no mínimo 3 caracteres.')
    .max(100, 'O nome deve ter no máximo 100 caracteres.'),
  cnpj: z
    .string()
    .trim()
    .refine((s) => onlyDigits(s).length === 14, { message: 'O CNPJ deve conter 14 dígitos.' }),
  email: z.string().trim().email('E-mail inválido.'),
  phone: z
    .string()
    .trim()
    .refine((s) => /^\d{10,11}$/.test(onlyDigits(s)), { message: 'Telefone inválido (10 ou 11 dígitos).' }),
  typeOfActivity: z.string().refine((s) => ACTIVITY_VALUE_SET.has(s), { message: 'Selecione o tipo de atividade.' }),
  address: addressFormSchema
});

export type CompanyFormValues = z.infer<typeof companyFormSchema>;

export const defaultFormCompany: CompanyFormValues = {
  companyName: '',
  cnpj: '',
  email: '',
  phone: '',
  typeOfActivity: '',
  address: {
    street: '',
    number: '',
    complement: '',
    neighborhood: '',
    city: '',
    state: '',
    zipCode: ''
  }
};

export function companyFormValuesFromDto(company: CompanyDto): CompanyFormValues {
  return {
    companyName: company.name,
    cnpj: onlyDigits(company.documentNo),
    email: company.email ?? '',
    phone: maskBrazilPhone(company.phone),
    typeOfActivity: normalizeTypeOfActivity(company.typeOfActivity),
    address: {
      street: company.address?.street ?? '',
      number: company.address?.number ?? '',
      complement: company.address?.complement ?? '',
      neighborhood: company.address?.neighborhood ?? '',
      city: company.address?.city ?? '',
      state: normalizeUf(company.address?.state),
      zipCode: company.address?.zipCode ?? ''
    }
  };
}

export function companyFormToApiPayload(values: CompanyFormValues) {
  return {
    companyName: values.companyName.trim(),
    cnpj: onlyDigits(values.cnpj),
    email: values.email.trim(),
    phone: onlyDigits(values.phone),
    typeOfActivity: values.typeOfActivity,
    address: {
      street: values.address.street.trim(),
      number: values.address.number.trim(),
      complement: values.address.complement.trim() || null,
      neighborhood: values.address.neighborhood.trim(),
      city: values.address.city.trim(),
      state: values.address.state,
      zipCode: values.address.zipCode.trim()
    }
  };
}

export const companiesFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  situation: z.enum(['all', 'active', 'deleted']),
  orderBy: z.enum(LIST_ORDER_BY_VALUES)
});

export type CompaniesFilterFormValues = z.infer<typeof companiesFilterFormSchema>;

export const defaultCompaniesFilter: CompaniesFilterFormValues = {
  search: '',
  situation: 'all',
  orderBy: 'createdAt_DESC'
};

export function companiesFilterToParams(
  values: CompaniesFilterFormValues
): Pick<CompaniesListQueryParams, 'search' | 'isDeleted' | 'orderBy'> {
  return {
    search: values.search.trim() || undefined,
    isDeleted: values.situation === 'all' ? undefined : values.situation === 'deleted',
    orderBy: values.orderBy
  };
}
