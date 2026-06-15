export function formatDivide(num: number | undefined): number | undefined {
  if (typeof num !== 'number' || isNaN(num) || !isFinite(num)) {
    return undefined;
  }

  return num / 100;
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
