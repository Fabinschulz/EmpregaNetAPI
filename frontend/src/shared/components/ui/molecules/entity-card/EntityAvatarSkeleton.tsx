import { cn } from '@/shared/utils/lib';
import { Skeleton } from '../../atoms/skeleton';
import styles from './EntityAvatar.module.scss';

export type EntityAvatarSkeletonProps = {
  className?: string;
};

export function EntityAvatarSkeleton({ className }: EntityAvatarSkeletonProps) {
  return <Skeleton className={cn(styles.placeholderMd, className)} />;
}
