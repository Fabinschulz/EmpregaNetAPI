'use client';

import { Alert, Button, Card, CardContent, CardFooter, CardHeader, CardTitle, StatusBadge } from '@/components';
import { useAuth } from '@/context';
import { useApplyToJobMutation } from '@/features/candidaturas/service';
import type { JobDto } from '@/features/recrutamento/vagas/service';
import { useRelativeTime } from '@/hooks';
import {
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  normalizeUf,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { formatSalaryRange } from '@/utils';
import type { LucideIcon } from 'lucide-react';
import {
  Accessibility,
  Banknote,
  Briefcase,
  Building2,
  CalendarDays,
  Clock,
  GraduationCap,
  LayoutGrid,
  MapPin
} from 'lucide-react';
import styles from './job-detail.module.scss';

type JobDetailPageProps = {
  /** Vaga já resolvida no servidor (Server Component). Este componente não busca dados. */
  job: JobDto;
};

type MetaItem = {
  key: string;
  icon: LucideIcon;
  label: string;
};

export function JobDetailPage({ job }: JobDetailPageProps) {
  const { isAuthenticated } = useAuth();
  const { apiError, mutateAsync, isPending: isApplying } = useApplyToJobMutation(job.id);
  const publishedLabel = useRelativeTime(job.publishedAt ?? job.createdAt);

  function onApply() {
    if (!isAuthenticated) return;
    void mutateAsync();
  }

  const applyLabel = !isAuthenticated ? 'Faça login para se candidatar' : isApplying ? 'Enviando...' : 'Candidatar-me';

  const meta: MetaItem[] = [];

  const city = job.city?.trim();
  const state = normalizeUf(job.state);
  if (city || state) {
    meta.push({ key: 'location', icon: MapPin, label: [city, state].filter(Boolean).join(', ') });
  }

  const workShift = workShiftVocabulary.normalize(job.workShift);
  if (workShift) {
    meta.push({ key: 'workShift', icon: Clock, label: workShiftVocabulary.label(workShift) });
  }

  const experienceLevel = experienceLevelVocabulary.normalize(job.experienceLevel);
  if (experienceLevel) {
    meta.push({
      key: 'experienceLevel',
      icon: GraduationCap,
      label: experienceLevelVocabulary.label(experienceLevel)
    });
  }

  const jobType = jobTypeVocabulary.normalize(job.jobType);
  if (jobType) {
    meta.push({ key: 'jobType', icon: Briefcase, label: jobTypeVocabulary.label(jobType) });
  }

  const workModel = workModelVocabulary.normalize(job.workModel);
  if (workModel && workModel !== 'OnSite') {
    meta.push({ key: 'workModel', icon: Building2, label: workModelVocabulary.label(workModel) });
  }

  if (job.isPcdFriendly) {
    meta.push({ key: 'pcd', icon: Accessibility, label: 'Vaga afirmativa para PcD' });
  }

  const area = jobAreaVocabulary.normalize(job.area);
  if (area) {
    meta.push({ key: 'area', icon: LayoutGrid, label: jobAreaVocabulary.label(area) });
  }

  meta.push({
    key: 'salary',
    icon: Banknote,
    label: formatSalaryRange(job.salaryMin, job.salaryMax, job.salaryDisclosed ?? true)
  });

  if (job.publishedAt ?? job.createdAt) {
    meta.push({ key: 'published', icon: CalendarDays, label: `Publicada ${publishedLabel}` });
  }

  const requirements = job.requirements ?? [];
  const benefits = job.benefits ?? [];

  return (
    <section>
      <Card className={styles.card}>
        <CardHeader>
          <div className={styles.headerRow}>
            <CardTitle>{job.title}</CardTitle>
            <StatusBadge label={job.isActive ? 'Ativa' : 'Encerrada'} tone={job.isActive ? 'positive' : 'negative'} />
          </div>

          {job.summary?.trim() ? <p className={styles.summary}>{job.summary}</p> : null}

          <ul className={styles.meta}>
            {meta.map(({ key, icon: Icon, label }) => (
              <li key={key} className={styles.metaItem}>
                <Icon aria-hidden />
                <span>{label}</span>
              </li>
            ))}
          </ul>
        </CardHeader>

        <CardContent>
          <h2 className={styles.sectionLabel}>Descrição</h2>
          <p className={styles.description}>{job.description?.trim() ? job.description : 'Sem descrição.'}</p>

          {requirements.length > 0 ? (
            <>
              <h2 className={styles.sectionLabel}>Requisitos</h2>
              <ul className={styles.tags}>
                {requirements.map((requirement) => (
                  <li key={requirement} className={styles.tag}>
                    {requirement}
                  </li>
                ))}
              </ul>
            </>
          ) : null}

          {benefits.length > 0 ? (
            <>
              <h2 className={styles.sectionLabel}>Benefícios</h2>
              <ul className={styles.tags}>
                {benefits.map((benefit) => (
                  <li key={benefit} className={styles.tag}>
                    {benefit}
                  </li>
                ))}
              </ul>
            </>
          ) : null}
        </CardContent>

        <CardFooter className={styles.footer}>
          <Button variant="primary" onClick={onApply} disabled={!isAuthenticated || isApplying}>
            {applyLabel}
          </Button>
          {!isAuthenticated ? (
            <p className={styles.footerHint}>É necessário entrar na conta para enviar sua candidatura.</p>
          ) : null}
          {apiError ? (
            <Alert variant="destructive" title="Erro">
              {apiError}
            </Alert>
          ) : null}
        </CardFooter>
      </Card>
    </section>
  );
}
