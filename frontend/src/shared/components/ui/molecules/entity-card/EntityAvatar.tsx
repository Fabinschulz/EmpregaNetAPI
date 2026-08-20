import { cn } from '@/shared/utils/lib';
import { entityInitials } from './entity-initials';
import styles from './EntityAvatar.module.scss';

export type EntityAvatarSize = 'sm' | 'md' | 'lg';

export type EntityAvatarProps = {
  name: string;
  imageUrl?: string | null;
  size?: EntityAvatarSize;
  className?: string;
};

const SIZE_CLASS: Record<EntityAvatarSize, string> = {
  sm: styles.sm,
  md: styles.md,
  lg: styles.lg
};

export function EntityAvatar({ name, imageUrl, size = 'sm', className }: EntityAvatarProps) {
  const url = imageUrl?.trim();

  if (url) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={url}
        alt=""
        loading="lazy"
        decoding="async"
        className={cn(styles.avatar, styles.image, SIZE_CLASS[size], className)}
      />
    );
  }

  return (
    <span className={cn(styles.avatar, SIZE_CLASS[size], className)} aria-hidden>
      {entityInitials(name)}
    </span>
  );
}
