import { Spinner, type SpinnerSize } from '@/shared/components';
import { cn } from '@/shared/utils';
import styles from './loading-state.module.scss';

type LoadingStateProps = {
  label?: string | null;
  fullscreen?: boolean;
  size?: SpinnerSize;
  className?: string;
};

/**
 * Estado de carregamento centralizado (spinner + rótulo), para esperas de forma
 * desconhecida, troca de rota, streaming de layout, verificação de sessão.
 *
 * Só se torna visível após ~250ms: carregamentos rápidos não exibem nada, evitando o
 * flash que faz a interface parecer instável. Quando a forma do conteúdo é conhecida,
 * prefira um skeleton (`ListRowsSkeleton`, `DetailPageSkeleton`, `FormFieldsSkeleton`).
 */
export function LoadingState({ label = 'Carregando…', fullscreen = false, size = 'md', className }: LoadingStateProps) {
  return (
    <div
      className={cn(styles.root, fullscreen && styles.fullscreen, className)}
      role="status"
      aria-live="polite"
      aria-busy="true"
    >
      <Spinner size={size} label={label ? null : 'Carregando'} />
      {label ? <p className={styles.label}>{label}</p> : null}
    </div>
  );
}
