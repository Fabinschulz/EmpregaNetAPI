/**
 * Palavras que não caracterizam a entidade: partículas de nome de pessoa e sufixos societários.
 * Sem isto, "Freetech Inovation Ltda" viraria "FL" e "Maria da Silva" viraria "MD".
 */
const IGNORED_WORDS = new Set([
  'de',
  'da',
  'do',
  'das',
  'dos',
  'e',
  'ltda',
  'me',
  'mei',
  'epp',
  'sa',
  's/a',
  'eireli'
]);

export function entityInitials(name: string): string {
  const words = name
    .split(/[\s._-]+/)
    .map((word) => word.replace(/[^\p{L}\p{N}]/gu, ''))
    .filter((word) => word.length > 0 && !IGNORED_WORDS.has(word.toLowerCase()));

  if (words.length === 0) return '?';

  return words
    .slice(0, 2)
    .map((word) => word.charAt(0).toUpperCase())
    .join('');
}
