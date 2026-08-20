/**
 * Descrição do aviso de "lista atualizada", ou `undefined` quando não há total para informar.
 *
 * O texto é neutro em gênero de propósito (`3 vagas no total`, `3 candidatos no total`): a mesma
 * mensagem serve as seis listagens, e concordar com cada recurso exigiria um mapa de género só
 * para isso.
 */
export function refreshedListMessage(total: number | undefined, resource: string): string | undefined {
  if (typeof total !== 'number' || total < 0) return undefined;
  if (total === 0) return `Nenhum resultado em ${resource}.`;

  return `${total} ${resource} no total.`;
}
