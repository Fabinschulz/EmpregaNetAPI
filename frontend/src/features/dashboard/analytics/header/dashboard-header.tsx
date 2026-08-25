'use client';

import { actionIcons, Button, PageHeader, Spinner } from '@/shared/components';
import { Building2, CalendarDays, Clock3 } from 'lucide-react';
import type { DashboardMeta } from '../../service';
import styles from './dashboard-header.module.scss';

export type DashboardHeaderProps = {
  meta: DashboardMeta | undefined;
  isRefreshing: boolean;
  onRefresh: () => void;
};

export function DashboardHeader({ meta, isRefreshing, onRefresh }: DashboardHeaderProps) {
  return (
    <div className={styles.root}>
      <PageHeader
        title="Métricas de recrutamento"
        description="Acompanhe vagas, candidaturas e candidatos da operação de recrutamento."
        actions={
          <Button type="button" variant="outline" onClick={onRefresh} disabled={isRefreshing} aria-busy={isRefreshing}>
            {isRefreshing ? <Spinner size="sm" label={null} /> : <actionIcons.refresh aria-hidden />}
            Atualizar
          </Button>
        }
      />

      {meta ? (
        <div className={styles.meta}>
          <span className={styles.metaItem}>
            <CalendarDays className={styles.metaIcon} aria-hidden />
            <strong className={styles.metaStrong}>{meta.periodLabel}</strong>
            {meta.from} - {meta.to}
          </span>

          <span className={styles.metaItem}>
            <Building2 className={styles.metaIcon} aria-hidden />
            {meta.scope.level === 'platform' ? 'Toda a plataforma' : (meta.scope.companyName ?? 'Sua empresa')}
          </span>

          <span className={styles.metaUpdated}>
            <Clock3 className={styles.metaIcon} aria-hidden />
            Atualizado {meta.generatedAt}
          </span>
        </div>
      ) : null}
    </div>
  );
}
