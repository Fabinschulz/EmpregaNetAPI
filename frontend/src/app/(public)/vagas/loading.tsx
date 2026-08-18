import { LoadingState } from '@/shared/components';

/**
 * Boundary do segmento de vagas.
 *
 * O `loading.tsx` do layout envolve o shell inteiro; este cria um boundary **dentro** dele, em
 * volta apenas da página. Sem ele, entrar em `/vagas` pela primeira vez desmontaria sidebar e
 * header enquanto o chunk carrega, causando piscada de tela cheia.
 */
export default function Loading() {
  return <LoadingState label="Carregando vagas" />;
}
