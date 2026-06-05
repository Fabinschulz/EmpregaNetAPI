import { cn } from '@/utils';
import styles from './empregauai-logo.module.scss';

type EmpregaUaiLogoProps = {
  className?: string;
  size?: number;
};

export function EmpregaUaiLogo({ className, size = 40 }: EmpregaUaiLogoProps) {
  const iconSize = Math.round(size * 0.52);

  return (
    <span
      className={cn(styles.root, className)}
      style={{ width: size, height: size }}
      role="img"
      aria-label="EmpregaUAI"
    >
      <svg
        className={styles.icon}
        width={iconSize}
        height={iconSize}
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden
        focusable="false"
      >
        <path
          d="M8 7V6a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v1"
          stroke="currentColor"
          strokeWidth="1.75"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M5 9h14a1 1 0 0 1 1 1v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2v-8a1 1 0 0 1 1-1Z"
          stroke="currentColor"
          strokeWidth="1.75"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M5 12h14"
          stroke="currentColor"
          strokeWidth="1.75"
          strokeLinecap="round"
        />
        <circle cx="17.5" cy="6.5" r="2.25" fill="currentColor" />
        <path
          d="M17.5 4.8v3.4M15.8 6.5h3.4"
          stroke="var(--brand)"
          strokeWidth="0.9"
          strokeLinecap="round"
        />
      </svg>
    </span>
  );
}
