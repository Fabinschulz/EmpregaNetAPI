import { getJobCached } from '@/features/recrutamento/vagas/service/jobs-server';
import { JobDetailPage } from '@/features/vagas/detail';
import type { Metadata } from 'next';
import { notFound } from 'next/navigation';

const SITE_NAME = 'EmpregaUAI';

type JobDetailRouteProps = {
  params: Promise<{ id: string }>;
};

/**
 * Metadata dinâmica por vaga (SEO + preview de compartilhamento). Roda no servidor a nível
 * de rota e entra no `<head>` do HTML inicial. Compartilha o `getJobCached` (`'use cache'`)
 * com a página, então a vaga é buscada uma única vez.
 */
export async function generateMetadata({ params }: JobDetailRouteProps): Promise<Metadata> {
  const { id } = await params;
  const job = await getJobCached(Number(id));

  if (!job) {
    return { title: `Vaga não encontrada • ${SITE_NAME}` };
  }

  const location = job.location?.trim() || 'Extrema/MG';
  const rawDescription = job.description?.trim();
  const description = rawDescription
    ? rawDescription.slice(0, 160)
    : `Vaga de ${job.title} em ${location}. Candidate-se pela ${SITE_NAME}.`;
  const title = `${job.title} • ${SITE_NAME}`;

  return {
    title,
    description,
    openGraph: { title, description, type: 'website', siteName: SITE_NAME },
    twitter: { card: 'summary', title, description }
  };
}

export default async function Page({ params }: JobDetailRouteProps) {
  const { id } = await params;
  const jobId = Number(id);

  if (!Number.isFinite(jobId) || jobId <= 0) notFound();

  const job = await getJobCached(jobId);
  if (!job) notFound();

  // Fluxo único de obtenção de vaga por id no segmento público: o servidor busca (SSR + cache)
  // e entrega a vaga por prop. O componente cliente cuida apenas da interação (candidatura).
  return <JobDetailPage job={job} />;
}
