export function formatDivide(num: number | undefined): number | undefined {
  if (typeof num !== 'number' || isNaN(num) || !isFinite(num)) {
    return undefined;
  }

  return num / 100;
}

export function formatCurrencyCompact(num: number): string {
  return num.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  });
}

export function formatSalaryRange(
  min: number | null | undefined,
  max: number | null | undefined,
  disclosed: boolean
): string {
  if (!disclosed) return 'A combinar';

  const hasMin = typeof min === 'number';
  const hasMax = typeof max === 'number';

  if (hasMin && hasMax) {
    return min === max ? formatCurrencyCompact(min) : `${formatCurrencyCompact(min)} - ${formatCurrencyCompact(max)}`;
  }

  if (hasMin) return `A partir de ${formatCurrencyCompact(min)}`;
  if (hasMax) return `Até ${formatCurrencyCompact(max)}`;

  return 'A combinar';
}

export function formatCurrency(num: number | undefined): string {
  if (num === undefined || isNaN(num) || !num) return 'R$ 0,00';
  //const dividedValue = formatDivide(num);

  const currency = num?.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });

  return String(currency);
}
