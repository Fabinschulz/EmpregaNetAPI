import { cn } from '@/shared/utils';
import styles from './Spinner.module.scss';

export type SpinnerSize = 'sm' | 'md' | 'lg';

export type SpinnerProps = {
  size?: SpinnerSize;
  className?: string;
  label?: string | null;
};

/**
 * Prefira um skeleton quando a forma do conteúdo é conhecida (lista, formulário, detalhe):
 * ele comunica o que está por vir e evita salto de layout. Use o spinner quando a espera é
 * indeterminada ou a área é pequena demais para um esqueleto fazer sentido.
 */
export function Spinner({ size = 'md', className, label = 'Carregando' }: SpinnerProps) {
  return (
    <span
      className={cn(styles.root, styles[size], className)}
      role={label ? 'status' : undefined}
      aria-hidden={label ? undefined : true}
    >
      <span className={styles.ring} />
      {label ? <span className="sr-only">{label}</span> : null}
    </span>
  );
}
