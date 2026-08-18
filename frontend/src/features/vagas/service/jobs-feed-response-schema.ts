import { createPaginatedResponseSchema } from '@/shared/schema';
import { z } from 'zod';

const jobFeedCompanyResponseSchema = z.object({
  id: z.number().int(),
  name: z.string(),
  logoUrl: z.string().nullable().optional()
});

const jobFeedLocationResponseSchema = z.object({
  city: z.string(),
  state: z.string(),
  country: z.string()
});

const jobFeedSalaryResponseSchema = z.object({
  min: z.number().nullable().optional(),
  max: z.number().nullable().optional(),
  /** `false` significa "a combinar". */
  disclosed: z.boolean()
});

export const jobFeedItemResponseSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  summary: z.string().nullable().optional(),
  company: jobFeedCompanyResponseSchema,
  location: jobFeedLocationResponseSchema,
  salary: jobFeedSalaryResponseSchema,
  jobType: z.string(),
  workModel: z.string(),
  workShift: z.string(),
  experienceLevel: z.string(),
  area: z.string(),
  isPcdFriendly: z.boolean(),
  requirements: z.array(z.string()),
  benefits: z.array(z.string()),
  publishedAt: z.string(),
  applicationsCount: z.number().int().nonnegative(),
  isActive: z.boolean()
});

export type JobFeedItemResponse = z.infer<typeof jobFeedItemResponseSchema>;
export const jobsFeedResponseSchema = createPaginatedResponseSchema(jobFeedItemResponseSchema);
export type JobsFeedResponse = z.infer<typeof jobsFeedResponseSchema>;

export const jobFeedInteractionsResponseSchema = z.object({
  appliedJobIds: z.array(z.number().int())
});

export type JobFeedInteractionsResponse = z.infer<typeof jobFeedInteractionsResponseSchema>;

const vocabularyGroupResponseSchema = z.object({
  label: z.string(),
  items: z.array(z.string())
});

export const jobVocabularyResponseSchema = z.object({
  requirements: z.array(vocabularyGroupResponseSchema),
  benefits: z.array(vocabularyGroupResponseSchema),
  maxItemsPerJob: z.number().int().positive()
});

export type JobVocabularyResponse = z.infer<typeof jobVocabularyResponseSchema>;

export const emptyJobVocabulary: JobVocabularyResponse = {
  requirements: [],
  benefits: [],
  maxItemsPerJob: 20
};
