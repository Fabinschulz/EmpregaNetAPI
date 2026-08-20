import type { JobFeedItemResponse } from '@/features/vagas/service';
import { jobTypeVocabulary, normalizeUf, workModelVocabulary } from '@/shared/schema';
import { formatSalaryRange } from '@/shared/utils';

export type JobFactIcon = 'location' | 'salary' | 'remote' | 'hybrid' | 'onSite' | 'contract';

export type JobFact = {
  key: string;
  icon: JobFactIcon;
  label: string;
  srLabel: string;
  strong?: boolean;
};

const WORK_MODEL_ICONS: Record<string, JobFactIcon> = {
  Remote: 'remote',
  Hybrid: 'hybrid',
  OnSite: 'onSite'
};

export function toJobFacts(job: JobFeedItemResponse): JobFact[] {
  const facts: JobFact[] = [];

  const location = [job.location.city?.trim(), normalizeUf(job.location.state)].filter(Boolean).join(', ');
  if (location) {
    facts.push({ key: 'location', icon: 'location', label: location, srLabel: 'Localização' });
  }

  facts.push({
    key: 'salary',
    icon: 'salary',
    label: formatSalaryRange(job.salary.min, job.salary.max, job.salary.disclosed),
    srLabel: 'Faixa salarial',
    strong: job.salary.disclosed
  });

  const workModel = workModelVocabulary.normalize(job.workModel);
  if (workModel) {
    facts.push({
      key: 'workModel',
      icon: WORK_MODEL_ICONS[workModel] ?? 'location',
      label: workModelVocabulary.label(workModel),
      srLabel: 'Modalidade'
    });
  }

  const jobType = jobTypeVocabulary.normalize(job.jobType);
  if (jobType) {
    facts.push({
      key: 'jobType',
      icon: 'contract',
      label: jobTypeVocabulary.label(jobType),
      srLabel: 'Contratação'
    });
  }

  return facts;
}
