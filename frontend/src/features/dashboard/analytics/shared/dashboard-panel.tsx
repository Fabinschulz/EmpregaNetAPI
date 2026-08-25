'use client';

import {
    actionIcons,
    Button,
    Card,
    CardContent,
    CardHeader,
    Tooltip,
    TooltipContent,
    TooltipTrigger
} from '@/shared/components';
import { cn } from '@/shared/utils';
import { AlertTriangle, ChartNoAxesColumn } from 'lucide-react';
import type { ReactNode } from 'react';
import styles from './dashboard-panel.module.scss';

export type DashboardPanelState = 'pending' | 'refreshing' | 'error' | 'empty' | 'ready';

export type DashboardPanelProps = {
  title: string;
  description?: string;
  /** Explicação de origem/limite do número, no ícone de ajuda ao lado do título. */
  hint?: string;
  /** Controles próprios da seção (granularidade, critério de ranking). */
  actions?: ReactNode;
  state: DashboardPanelState;
  /** Placeholder com a forma do conteúdo desta seção. */
  skeleton: ReactNode;
  errorMessage?: string | null;
  onRetry?: () => void;
  emptyMessage?: string;
  /** Nota de rodapé: soma que não fecha, métrica indisponível, convenção do gráfico. */
  note?: string | null;
  className?: string;
  children: ReactNode;
};

export function DashboardPanel({
  title,
  description,
  hint,
  actions,
  state,
  skeleton,
  errorMessage,
  onRetry,
  emptyMessage = 'Não existem dados suficientes para este período.',
  note,
  className,
  children
}: DashboardPanelProps) {
  return (
    <Card className={cn(styles.panel, className)} aria-busy={state === 'pending' || state === 'refreshing'}>
      <CardHeader className={styles.header}>
        <div className={styles.heading}>
          <div className={styles.titleRow}>
            <h3 className={styles.title}>{title}</h3>
            {hint ? (
              <Tooltip>
                <TooltipTrigger asChild>
                  <button type="button" className={styles.hintButton} aria-label={`Sobre ${title}`}>
                    <actionIcons.info aria-hidden />
                  </button>
                </TooltipTrigger>
                <TooltipContent className={styles.hintContent}>{hint}</TooltipContent>
              </Tooltip>
            ) : null}
          </div>
          {description ? <p className={styles.description}>{description}</p> : null}
        </div>
        {actions ? <div className={styles.actions}>{actions}</div> : null}
      </CardHeader>

      <CardContent className={styles.content}>
        {state === 'pending' ? skeleton : null}

        {state === 'error' ? (
          <div className={styles.feedback} role="alert">
            <AlertTriangle className={styles.feedbackIconDanger} aria-hidden />
            <p className={styles.feedbackTitle}>Não foi possível carregar as métricas.</p>
            {errorMessage ? <p className={styles.feedbackText}>{errorMessage}</p> : null}
            {onRetry ? (
              <Button type="button" variant="outline" size="sm" startIcon={actionIcons.retry} onClick={onRetry}>
                Tentar novamente
              </Button>
            ) : null}
          </div>
        ) : null}

        {state === 'empty' ? (
          <div className={styles.feedback} role="status">
            <ChartNoAxesColumn className={styles.feedbackIcon} aria-hidden />
            <p className={styles.feedbackTitle}>{emptyMessage}</p>
            <p className={styles.feedbackText}>Amplie o período ou remova filtros para ver mais resultados.</p>
          </div>
        ) : null}

        {state === 'ready' || state === 'refreshing' ? (
          <div className={cn(styles.body, state === 'refreshing' && styles.bodyRefreshing)}>{children}</div>
        ) : null}

        {note && (state === 'ready' || state === 'refreshing') ? <p className={styles.note}>{note}</p> : null}
      </CardContent>
    </Card>
  );
}

export function resolvePanelState({
  isPending,
  isFetching,
  isError,
  isEmpty
}: {
  isPending: boolean;
  isFetching: boolean;
  isError: boolean;
  isEmpty: boolean;
}): DashboardPanelState {
  if (isPending) return 'pending';
  if (isError) return 'error';
  if (isEmpty) return 'empty';
  return isFetching ? 'refreshing' : 'ready';
}
