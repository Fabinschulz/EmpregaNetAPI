import { cn } from '@/utils';
import styles from './job-card.module.scss';

type CompanyAvatarProps = {
  name: string;
  logoUrl?: string | null;
  className?: string;
};

function initialsOf(name: string): string {
  const ignored = new Set(['de', 'da', 'do', 'das', 'dos', 'e', 'ltda', 'me', 'sa', 's/a', 'eireli']);

  const words = name
    .split(/\s+/)
    .map((word) => word.replace(/[^\p{L}\p{N}]/gu, ''))
    .filter((word) => word.length > 0 && !ignored.has(word.toLowerCase()));

  if (words.length === 0) return '?';

  return words
    .slice(0, 2)
    .map((word) => word.charAt(0).toUpperCase())
    .join('');
}

export function CompanyAvatar({ name, logoUrl, className }: CompanyAvatarProps) {
  if (logoUrl) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={logoUrl}
        alt=""
        loading="lazy"
        decoding="async"
        className={cn(styles.avatar, styles.avatarImage, className)}
      />
    );
  }

  return (
    <span className={cn(styles.avatar, className)} aria-hidden>
      {initialsOf(name)}
    </span>
  );
}
