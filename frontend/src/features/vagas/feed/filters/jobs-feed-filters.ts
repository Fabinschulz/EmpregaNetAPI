import {
  DEFAULT_JOB_SORT,
  experienceLevelVocabulary,
  findSalaryRange,
  isJobSortValue,
  isPublishedWithinValue,
  isUfValue,
  jobAreaVocabulary,
  jobTypeVocabulary,
  workModelVocabulary,
  workShiftVocabulary,
  type ExperienceLevelValue,
  type JobAreaValue,
  type JobSortValue,
  type JobTypeValue,
  type PublishedWithinValue,
  type WorkModelValue,
  type WorkShiftValue
} from '@/shared/schema';
import { JOBS_FEED_PAGE_SIZE, type JobsFeedQueryParams } from '../../service/jobs-feed-params';

export type JobsFeedFilters = {
  search: string;
  cities: string[];
  states: string[];
  workModels: WorkModelValue[];
  workShifts: WorkShiftValue[];
  jobTypes: JobTypeValue[];
  experienceLevels: ExperienceLevelValue[];
  areas: JobAreaValue[];
  requirements: string[];
  benefits: string[];
  companyIds: number[];
  salaryRange: string | null;
  publishedWithin: PublishedWithinValue | null;
  onlyPcd: boolean;
  sort: JobSortValue;
};

export const defaultJobsFeedFilters: JobsFeedFilters = {
  search: '',
  cities: [],
  states: [],
  workModels: [],
  workShifts: [],
  jobTypes: [],
  experienceLevels: [],
  areas: [],
  requirements: [],
  benefits: [],
  companyIds: [],
  salaryRange: null,
  publishedWithin: null,
  onlyPcd: false,
  sort: DEFAULT_JOB_SORT
};

export const JOBS_FEED_PARAM_KEYS = {
  search: 'q',
  city: 'city',
  state: 'uf',
  workModel: 'model',
  workShift: 'shift',
  jobType: 'type',
  experienceLevel: 'exp',
  area: 'area',
  requirement: 'req',
  benefit: 'benefit',
  company: 'company',
  salary: 'salary',
  publishedWithin: 'since',
  pcd: 'pcd',
  sort: 'sort'
} as const;

type ReadableParams = Pick<URLSearchParams, 'get' | 'getAll'>;

export function searchParamsFromRecord(input: Record<string, string | string[] | undefined>): URLSearchParams {
  const params = new URLSearchParams();

  Object.entries(input).forEach(([key, value]) => {
    if (value === undefined) return;

    if (Array.isArray(value)) {
      value.forEach((item) => params.append(key, item));
      return;
    }

    params.append(key, value);
  });

  return params;
}

function readAll(params: ReadableParams, key: string): string[] {
  return params
    .getAll(key)
    .flatMap((value) => value.split(','))
    .map((value) => value.trim())
    .filter(Boolean);
}

function unique<T>(values: T[]): T[] {
  return [...new Set(values)];
}

function readVocabulary<T extends string>(
  params: ReadableParams,
  key: string,
  normalize: (input: string) => T | ''
): T[] {
  return unique(
    readAll(params, key)
      .map(normalize)
      .filter((value): value is T => value !== '')
  );
}

export function parseJobsFeedFilters(params: ReadableParams): JobsFeedFilters {
  const sort = params.get(JOBS_FEED_PARAM_KEYS.sort)?.trim() ?? '';
  const publishedWithin = params.get(JOBS_FEED_PARAM_KEYS.publishedWithin)?.trim() ?? '';
  const salaryRange = params.get(JOBS_FEED_PARAM_KEYS.salary)?.trim() ?? '';

  return {
    search: params.get(JOBS_FEED_PARAM_KEYS.search)?.trim() ?? '',
    cities: unique(readAll(params, JOBS_FEED_PARAM_KEYS.city)),
    states: unique(
      readAll(params, JOBS_FEED_PARAM_KEYS.state)
        .map((v) => v.toUpperCase())
        .filter(isUfValue)
    ),
    workModels: readVocabulary(params, JOBS_FEED_PARAM_KEYS.workModel, workModelVocabulary.normalize),
    workShifts: readVocabulary(params, JOBS_FEED_PARAM_KEYS.workShift, workShiftVocabulary.normalize),
    jobTypes: readVocabulary(params, JOBS_FEED_PARAM_KEYS.jobType, jobTypeVocabulary.normalize),
    experienceLevels: readVocabulary(params, JOBS_FEED_PARAM_KEYS.experienceLevel, experienceLevelVocabulary.normalize),
    areas: readVocabulary(params, JOBS_FEED_PARAM_KEYS.area, jobAreaVocabulary.normalize),
    requirements: unique(readAll(params, JOBS_FEED_PARAM_KEYS.requirement)),
    benefits: unique(readAll(params, JOBS_FEED_PARAM_KEYS.benefit)),
    companyIds: unique(
      readAll(params, JOBS_FEED_PARAM_KEYS.company)
        .map(Number)
        .filter((id) => Number.isInteger(id) && id > 0)
    ),
    salaryRange: findSalaryRange(salaryRange)?.value ?? null,
    publishedWithin: isPublishedWithinValue(publishedWithin) ? publishedWithin : null,
    onlyPcd: params.get(JOBS_FEED_PARAM_KEYS.pcd) === '1',
    sort: isJobSortValue(sort) ? sort : DEFAULT_JOB_SORT
  };
}

