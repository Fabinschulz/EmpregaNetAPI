import { createPaginatedResponseSchema } from '@/shared/schema';
import { z } from 'zod';

const addressResponseSchema = z.object({
  street: z.string().nullable().optional(),
  zipCode: z.string().nullable().optional(),
  city: z.string().nullable().optional(),
  state: z.string().nullable().optional(),
  neighborhood: z.string().nullable().optional(),
  number: z.string().nullable().optional(),
  complement: z.string().nullable().optional()
});

export const companyResponseSchema = z
  .object({
    id: z.number().int(),
    companyName: z.string().nullable().optional(),
    registrationNumber: z.string().nullable().optional(),
    email: z.string().nullable().optional(),
    phone: z.string().nullable().optional(),
    typeOfActivity: z.string().nullable().optional(),
    address: addressResponseSchema.nullable().optional(),
    createdAt: z.string().nullable().optional(),
    updatedAt: z.string().nullable().optional(),
    deletedAt: z.string().nullable().optional(),
    isDeleted: z.boolean().optional()
  })
  .transform((company) => ({
    id: company.id,
    name: company.companyName ?? '',
    documentNo: company.registrationNumber ?? null,
    email: company.email ?? null,
    phone: company.phone ?? null,
    typeOfActivity: company.typeOfActivity ?? null,
    address: company.address ?? null,
    createdAt: company.createdAt ?? null,
    updatedAt: company.updatedAt ?? null,
    deletedAt: company.deletedAt ?? null,
    isDeleted: company.isDeleted ?? false
  }));

export type CompanyResponse = z.infer<typeof companyResponseSchema>;
export const companiesListResponseSchema = createPaginatedResponseSchema(companyResponseSchema);
export type CompaniesListResponse = z.infer<typeof companiesListResponseSchema>;
