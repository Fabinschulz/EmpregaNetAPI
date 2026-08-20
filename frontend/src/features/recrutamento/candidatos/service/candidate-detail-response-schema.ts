import { displayNameOrId } from '@/shared/utils';
import { z } from 'zod';

/**
 * Contagem de candidaturas por status. `status` vem como **nome do enum** do backend — o mesmo
 * contrato de `JobApplicationViewModel`, traduzido em `features/candidaturas/domain`.
 */
const candidateApplicationsByStatusSchema = z.object({
  status: z.string(),
  count: z.number().int().nonnegative()
});

const candidateApplicationsSummarySchema = z.object({
  total: z.number().int().nonnegative(),
  byStatus: z.array(candidateApplicationsByStatusSchema)
});

export const candidateDetailResponseSchema = z
  .object({
    id: z.number().int(),
    username: z.string(),
    email: z.string(),
    phoneNumber: z.string().nullable().optional(),
    userType: z.string(),
    roles: z.array(z.string()),
    profilePicture: z.string().nullable().optional(),
    city: z.string().nullable().optional(),
    state: z.string().nullable().optional(),
    age: z.number().int().nullable().optional(),
    createdAt: z.string(),
    updatedAt: z.string().nullable().optional(),
    isDeleted: z.boolean(),
    applications: candidateApplicationsSummarySchema
  })
  .transform((candidate) => ({
    id: candidate.id,
    username: candidate.username.trim(),
    email: candidate.email.trim(),
    phoneNumber: candidate.phoneNumber?.trim() || null,
    userType: candidate.userType.trim(),
    roles: candidate.roles,
    profilePicture: candidate.profilePicture?.trim() || null,
    city: candidate.city?.trim() || null,
    state: candidate.state?.trim() || null,
    age: candidate.age ?? null,
    createdAt: candidate.createdAt,
    updatedAt: candidate.updatedAt ?? null,
    isDeleted: candidate.isDeleted,
    applications: candidate.applications
  }));

export type CandidateDetailResponse = z.infer<typeof candidateDetailResponseSchema>;

export function candidateDisplayName(candidate: Pick<CandidateDetailResponse, 'id' | 'username'>): string {
  return displayNameOrId(candidate.username, candidate.id);
}
