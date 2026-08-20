import { CardSectionLabel, TagList, toCardTags } from '@/shared/components';
import { roleLabel } from '@/shared/utils';
import styles from './candidate-card.module.scss';

export function CandidateRolesSection({ roles }: { roles: readonly string[] }) {
  const tags = toCardTags(roles.map(roleLabel));
  if (tags.length === 0) return null;

  return (
    <section className={styles.section}>
      <CardSectionLabel as="h3">Papéis</CardSectionLabel>
      <TagList tags={tags} ariaLabel="Papéis do candidato" />
    </section>
  );
}
