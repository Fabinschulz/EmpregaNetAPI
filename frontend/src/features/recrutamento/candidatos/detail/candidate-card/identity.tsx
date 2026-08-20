import { EntityAvatar, InfoItem, InfoList, StatusBadge } from '@/shared/components';
import { formatDate } from '@/shared/utils';
import { Cake, CalendarDays, MapPin } from 'lucide-react';
import type { CandidateDetailResponse } from '../../service';
import styles from './candidate-card.module.scss';

type CandidateIdentityProps = {
  candidate: CandidateDetailResponse;
  name: string;
  nameId: string;
};

export function CandidateIdentity({ candidate, name, nameId }: CandidateIdentityProps) {
  const location = [candidate.city, candidate.state].filter(Boolean).join(', ');

  return (
    <header className={styles.identity}>
      <EntityAvatar name={name} imageUrl={candidate.profilePicture} size="lg" />

      <div className={styles.identityText}>
        {candidate.userType ? <p className={styles.eyebrow}>{candidate.userType}</p> : null}

        <h2 className={styles.name} id={nameId}>
          {name}
        </h2>

        <InfoList className={styles.facts} ariaLabel="Dados do candidato">
          {location ? (
            <InfoItem icon={MapPin} srLabel="Localização" title={location}>
              {location}
            </InfoItem>
          ) : null}

          {candidate.age !== null ? (
            <InfoItem icon={Cake} srLabel="Idade">
              {candidate.age} anos
            </InfoItem>
          ) : null}

          <InfoItem icon={CalendarDays} srLabel="Cadastrado em">
            Desde {formatDate(candidate.createdAt)}
          </InfoItem>
        </InfoList>
      </div>

      <div className={styles.identityAside}>
        <StatusBadge
          label={candidate.isDeleted ? 'Excluído' : 'Ativo'}
          tone={candidate.isDeleted ? 'negative' : 'positive'}
        />
      </div>
    </header>
  );
}
