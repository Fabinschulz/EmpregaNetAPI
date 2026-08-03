import type { JobFeedItemDto } from '@/features/vagas/service';
import {
  experienceLevelVocabulary,
  jobTypeVocabulary,
  NO_EXPERIENCE_REQUIRED,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema';
import { cn } from '@/utils';
import type { LucideIcon } from 'lucide-react';
import { Accessibility, Briefcase, Building2, Clock, GraduationCap, House, MapPin, Sparkles } from 'lucide-react';
import styles from './job-card.module.scss';

type JobCardHighlightsProps = {
  job: JobFeedItemDto;
};

const WORK_MODEL_ICONS: Record<string, LucideIcon> = {
  Remote: House,
  Hybrid: Building2,
  OnSite: MapPin
};

type Highlight = {
  key: string;
  label: string;
  icon: LucideIcon;
  emphasis?: boolean;
};

export function JobCardHighlights({ job }: JobCardHighlightsProps) {
  const highlights: Highlight[] = [];

  const workShift = workShiftVocabulary.normalize(job.workShift);
  if (workShift) {
    highlights.push({
      key: 'workShift',
      label: workShiftVocabulary.label(workShift),
      icon: Clock,
      emphasis: true
    });
  }

  const experienceLevel = experienceLevelVocabulary.normalize(job.experienceLevel);
  if (experienceLevel) {
    highlights.push({
      key: 'experienceLevel',
      label: experienceLevelVocabulary.label(experienceLevel),
      icon: experienceLevel === NO_EXPERIENCE_REQUIRED ? Sparkles : GraduationCap,
      emphasis: experienceLevel === NO_EXPERIENCE_REQUIRED
    });
  }

  const jobType = jobTypeVocabulary.normalize(job.jobType);
  if (jobType) {
    highlights.push({ key: 'jobType', label: jobTypeVocabulary.label(jobType), icon: Briefcase });
  }

  const workModel = workModelVocabulary.normalize(job.workModel);
  if (workModel && workModel !== 'OnSite') {
    highlights.push({
      key: 'workModel',
      label: workModelVocabulary.label(workModel),
      icon: WORK_MODEL_ICONS[workModel] ?? MapPin
    });
  }

  if (job.isPcdFriendly) {
    highlights.push({ key: 'pcd', label: 'Vaga para PcD', icon: Accessibility, emphasis: true });
  }

  if (highlights.length === 0) return null;

  return (
    <ul className={styles.highlights}>
      {highlights.map(({ key, label, icon: Icon, emphasis }) => (
        <li key={key} className={cn(styles.chip, emphasis && styles.chipEmphasis)}>
          <Icon className={styles.chipIcon} aria-hidden />
          {label}
        </li>
      ))}
    </ul>
  );
}
