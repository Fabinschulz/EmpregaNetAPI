'use client';

import { useMyJobApplicationsQuery } from '@/features/candidaturas/service';
import {
    actionIcons,
    Button,
    Card,
    CardContent,
    CardHeader,
    CardTitle,
    entityIcons,
    PageHeader,
    Skeleton
} from '@/shared/components';
import { useAuth } from '@/shared/context';
import Link from 'next/link';
import styles from './candidate-dashboard.module.scss';

const numberFormatter = new Intl.NumberFormat('pt-BR');

export function CandidateDashboard() {
  const { username } = useAuth();
  const { data, isPending, isError } = useMyJobApplicationsQuery();

  const welcome = username ? `Bem-vindo(a), ${username}.` : 'Bem-vindo(a).';

  return (
    <div>
      <PageHeader title="Painel" description={welcome} />

      <section className={styles.section} aria-label="Minha atividade">
        <h2 className={styles.sectionTitle}>Minha atividade</h2>

        <div className={styles.grid}>
          <Card className={styles.card}>
            <CardHeader className={styles.cardHead}>
              <span className={styles.iconWrap}>
                <entityIcons.application aria-hidden />
              </span>
              <CardTitle className={styles.cardTitle}>Minhas candidaturas</CardTitle>
            </CardHeader>

            <CardContent className={styles.cardBody}>
              {isPending ? (
                <Skeleton className={styles.metricSkeleton} aria-hidden />
              ) : (
                <p className={styles.metric}>
                  {isError || data?.totalItems === undefined ? '-' : numberFormatter.format(data.totalItems)}
                </p>
              )}

              <p className={styles.metricHint}>Candidaturas que você enviou.</p>

              <div className={styles.actions}>
                <Button asChild variant="outline" size="sm">
                  <Link href="/candidaturas">
                    <entityIcons.application aria-hidden />
                    Ver candidaturas
                  </Link>
                </Button>
              </div>
            </CardContent>
          </Card>

          <Card className={styles.card}>
            <CardHeader className={styles.cardHead}>
              <span className={styles.iconWrap}>
                <entityIcons.job aria-hidden />
              </span>
              <CardTitle className={styles.cardTitle}>Novas oportunidades</CardTitle>
            </CardHeader>

            <CardContent className={styles.cardBody}>
              <p className={styles.metricHint}>
                Busque por cargo, cidade, turno ou faixa salarial e candidate-se direto do feed.
              </p>

              <div className={styles.actions}>
                <Button asChild variant="primary" size="sm">
                  <Link href="/vagas">
                    <actionIcons.search aria-hidden />
                    Explorar vagas
                  </Link>
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </section>
    </div>
  );
}
