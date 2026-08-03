import styles from './job-card.module.scss';

type JobCardTagsProps = {
  requirements: readonly string[];
  benefits: readonly string[];
};

const MAX_VISIBLE = 5;

export function JobCardTags({ requirements, benefits }: JobCardTagsProps) {
  const all = [...benefits, ...requirements];
  if (all.length === 0) return null;

  const visible = all.slice(0, MAX_VISIBLE);
  const hidden = all.slice(MAX_VISIBLE);

  return (
    <ul className={styles.tags}>
      {visible.map((tag) => (
        <li key={tag} className={styles.tag}>
          {tag}
        </li>
      ))}

      {hidden.length > 0 ? (
        <li className={styles.tagMore} title={hidden.join(', ')}>
          +{hidden.length}
        </li>
      ) : null}
    </ul>
  );
}
