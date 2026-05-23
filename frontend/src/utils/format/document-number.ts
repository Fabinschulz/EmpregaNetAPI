export function formatDocumentNumber(value: string): string {
  if (!value || !value.replace) return value;

  const cleanValue = value.replace(/\D/g, '');

  if (cleanValue.length <= 11) {
    return cleanValue.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/g, '$1.$2.$3-$4'); // Format CPF
  } else if (cleanValue.length > 11 && cleanValue.length <= 14) {
    return cleanValue.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/g, '$1.$2.$3/$4-$5'); // Format CNPJ
  }

  return 'N/D';
}
