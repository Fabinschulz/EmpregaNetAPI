'use client';

import { useApplyToJobMutation } from '@/features/candidaturas/service';
import { describeAvailablePositions, jobStatusLabel, jobStatusTone } from '@/features/recrutamento/vagas/domain';
import type { JobResponse } from '@/features/recrutamento/vagas/service';
import { AppliedBadge } from '@/features/vagas/applied-badge';
import { useJobFeedInteractionsQuery } from '@/features/vagas/service';
import {
  actionIcons,
  Alert,
  Button,
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardSectionLabel,
  CardTitle,
  InfoItem,
  InfoList,
  Spinner,
  StatusBadge,
  TagList,
  toCardTags
} from '@/shared/components';
import { useAuth } from '@/shared/context';
import { useRelativeTime } from '@/shared/hooks';
import {
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  normalizeUf,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { formatSalaryRange } from '@/shared/utils';
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
  MapPin,
  Users
} from 'lucide-react';
import styles from './job-detail.module.scss';

type JobDetailPageProps = {
  /** Vaga já resolvida no servidor (Server Component). Este componente não busca dados. */
  job: JobResponse;
};

type MetaItem = {
  key: string;
  icon: LucideIcon;
  label: string;
  /** Dado que decide a leitura: sobe de peso na banda. */
  strong?: boolean;
  srLabel?: string;
};

export function JobDetailPage({ job }: JobDetailPageProps) {
  const { isAuthenticated } = useAuth();
  const { apiError, mutateAsync, isPending: isApplying } = useApplyToJobMutation(job.id);
  const { data: interactions } = useJobFeedInteractionsQuery([job.id]);
  const hasApplied = interactions?.appliedJobIds.includes(job.id) ?? false;
  const publishedLabel = useRelativeTime(job.publishedAt ?? job.createdAt);

  function onApply() {
    if (!isAuthenticated || hasApplied) return;
    mutateAsync().catch(() => {});
  }

  const applyLabel = isAuthenticated ? 'Candidatar-me' : 'Faça login para se candidatar';
  const meta: MetaItem[] = [];

  const city = job.city?.trim();
  const state = normalizeUf(job.state);
  if (city || state) {
    meta.push({
      key: 'location',
      icon: MapPin,
      label: [city, state].filter(Boolean).join(', '),
      srLabel: 'Localização'
    });
  }

  meta.push({
    key: 'salary',
    icon: Banknote,
    label: formatSalaryRange(job.salaryMin, job.salaryMax, job.salaryDisclosed ?? true),
    strong: job.salaryDisclosed ?? true,
    srLabel: 'Faixa salarial'
  });

  const workModel = workModelVocabulary.normalize(job.workModel);
  if (workModel) {
    meta.push({
      key: 'workModel',
      icon: Building2,
      label: workModelVocabulary.label(workModel),
      srLabel: 'Modalidade'
    });
  }

  const jobType = jobTypeVocabulary.normalize(job.jobType);
  if (jobType) {
    meta.push({ key: 'jobType', icon: Briefcase, label: jobTypeVocabulary.label(jobType), srLabel: 'Contratação' });
  }

  const workShift = workShiftVocabulary.normalize(job.workShift);
  if (workShift) {
    meta.push({ key: 'workShift', icon: Clock, label: workShiftVocabulary.label(workShift), srLabel: 'Turno' });
  }

  const experienceLevel = experienceLevelVocabulary.normalize(job.experienceLevel);
  if (experienceLevel) {
    meta.push({
      key: 'experienceLevel',
      icon: GraduationCap,
      label: experienceLevelVocabulary.label(experienceLevel),
      srLabel: 'Experiência'
    });
  }

  const area = jobAreaVocabulary.normalize(job.area);
  if (area) {
    meta.push({ key: 'area', icon: LayoutGrid, label: jobAreaVocabulary.label(area), srLabel: 'Área' });
  }

  if (job.isPcdFriendly) {
    meta.push({ key: 'pcd', icon: Accessibility, label: 'Vaga afirmativa para PcD' });
  }

  // Numa vaga de posição única a frase não acrescenta nada; com várias, quantas restam é o dado
  // que decide se vale a pena candidatar-se hoje.
  if (job.positions > 1) {
    meta.push({
      key: 'positions',
      icon: Users,
      label: `${describeAvailablePositions(job.availablePositions)} de ${job.positions}`,
      strong: job.isActive,
      srLabel: 'Vagas disponíveis'
    });
  }

  if (job.publishedAt ?? job.createdAt) {
    meta.push({ key: 'published', icon: CalendarDays, label: `Publicada ${publishedLabel}`, srLabel: 'Publicação' });
  }

  const requirements = toCardTags(job.requirements ?? []);
  const benefits = toCardTags(job.benefits ?? []);

  return (
    <section>
      <Card className={styles.card}>
        <CardHeader>
          <div className={styles.headerRow}>
            <CardTitle>{job.title}</CardTitle>
            <StatusBadge label={jobStatusLabel(job.status, 'candidate')} tone={jobStatusTone(job.status)} />
          </div>

          {job.summary?.trim() ? <p className={styles.summary}>{job.summary}</p> : null}

          <InfoList className={styles.meta} ariaLabel="Dados da vaga">
            {meta.map(({ key, icon, label, strong, srLabel }) => (
              <InfoItem key={key} icon={icon} strong={strong} srLabel={srLabel}>
                {label}
              </InfoItem>
            ))}
          </InfoList>
        </CardHeader>

        <CardContent className={styles.body}>
          <CardSectionLabel as="h2">Descrição</CardSectionLabel>
          <p className={styles.description}>{job.description?.trim() ? job.description : 'Sem descrição.'}</p>

          {requirements.length > 0 ? (
            <>
              <CardSectionLabel as="h2">Requisitos</CardSectionLabel>
              <TagList tags={requirements} ariaLabel="Requisitos da vaga" />
            </>
          ) : null}

          {benefits.length > 0 ? (
            <>
              <CardSectionLabel as="h2">Benefícios</CardSectionLabel>
              <TagList tags={benefits} ariaLabel="Benefícios da vaga" />
            </>
          ) : null}
        </CardContent>

        <CardFooter className={styles.footer}>
          {hasApplied ? (
            <AppliedBadge />
          ) : (
            <Button
              variant="primary"
              onClick={onApply}
              disabled={!isAuthenticated || isApplying}
              aria-busy={isApplying}
            >
              {isApplying ? (
                <Spinner size="sm" label={null} />
              ) : isAuthenticated ? (
                <actionIcons.apply aria-hidden />
              ) : (
                <actionIcons.signIn aria-hidden />
              )}
              {applyLabel}
            </Button>
          )}
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
