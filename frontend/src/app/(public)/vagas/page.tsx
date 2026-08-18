import { JobsFeed } from '@/features/vagas/feed';
import {
    jobsFeedFiltersToApiParams,
    parseJobsFeedFilters,
    searchParamsFromRecord
} from '@/features/vagas/feed/filters';
import { JOBS_FEED_PAGE_SIZE } from '@/features/vagas/service';
// eslint-disable-next-line no-restricted-imports
import { getJobVocabularyCached, getJobsFeedCached } from '@/features/vagas/service/jobs-feed-server';
import type { Metadata } from 'next';

const SITE_NAME = 'EmpregaUAI';

type VagasPageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

export const metadata: Metadata = {
  title: `Vagas de emprego em Extrema e região • ${SITE_NAME}`,
  description:
    'Vagas em indústria, logística, produção e manutenção. Filtre por turno, faixa salarial, ' +
    'experiência exigida e benefícios como fretado e cesta básica.',
  alternates: { canonical: '/vagas' },
  openGraph: {
    title: `Vagas de emprego em Extrema e região • ${SITE_NAME}`,
    description: 'Oportunidades por turno, faixa salarial, experiência e benefícios.',
    type: 'website',
    siteName: SITE_NAME
  }
};

/**
 * Executa a busca no servidor tolerando falha, mas <b>registando</b>, engolir em silêncio
 * deixaria uma API caída invisível para quem opera o sistema.
 */
async function safely<T>(load: () => Promise<T>, label: string): Promise<T | undefined> {
  try {
    return await load();
  } catch (error) {
    console.error(`[vagas] SSR falhou ao carregar ${label}; o cliente vai tentar de novo.`, error);
    return undefined;
  }
}

/**
 * Catálogo público de vagas.
 *
 * Server Component: os filtros vêm da URL e a primeira página é resolvida aqui, então o HTML
 * inicial já sai com vagas, indexável e com o primeiro paint sem espera de rede no cliente.
 * O vocabulário de requisitos e benefícios também é resolvido aqui, evitando que o painel de
 * filtros abra vazio e preencha depois.
 *
 * O `<Suspense>` que o acesso dinâmico a `searchParams` exige (`cacheComponents`) já está no
 * layout do segmento `(public)`, acima do `AppShell`.
 */
export default async function Page({ searchParams }: VagasPageProps) {
  const params = await searchParams;
  const filters = parseJobsFeedFilters(searchParamsFromRecord(params));

  // As duas chamadas são independentes: em série somariam as latências sem motivo.
  const [initialPage, initialVocabulary] = await Promise.all([
    safely(() => getJobsFeedCached(jobsFeedFiltersToApiParams(filters, 1, JOBS_FEED_PAGE_SIZE)), 'feed de vagas'),
    safely(() => getJobVocabularyCached(), 'vocabulário de vagas')
  ]);

  return <JobsFeed initialPage={initialPage} initialFilters={filters} initialVocabulary={initialVocabulary} />;
}
