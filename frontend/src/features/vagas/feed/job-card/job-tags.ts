import type { JobFeedItemResponse } from '@/features/vagas/service';
import { toCardTags, type CardTag } from '@/shared/components/ui/molecules/entity-card/card-tags';
import {
    experienceLevelVocabulary,
    jobAreaVocabulary,
    NO_EXPERIENCE_REQUIRED,
    workShiftVocabulary
} from '@/shared/schema';

export function toJobTags(job: JobFeedItemResponse): CardTag[] {
  const tags: CardTag[] = [];

  if (job.isPcdFriendly) {
    tags.push({ key: 'pcd', label: 'Vaga para PcD', tone: 'accent' });
  }

  const experienceLevel = experienceLevelVocabulary.normalize(job.experienceLevel);
  if (experienceLevel) {
    tags.push({
      key: 'experienceLevel',
      label: experienceLevelVocabulary.label(experienceLevel),
      tone: experienceLevel === NO_EXPERIENCE_REQUIRED ? 'accent' : 'default'
    });
  }

  const workShift = workShiftVocabulary.normalize(job.workShift);
  if (workShift) {
    tags.push({ key: 'workShift', label: workShiftVocabulary.label(workShift) });
  }

  const area = jobAreaVocabulary.normalize(job.area);
  if (area) {
    tags.push({ key: 'area', label: jobAreaVocabulary.label(area) });
  }

  tags.push(...toCardTags(job.requirements), ...toCardTags(job.benefits));

  return tags;
}
