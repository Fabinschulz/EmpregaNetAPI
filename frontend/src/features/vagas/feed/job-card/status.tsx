'use client';

import { InfoItem, InfoList } from '@/shared/components';
import { useRelativeTime } from '@/shared/hooks';
import { CalendarDays, Users } from 'lucide-react';
import styles from './job-card.module.scss';

type JobCardStatusProps = {
  publishedAt: string;
  applicationsCount: number;
};

export function JobCardStatus({ publishedAt, applicationsCount }: JobCardStatusProps) {
  const publishedLabel = useRelativeTime(publishedAt);

  return (
    <InfoList className={styles.footerMeta} ariaLabel="Situação da vaga">
      <InfoItem icon={CalendarDays} srLabel="Publicada">
        <time dateTime={publishedAt}>{publishedLabel}</time>
      </InfoItem>

      {applicationsCount > 0 ? (
        <InfoItem icon={Users} srLabel="Candidaturas recebidas">
          {applicationsCount} {applicationsCount === 1 ? 'candidato' : 'candidatos'}
        </InfoItem>
      ) : null}
    </InfoList>
  );
}
