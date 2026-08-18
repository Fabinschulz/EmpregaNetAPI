import { UF_VALUE_SET, normalizeUf } from '@/shared/schema';
import { maskBrazilPhone, onlyDigits } from '@/utils';
import { z } from 'zod';
import { TYPE_OF_ACTIVITY_VALUE_SET, normalizeTypeOfActivity } from '../domain/type-of-activity';
import type { CompanyRequest } from '../service/companies-request-schema';
import type { CompanyResponse } from '../service/companies-response-schema';

const addressFormSchema = z.object({
  street: z.string().trim().min(1, 'Informe o logradouro.').max(200, 'Máximo de 200 caracteres.'),
  number: z.string().trim().min(1, 'Informe o número.').max(20, 'Máximo de 20 caracteres.'),
  complement: z.string(),
  neighborhood: z.string().trim().min(1, 'Informe o bairro.').max(100, 'Máximo de 100 caracteres.'),
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
  typeOfActivity: z
    .string()
    .refine((s) => TYPE_OF_ACTIVITY_VALUE_SET.has(s), { message: 'Selecione o tipo de atividade.' }),
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

export function companyFormValuesFromResponse(company: CompanyResponse): CompanyFormValues {
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

export function companyFormToRequest(values: CompanyFormValues): CompanyRequest {
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
