import { CardSectionLabel, InfoItem, InfoList } from '@/shared/components';
import { Mail, Phone } from 'lucide-react';
import styles from './candidate-card.module.scss';
import type { CandidateContact } from './candidate-contact';

export function CandidateContactSection({ contact }: { contact: CandidateContact }) {
  return (
    <section className={styles.section}>
      <CardSectionLabel as="h3">Contato</CardSectionLabel>

      <InfoList ariaLabel="Contato do candidato">
        {contact.email && contact.mailtoHref ? (
          <InfoItem icon={Mail} srLabel="E-mail" title={contact.email}>
            <a className={styles.link} href={contact.mailtoHref}>
              {contact.email}
            </a>
          </InfoItem>
        ) : null}

        {contact.phone && contact.telHref ? (
          <InfoItem icon={Phone} srLabel="Telefone">
            <a className={styles.link} href={contact.telHref}>
              {contact.phone}
            </a>
          </InfoItem>
        ) : null}

        {contact.isEmpty ? <InfoItem icon={Mail}>Nenhum contato informado.</InfoItem> : null}
      </InfoList>
    </section>
  );
}
