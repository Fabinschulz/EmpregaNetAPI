import { createPaginatedResponseSchema, type JobsListQueryParams } from '@/shared';
import { z } from 'zod';

export const JOB_TYPE_OPTIONS = [
  { value: 'FullTime', label: 'Tempo Integral' },
  { value: 'PartTime', label: 'Meio Período' },
  { value: 'Internship', label: 'Estágio' },
  { value: 'Freelancer', label: 'Freelancer' },
  { value: 'Temporary', label: 'Temporário' },
  { value: 'Trainee', label: 'Trainee' },
  { value: 'Volunteer', label: 'Voluntário' },
  { value: 'Remote', label: 'Remoto' }
] as const;

export type JobTypeValue = (typeof JOB_TYPE_OPTIONS)[number]['value'];

/** Ordem do enum (índice 0 = NaoSelecionado) para converter o inteiro da leitura em nome. */
const JOB_TYPE_ORDER = ['NaoSelecionado', ...JOB_TYPE_OPTIONS.map((o) => o.value)];
const JOB_TYPE_VALUE_SET = new Set<string>(JOB_TYPE_OPTIONS.map((o) => o.value));

export function normalizeJobType(input: string | number | null | undefined): JobTypeValue | '' {
  if (input == null) return '';
  if (typeof input === 'number') return (JOB_TYPE_ORDER[input] as JobTypeValue) ?? '';
  const trimmed = input.trim();
  if (JOB_TYPE_VALUE_SET.has(trimmed)) return trimmed as JobTypeValue;
  const asIndex = Number(trimmed);
  if (Number.isInteger(asIndex) && asIndex > 0) return (JOB_TYPE_ORDER[asIndex] as JobTypeValue) ?? '';
  return '';
}

export const jobSchema = z.object({
  id: z.number().int(),
  title: z.string().min(1),
  description: z.string().nullable().optional(),
  location: z.string().nullable().optional(),
  companyId: z.number().int().nullable().optional(),
  salary: z.number().nullable().optional(),
  jobType: z.union([z.string(), z.number()]).nullable().optional(),
  isActive: z.boolean().optional()
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


export const jobFormSchema = z.object({
  companyId: z.string().min(1, 'Selecione a empresa.'),
  title: z.string().trim().min(1, 'Informe o título.').max(100, 'O título deve ter no máximo 100 caracteres.'),
  description: z.string().trim().min(1, 'Informe a descrição.'),
  jobType: z.string().refine((s) => JOB_TYPE_VALUE_SET.has(s), { message: 'Selecione o tipo de vaga.' }),
  salary: z
    .string()
    .trim()
    .refine((s) => s !== '' && Number.isFinite(Number(s)) && Number(s) >= 0, {
      message: 'Informe um salário válido (maior ou igual a zero).'
    })
});

export type JobFormValues = z.infer<typeof jobFormSchema>;

export const defaultFormJob: JobFormValues = {
  companyId: '',
  title: '',
  description: '',
  jobType: '',
  salary: ''
};

export function jobFormValuesFromDto(job: JobDto): JobFormValues {
  return {
    companyId: job.companyId != null ? String(job.companyId) : '',
    title: job.title,
    description: job.description ?? '',
    jobType: normalizeJobType(job.jobType),
    salary: job.salary != null ? String(job.salary) : ''
  };
}

export function jobFormToApiPayload(values: JobFormValues) {
  return {
    companyId: Number(values.companyId),
    title: values.title.trim(),
    description: values.description.trim(),
    jobType: values.jobType,
    salary: Number(values.salary)
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

export const jobsSearchFormSchema = jobsFilterFormSchema.pick({ search: true });

export type JobsSearchFormValues = z.infer<typeof jobsSearchFormSchema>;

export const defaultJobsSearch: JobsSearchFormValues = { search: '' };