/**
 * Serializa os filtros de volta para a query string.
 *
 * Valores no padrão são omitidos: uma URL sem filtro nenhum fica `/vagas` limpa, e não
 * `/vagas?q=&sort=Recent&...`.
 */
export function jobsFeedFiltersToSearchParams(filters: JobsFeedFilters): URLSearchParams {
  const params = new URLSearchParams();

  const appendAll = (key: string, values: readonly string[]) => {
    values.forEach((value) => params.append(key, value));
  };

  if (filters.search.trim()) params.set(JOBS_FEED_PARAM_KEYS.search, filters.search.trim());
  appendAll(JOBS_FEED_PARAM_KEYS.city, filters.cities);
  appendAll(JOBS_FEED_PARAM_KEYS.state, filters.states);
  appendAll(JOBS_FEED_PARAM_KEYS.workModel, filters.workModels);
  appendAll(JOBS_FEED_PARAM_KEYS.workShift, filters.workShifts);
  appendAll(JOBS_FEED_PARAM_KEYS.jobType, filters.jobTypes);
  appendAll(JOBS_FEED_PARAM_KEYS.experienceLevel, filters.experienceLevels);
  appendAll(JOBS_FEED_PARAM_KEYS.area, filters.areas);
  appendAll(JOBS_FEED_PARAM_KEYS.requirement, filters.requirements);
  appendAll(JOBS_FEED_PARAM_KEYS.benefit, filters.benefits);
  appendAll(
    JOBS_FEED_PARAM_KEYS.company,
    filters.companyIds.map((id) => String(id))
  );

  if (filters.salaryRange) params.set(JOBS_FEED_PARAM_KEYS.salary, filters.salaryRange);
  if (filters.publishedWithin) params.set(JOBS_FEED_PARAM_KEYS.publishedWithin, filters.publishedWithin);
  if (filters.onlyPcd) params.set(JOBS_FEED_PARAM_KEYS.pcd, '1');
  if (filters.sort !== DEFAULT_JOB_SORT) params.set(JOBS_FEED_PARAM_KEYS.sort, filters.sort);

  return params;
}

export function jobsFeedFiltersToQueryString(filters: JobsFeedFilters): string {
  const params = jobsFeedFiltersToSearchParams(filters);
  params.sort();
  return params.toString();
}

export function countActiveJobsFeedFilters(filters: JobsFeedFilters): number {
  return (
    (filters.search.trim() ? 1 : 0) +
    filters.cities.length +
    filters.states.length +
    filters.workModels.length +
    filters.workShifts.length +
    filters.jobTypes.length +
    filters.experienceLevels.length +
    filters.areas.length +
    filters.requirements.length +
    filters.benefits.length +
    filters.companyIds.length +
    (filters.salaryRange ? 1 : 0) +
    (filters.publishedWithin ? 1 : 0) +
    (filters.onlyPcd ? 1 : 0)
  );
}

const omitEmpty = <T>(values: T[]): T[] | undefined => (values.length > 0 ? values : undefined);

export function jobsFeedFiltersToApiParams(
  filters: JobsFeedFilters,
  page: number,
  size: number = JOBS_FEED_PAGE_SIZE
): JobsFeedQueryParams {
  const range = findSalaryRange(filters.salaryRange);

  return {
    page,
    size,
    search: filters.search.trim() || undefined,
    city: omitEmpty(filters.cities),
    state: omitEmpty(filters.states),
    workModel: omitEmpty(filters.workModels),
    shift: omitEmpty(filters.workShifts),
    jobType: omitEmpty(filters.jobTypes),
    experience: omitEmpty(filters.experienceLevels),
    area: omitEmpty(filters.areas),
    requirement: omitEmpty(filters.requirements),
    benefit: omitEmpty(filters.benefits),
    companyId: omitEmpty(filters.companyIds),
    salaryMin: range?.min,
    salaryMax: range?.max,
    pcd: filters.onlyPcd || undefined,
    publishedWithin: filters.publishedWithin ?? undefined,
    sort: filters.sort === 'Relevance' && !filters.search.trim() ? DEFAULT_JOB_SORT : filters.sort
  };
}
