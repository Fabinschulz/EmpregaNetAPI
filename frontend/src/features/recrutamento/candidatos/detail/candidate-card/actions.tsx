import { actionIcons, Button, entityIcons } from '@/shared/components';
import { formatDate } from '@/shared/utils';
import styles from './candidate-card.module.scss';
import type { CandidateContact } from './candidate-contact';

type CandidateActionsProps = {
  contact: CandidateContact;
  name: string;
  updatedAt: string | null;
};

export function CandidateActions({ contact, name, updatedAt }: CandidateActionsProps) {
  return (
    <footer className={styles.footer}>
      <p className={styles.footerMeta}>
        {updatedAt ? `Atualizado em ${formatDate(updatedAt)}` : 'Sem atualizações no cadastro.'}
      </p>

      <div className={styles.actions}>
        {contact.telHref ? (
          <Button variant="outline" asChild>
            <a href={contact.telHref} aria-label={`Ligar para ${name}`}>
              <actionIcons.phone aria-hidden />
              Ligar
            </a>
          </Button>
        ) : null}

        {contact.mailtoHref ? (
          <Button variant="primary" asChild>
            <a href={contact.mailtoHref} aria-label={`Enviar e-mail para ${name}`}>
              <actionIcons.email aria-hidden />
              Enviar e-mail
            </a>
          </Button>
        ) : (
          <span className={styles.noAction}>
            <entityIcons.candidate aria-hidden className={styles.noActionIcon} />
            Sem e-mail cadastrado
          </span>
        )}
      </div>
    </footer>
  );
}
