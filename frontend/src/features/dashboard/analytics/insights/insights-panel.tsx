'use client';

import { actionIcons, Button, Card, CardContent, CardHeader } from '@/shared/components';
import { cn } from '@/shared/utils';
import { CircleAlert, CircleCheck, Lightbulb, Sparkles, TrendingUp, Waves } from 'lucide-react';
import { DASHBOARD_INSIGHT_CATEGORIES, type DashboardInsight, type DashboardInsightCategory } from '../../service';
import { InsightsSkeleton } from '../shared/dashboard-skeletons';
import styles from './insights-panel.module.scss';

const CATEGORY_META: Record<DashboardInsightCategory, { label: string; icon: typeof CircleAlert }> = {
  attention: { label: 'Atenção', icon: CircleAlert },
  growth: { label: 'Crescimento', icon: TrendingUp },
  highlight: { label: 'Destaque', icon: Sparkles },
  behavior: { label: 'Comportamento', icon: Waves }
};

const SEVERITY_RANK: Record<DashboardInsight['severity'], number> = { high: 0, medium: 1, low: 2 };

export type InsightsPanelProps = {
  insights: DashboardInsight[];
  isPending: boolean;
  isRefreshing: boolean;
  isError: boolean;
  errorMessage?: string | null;
  onRetry: () => void;
};

export function InsightsPanel({
  insights,
  isPending,
  isRefreshing,
  isError,
  errorMessage,
  onRetry
}: InsightsPanelProps) {
  const groups = DASHBOARD_INSIGHT_CATEGORIES.map((category) => ({
    category,
    items: insights
      .filter((insight) => insight.category === category)
      .sort((a, b) => SEVERITY_RANK[a.severity] - SEVERITY_RANK[b.severity])
  })).filter((group) => group.items.length > 0);

  return (
    <Card className={styles.panel} aria-busy={isPending || isRefreshing}>
      <CardHeader className={styles.header}>
        <span className={styles.headerIcon} aria-hidden>
          <Lightbulb />
        </span>
        <div>
          <h3 className={styles.title}>Insights do período</h3>
          <p className={styles.description}>O que os números deste recorte dizem, e o que pedem ação.</p>
        </div>
      </CardHeader>

      <CardContent className={styles.content}>
        {isPending ? <InsightsSkeleton /> : null}

        {!isPending && isError ? (
          <div className={styles.feedback} role="alert">
            <p className={styles.feedbackTitle}>Não foi possível carregar os insights.</p>
            {errorMessage ? <p className={styles.feedbackText}>{errorMessage}</p> : null}
            <Button type="button" variant="outline" size="sm" startIcon={actionIcons.retry} onClick={onRetry}>
              Tentar novamente
            </Button>
          </div>
        ) : null}

        {!isPending && !isError && groups.length === 0 ? (
          <div className={styles.feedback} role="status">
            <CircleCheck className={styles.feedbackIcon} aria-hidden />
            <p className={styles.feedbackTitle}>Nada fora do padrão neste período.</p>
            <p className={styles.feedbackText}>
              Sem vagas paradas, quedas relevantes de candidatura ou concentração fora do normal.
            </p>
          </div>
        ) : null}

        {!isPending && !isError && groups.length > 0 ? (
          <div className={cn(styles.groups, isRefreshing && styles.groupsRefreshing)}>
            {groups.map(({ category, items }) => {
              const { label, icon: Icon } = CATEGORY_META[category];

              return (
                <section key={category} className={styles.group} aria-labelledby={`insights-${category}`}>
                  <h4 id={`insights-${category}`} className={cn(styles.groupTitle, styles[category])}>
                    <Icon className={styles.groupIcon} aria-hidden />
                    {label}
                  </h4>

                  <ul className={styles.list}>
                    {items.map((insight) => (
                      <li key={insight.code} className={styles.item} data-severity={insight.severity}>
                        <p className={styles.itemTitle}>{insight.title}</p>
                        <p className={styles.itemText}>{insight.message}</p>
                      </li>
                    ))}
                  </ul>
                </section>
              );
            })}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
