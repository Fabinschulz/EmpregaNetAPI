import clsx from 'clsx';
import styles from './user-avatar.module.scss';

type UserAvatarProps = {
  name: string;
  className?: string;
};

export function UserAvatar({ name, className }: UserAvatarProps) {
  const initial = (name.trim().slice(0, 1) || '?').toUpperCase();

  return (
    <span className={clsx(styles.avatar, className)} aria-hidden>
      {initial}
    </span>
  );
}
