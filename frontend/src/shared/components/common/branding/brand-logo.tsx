import { cn } from '@/shared/utils';
import Image from 'next/image';
import logoOnDark from './assets/logo-on-dark.png';
import logoOnLight from './assets/logo-on-light.png';
import styles from './brand-logo.module.scss';

type BrandLogoProps = {
  className?: string;
  priority?: boolean;
};

/**
 * Brasão da Empresa. Duas variantes (clara/escura) ficam
 * empilhadas no DOM e a troca é feita via CSS (`data-theme`), sem JS
 * de deteção de tema - evita flash e mantém o componente sem 'use client'.
 */
export function BrandLogo({ className, priority = true }: BrandLogoProps) {
  return (
    <span className={cn(styles.root, className)}>
      <Image
        src={logoOnLight}
        alt="Prefeitura de Extrema"
        className={cn(styles.image, styles.onLight)}
        priority={priority}
        quality={100}
      />
      <Image
        src={logoOnDark}
        alt="Prefeitura de Extrema"
        className={cn(styles.image, styles.onDark)}
        priority={priority}
        quality={100}
      />
    </span>
  );
}
