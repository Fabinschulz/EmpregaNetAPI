import {
    MAX_VOCABULARY_ITEMS_PER_JOB,
    UF_VALUE_SET,
    createPaginatedResponseSchema,
    experienceLevelVocabulary,
    jobAreaVocabulary,
    jobTypeVocabulary,
    normalizeUf,
    workModelVocabulary,
    workShiftVocabulary,
    type JobsListQueryParams
} from '@/shared/schema';
import { z } from 'zod';

export {
    experienceLevelVocabulary,
    jobAreaVocabulary,
    jobTypeVocabulary,
    workModelVocabulary,
    workShiftVocabulary
} from '@/shared/schema';

export const jobSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  summary: z.string().nullable().optional(),
  description: z.string().nullable().optional(),
  companyId: z.number().int().nullable().optional(),
  salaryMin: z.number().nullable().optional(),
  salaryMax: z.number().nullable().optional(),
  salaryDisclosed: z.boolean().nullable().optional(),
  jobType: z.union([z.string(), z.number()]).nullable().optional(),
  workModel: z.union([z.string(), z.number()]).nullable().optional(),
  workShift: z.union([z.string(), z.number()]).nullable().optional(),
  experienceLevel: z.union([z.string(), z.number()]).nullable().optional(),
  area: z.union([z.string(), z.number()]).nullable().optional(),
  isPcdFriendly: z.boolean().nullable().optional(),
  city: z.string().nullable().optional(),
  state: z.union([z.string(), z.number()]).nullable().optional(),
  country: z.string().nullable().optional(),
  requirements: z.array(z.string()).nullable().optional(),
  benefits: z.array(z.string()).nullable().optional(),
  isActive: z.boolean(),
  publishedAt: z.string().nullable().optional(),
  createdAt: z.string().nullable().optional()
});

export type JobDto = z.infer<typeof jobSchema>;
export const jobsListResponseSchema = createPaginatedResponseSchema(jobSchema);
export type JobsListResponseDto = z.infer<typeof jobsListResponseSchema>;

export const companyOptionSchema = z.object({
  id: z.number().int(),
  name: z.string()
});

export const companyOptionsResponseSchema = z.array(companyOptionSchema);
export type CompanyOption = z.infer<typeof companyOptionSchema>;

const optionalMoney = z
  .string()
  .trim()
  .refine((s) => s === '' || (Number.isFinite(Number(s)) && Number(s) >= 0), {
    message: 'Informe um valor válido (maior ou igual a zero).'
  });

const vocabularySelection = (label: string) =>
  z
    .array(z.string())
    .max(MAX_VOCABULARY_ITEMS_PER_JOB, { message: `Selecione no máximo ${MAX_VOCABULARY_ITEMS_PER_JOB} ${label}.` });

export const jobFormSchema = z
  .object({
    companyId: z.string().min(1, 'Selecione a empresa.'),
    title: z.string().trim().min(1, 'Informe o título.').max(100, 'O título deve ter no máximo 100 caracteres.'),
    summary: z.string().trim().max(280, 'O resumo deve ter no máximo 280 caracteres.'),
    description: z.string().trim().min(1, 'Informe a descrição.'),
    jobType: z.string().refine((value): boolean => jobTypeVocabulary.has(value), {
      message: 'Selecione o tipo de contratação.'
    }),
    workModel: z.string().refine((value): boolean => workModelVocabulary.has(value), {
      message: 'Selecione a modalidade.'
    }),
    workShift: z.string().refine((value): boolean => workShiftVocabulary.has(value), {
      message: 'Selecione o turno.'
    }),
    experienceLevel: z.string().refine((value): boolean => experienceLevelVocabulary.has(value), {
      message: 'Selecione a experiência exigida.'
    }),
    area: z.string().refine((value): boolean => jobAreaVocabulary.has(value), { message: 'Selecione a área.' }),
    city: z.string().trim().min(1, 'Informe a cidade.').max(100, 'A cidade deve ter no máximo 100 caracteres.'),
    state: z.string().refine((s): boolean => UF_VALUE_SET.has(s), { message: 'Selecione o estado.' }),
    pcd: z.enum(['no', 'yes']),
    salaryDisclosure: z.enum(['disclosed', 'undisclosed']),
    salaryMin: optionalMoney,
    salaryMax: optionalMoney,
    requirements: vocabularySelection('requisitos'),
    benefits: vocabularySelection('benefícios')
  })
  .refine((values) => values.salaryDisclosure === 'undisclosed' || values.salaryMin !== '' || values.salaryMax !== '', {
    message: 'Informe ao menos o piso ou o teto salarial, ou marque como "a combinar".',
    path: ['salaryMin']
  })
  .refine(
    (values) =>
      values.salaryMin === '' || values.salaryMax === '' || Number(values.salaryMax) >= Number(values.salaryMin),
    { message: 'O teto salarial não pode ser menor que o piso.', path: ['salaryMax'] }
  );

