'use client';

import { candidateDisplayName, type CandidateDetailResponse } from '../../service';
import { CandidateActions } from './actions';
import { CandidateApplicationsSection } from './applications';
import styles from './candidate-card.module.scss';
import { toCandidateContact } from './candidate-contact';
import { CandidateContactSection } from './contact';
import { CandidateIdentity } from './identity';
import { CandidateRolesSection } from './roles';

type CandidateCardProps = {
  candidate: CandidateDetailResponse;
};

export function CandidateCard({ candidate }: CandidateCardProps) {
  const name = candidateDisplayName(candidate);
  const contact = toCandidateContact(candidate);
  const nameId = `candidate-name-${candidate.id}`;

  return (
    <article className={styles.card} aria-labelledby={nameId} data-inactive={candidate.isDeleted}>
      <CandidateIdentity candidate={candidate} name={name} nameId={nameId} />
      <CandidateContactSection contact={contact} />
      <CandidateRolesSection roles={candidate.roles} />
      <CandidateApplicationsSection applications={candidate.applications} />
      <CandidateActions contact={contact} name={name} updatedAt={candidate.updatedAt} />
    </article>
  );
}
