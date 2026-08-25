import { LoadingState } from '@/shared/components';

/**
 * Boundary de carregamento do segmento do painel.
 *
 * Obrigatório: sem ele, a suspensão da página sobe até o boundary do layout, que envolve o shell
 * inteiro, sidebar e header desmontam e remontam, e o primeiro acesso pisca a tela toda.
 */
export default function DashboardLoading() {
  return <LoadingState />;
}
