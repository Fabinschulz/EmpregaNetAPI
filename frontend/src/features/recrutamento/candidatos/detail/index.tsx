'use client';

import { ApiQueryBoundary, Button, DetailPageSkeleton, PageHeader } from '@/shared/components';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { useCandidateQuery } from '../service';
import styles from './candidate-detail.module.scss';
import { CandidateCard } from './candidate-card';

export function CandidateDetailPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: candidate, isPending, isError, error, refetch } = useCandidateQuery(id);

  return (
    <ApiQueryBoundary
      fallback="candidato"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="candidato"
      onRetry={() => void refetch()}
    >
      <section className={styles.page}>
        <PageHeader
          title="Candidato"
          description="Ficha do candidato e situação nos processos seletivos."
          actions={
            <Button variant="outline" asChild>
              <Link href="/recrutamento/candidatos">
                <ArrowLeft aria-hidden />
                Voltar
              </Link>
            </Button>
          }
        />

        {isPending ? <DetailPageSkeleton bodyLines={5} /> : null}
        {candidate ? <CandidateCard candidate={candidate} /> : null}
      </section>
    </ApiQueryBoundary>
  );
}
