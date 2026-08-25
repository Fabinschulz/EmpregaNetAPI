export const CHART_SERIES_COLORS = [
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
  'var(--chart-6)'
] as const;

/** Categoria residual ("Outras"), deliberadamente neutra. */
const CHART_REST_COLOR = 'var(--chart-rest)';

const CHART_SEMANTIC_COLORS = {
  positive: 'var(--chart-positive)',
  negative: 'var(--chart-negative)',
  warning: 'var(--chart-warning)',
  neutral: 'var(--chart-neutral)'
} as const;


export function seriesColor(index: number, key?: string): string {
  if (key === 'Others') return CHART_REST_COLOR;
  return CHART_SERIES_COLORS[index % CHART_SERIES_COLORS.length];
}

/**
 * Cor por status de candidatura.
 */
export function statusColor(status: string): string {
  switch (status) {
    case 'Approved':
      return CHART_SEMANTIC_COLORS.positive;
    case 'Rejected':
    case 'Error':
      return CHART_SEMANTIC_COLORS.negative;
    case 'Pending':
    case 'Timeout':
      return CHART_SEMANTIC_COLORS.warning;
    case 'Processing':
      return 'var(--chart-1)';
    case 'Finished':
      return 'var(--chart-5)';
    default:
      return CHART_SEMANTIC_COLORS.neutral;
  }
}

export const CHART_AXIS_PROPS = {
  stroke: 'var(--chart-axis)',
  tick: { fill: 'var(--chart-axis)', fontSize: 12 },
  tickLine: false,
  axisLine: false
} as const;

export const CHART_GRID_PROPS = {
  stroke: 'var(--chart-grid)',
  strokeDasharray: '3 3',
  vertical: false
} as const;

/** Altura padrão dos gráficos, em px. Mantém as seções alinhadas na grade. */
export const CHART_HEIGHT = 260;

/** Altura reduzida, para gráficos que dividem a linha com outro. */
export const CHART_HEIGHT_COMPACT = 220;

/**
 * Altura do gráfico protagonista da tela (a evolução do período).
 */
export const CHART_HEIGHT_HERO = 300;
export const CHART_HEIGHT_SPARSE = 200;
export const SPARSE_SERIES_THRESHOLD = 4;
