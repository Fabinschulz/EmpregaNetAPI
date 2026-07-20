/**
 * Define um valor em um objeto por um caminho com pontos (ex.: "address.street"),
 * criando os níveis intermediários se necessário. Usado pelos steps para permitir
 * que o Gherkin sobrescreva um único campo (inclusive aninhado) de um objeto-base
 * válido, mantendo cada cenário focado em uma única regra.
 */
export function setByPath(target: Record<string, unknown>, path: string, value: unknown): void {
  const parts = path.split('.');
  let current = target;

  for (let i = 0; i < parts.length - 1; i++) {
    const key = parts[i];
    if (typeof current[key] !== 'object' || current[key] === null) {
      current[key] = {};
    }
    current = current[key] as Record<string, unknown>;
  }

  current[parts[parts.length - 1]] = value;
}

/** Lê um valor de um objeto por um caminho com pontos (ex.: "address.city"). */
export function getByPath(source: Record<string, unknown>, path: string): unknown {
  return path.split('.').reduce<unknown>((current, key) => {
    if (current == null || typeof current !== 'object') return undefined;
    return (current as Record<string, unknown>)[key];
  }, source);
}
