import { UF_VALUE_SET } from '@/shared/schema';
import { z } from 'zod';

export const TYPE_OF_ACTIVITY_VALUES = ['Industry', 'services', 'business'] as const;
export type TypeOfActivityValue = (typeof TYPE_OF_ACTIVITY_VALUES)[number];
export const TYPE_OF_ACTIVITY_VALUE_SET: ReadonlySet<string> = new Set(TYPE_OF_ACTIVITY_VALUES);

const digitsOnly = (pattern: RegExp, field: string, expected: string) =>
  z.string().refine((value) => pattern.test(value), { message: `"${field}" deve conter ${expected}, sem máscara.` });

const addressRequestSchema = z.object({
  street: z.string().trim().min(1).max(200),
  number: z.string().trim().min(1).max(20),
  complement: z.string().trim().min(1).nullable(),
  neighborhood: z.string().trim().min(1).max(100),
  city: z.string().trim().min(1).max(100),
  state: z.string().refine((value) => UF_VALUE_SET.has(value), {
    message: '"address.state" fora da lista de UFs aceita pela API.'
  }),
  zipCode: z.string().refine((value) => /^\d{5}-?\d{3}$/.test(value), {
    message: '"address.zipCode" deve estar no formato 00000-000.'
  })
});

export const companyRequestSchema = z.object({
  companyName: z.string().trim().min(3).max(100),
  cnpj: digitsOnly(/^\d{14}$/, 'cnpj', 'exatamente 14 dígitos'),
  email: z.email(),
  phone: digitsOnly(/^\d{10,11}$/, 'phone', '10 ou 11 dígitos'),
  typeOfActivity: z.string().refine((value): boolean => TYPE_OF_ACTIVITY_VALUE_SET.has(value), {
    message: '"typeOfActivity" fora do vocabulário aceito pela API.'
  }),
  address: addressRequestSchema
});

export type CompanyRequest = z.infer<typeof companyRequestSchema>;
