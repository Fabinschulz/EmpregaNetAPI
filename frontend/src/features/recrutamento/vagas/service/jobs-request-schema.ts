import {
  MAX_VOCABULARY_ITEMS_PER_JOB,
  UF_VALUE_SET,
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { z } from 'zod';

const vocabularyName = (vocabulary: { has: (value: string) => boolean }, field: string) =>
  z.string().refine((value) => vocabulary.has(value), {
    message: `"${field}" fora do vocabulário aceito pela API (o nome do enum, nunca o índice).`
  });

const vocabularyItems = (field: string) =>
  z.array(z.string()).max(MAX_VOCABULARY_ITEMS_PER_JOB, {
    message: `"${field}" excede o máximo de ${MAX_VOCABULARY_ITEMS_PER_JOB} itens aceito pela API.`
  });

export const jobRequestSchema = z
  .object({
    companyId: z.number().int().positive({ message: 'Id da empresa inválido.' }),
    title: z.string().trim().min(1).max(100),
    summary: z.string().trim().max(280).optional(),
    description: z.string().trim().min(1),
    jobType: vocabularyName(jobTypeVocabulary, 'jobType'),
    workModel: vocabularyName(workModelVocabulary, 'workModel'),
    workShift: vocabularyName(workShiftVocabulary, 'workShift'),
    experienceLevel: vocabularyName(experienceLevelVocabulary, 'experienceLevel'),
    area: vocabularyName(jobAreaVocabulary, 'area'),
    city: z.string().trim().min(1).max(100),
    state: z.string().refine((value) => UF_VALUE_SET.has(value), {
      message: '"state" fora da lista de UFs aceita pela API.'
    }),
    salaryMin: z.number().nonnegative().optional(),
    salaryMax: z.number().nonnegative().optional(),
    salaryDisclosed: z.boolean(),
    isPcdFriendly: z.boolean(),
    requirements: vocabularyItems('requirements').optional(),
    benefits: vocabularyItems('benefits').optional()
  })
  .refine((job) => !job.salaryDisclosed || job.salaryMin !== undefined || job.salaryMax !== undefined, {
    message: 'Salário divulgado exige ao menos o piso ou o teto.',
    path: ['salaryMin']
  })
  .refine((job) => job.salaryMin === undefined || job.salaryMax === undefined || job.salaryMax >= job.salaryMin, {
    message: 'O teto salarial não pode ser menor que o piso.',
    path: ['salaryMax']
  });

export type JobRequest = z.infer<typeof jobRequestSchema>;
