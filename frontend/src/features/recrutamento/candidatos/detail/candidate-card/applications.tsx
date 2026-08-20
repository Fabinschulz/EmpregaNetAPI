import { ApplicationStatusBadge } from '@/features/candidaturas/application-status-badge';
import { CardSectionLabel } from '@/shared/components';
import type { CandidateDetailResponse } from '../../service';
import styles from './candidate-card.module.scss';

type CandidateApplicationsProps = {
  applications: CandidateDetailResponse['applications'];
};

export function CandidateApplicationsSection({ applications }: CandidateApplicationsProps) {
  const { total, byStatus } = applications;

  return (
    <section className={styles.section}>
      <CardSectionLabel as="h3">Candidaturas</CardSectionLabel>

      {total === 0 ? (
        <p className={styles.empty}>Nenhuma candidatura registrada.</p>
      ) : (
        <>
          <p className={styles.total}>
            {total} {total === 1 ? 'candidatura' : 'candidaturas'}
          </p>

          <ul className={styles.statusList} aria-label="Candidaturas por situação">
            {byStatus.map((entry) => (
              <li key={entry.status}>
                <ApplicationStatusBadge status={entry.status} count={entry.count} />
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
