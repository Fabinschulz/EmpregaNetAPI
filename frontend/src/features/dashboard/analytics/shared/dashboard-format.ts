
const countFormatter = new Intl.NumberFormat('pt-BR');
const decimalFormatter = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
const compactFormatter = new Intl.NumberFormat('pt-BR', { notation: 'compact', maximumFractionDigits: 1 });

export function formatCount(value: number): string {
  return countFormatter.format(value);
}

export function formatCompact(value: number): string {
  return value < 1000 ? countFormatter.format(value) : compactFormatter.format(value);
}

export function formatPercent(value: number): string {
  const formatted = Number.isInteger(value) ? countFormatter.format(value) : decimalFormatter.format(value);
  return `${formatted}%`;
}

export function formatSignedPercent(value: number): string {
  if (value === 0) return '0%';
  return value > 0 ? `+${formatPercent(value)}` : `−${formatPercent(Math.abs(value))}`;
}

export function formatDecimal(value: number): string {
  return decimalFormatter.format(value);
}

export function formatKpiValue(value: number, unit: string | null | undefined): string {
  return unit === 'percent' ? formatPercent(value) : formatCount(value);
}

export function formatDays(days: number): string {
  return days === 1 ? '1 dia' : `${formatCount(days)} dias`;
}