export type JobFormValues = z.infer<typeof jobFormSchema>;

export const defaultFormJob: JobFormValues = {
  companyId: '',
  title: '',
  summary: '',
  description: '',
  jobType: '',
  workModel: '',
  workShift: '',
  experienceLevel: '',
  area: '',
  city: '',
  state: '',
  pcd: 'no',
  salaryDisclosure: 'disclosed',
  salaryMin: '',
  salaryMax: '',
  requirements: [],
  benefits: []
};

const moneyToInput = (value: number | null | undefined): string => (value != null ? String(value) : '');

export function jobFormValuesFromDto(job: JobDto): JobFormValues {
  return {
    companyId: job.companyId != null ? String(job.companyId) : '',
    title: job.title,
    summary: job.summary ?? '',
    description: job.description ?? '',
    jobType: jobTypeVocabulary.normalize(job.jobType),
    workModel: workModelVocabulary.normalize(job.workModel),
    workShift: workShiftVocabulary.normalize(job.workShift),
    experienceLevel: experienceLevelVocabulary.normalize(job.experienceLevel),
    area: jobAreaVocabulary.normalize(job.area),
    city: job.city ?? '',
    state: normalizeUf(job.state),
    pcd: job.isPcdFriendly ? 'yes' : 'no',
    salaryDisclosure: (job.salaryDisclosed ?? true) ? 'disclosed' : 'undisclosed',
    salaryMin: moneyToInput(job.salaryMin),
    salaryMax: moneyToInput(job.salaryMax),
    requirements: job.requirements ?? [],
    benefits: job.benefits ?? []
  };
}

export const SALARY_DISCLOSURE_OPTIONS = [
  { value: 'disclosed', label: 'Divulgar faixa salarial' },
  { value: 'undisclosed', label: 'A combinar' }
] as const;

export const PCD_OPTIONS = [
  { value: 'no', label: 'Vaga regular' },
  { value: 'yes', label: 'Vaga afirmativa para PcD' }
] as const;

export function jobFormToApiPayload(values: JobFormValues) {
  const toNumber = (value: string) => (value === '' ? undefined : Number(value));
  const salaryDisclosed = values.salaryDisclosure === 'disclosed';

  return {
    companyId: Number(values.companyId),
    title: values.title.trim(),
    summary: values.summary.trim() || undefined,
    description: values.description.trim(),
    jobType: values.jobType,
    workModel: values.workModel,
    workShift: values.workShift,
    experienceLevel: values.experienceLevel,
    area: values.area,
    city: values.city.trim(),
    state: values.state,
    isPcdFriendly: values.pcd === 'yes',
    salaryDisclosed,
    salaryMin: salaryDisclosed ? toNumber(values.salaryMin) : undefined,
    salaryMax: salaryDisclosed ? toNumber(values.salaryMax) : undefined,
    requirements: values.requirements,
    benefits: values.benefits
  };
}

export const jobsFilterFormSchema = z.object({
  search: z.string().trim().max(120, { message: 'A busca não pode exceder 120 caracteres.' }),
  status: z.enum(['all', 'active', 'closed'])
});

export type JobsFilterFormValues = z.infer<typeof jobsFilterFormSchema>;
export const defaultJobsFilter: JobsFilterFormValues = {
  search: '',
  status: 'all'
};

export function jobsFilterToParams(values: JobsFilterFormValues): Pick<JobsListQueryParams, 'search' | 'isActive'> {
  return {
    search: values.search.trim() || undefined,
    isActive: values.status === 'all' ? undefined : values.status === 'active'
  };
}
