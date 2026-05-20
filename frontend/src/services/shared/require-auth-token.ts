const UNAUTHENTICATED_MESSAGE = 'Sessão inválida. Faça login novamente.';

/** Garante token presente antes de chamar a API (mutations). */
export function requireAuthToken(token: string | null): string {
  if (!token) {
    throw new Error(UNAUTHENTICATED_MESSAGE);
  }
  return token;
}
