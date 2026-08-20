import type { JobFeedItemResponse } from '@/features/vagas/service';
import { TagList } from '@/shared/components';
import { toJobTags } from './job-tags';

const MAX_VISIBLE = 6;

export function JobCardTags({ job }: { job: JobFeedItemResponse }) {
  return <TagList tags={toJobTags(job)} max={MAX_VISIBLE} ariaLabel="Características da vaga" />;
}
