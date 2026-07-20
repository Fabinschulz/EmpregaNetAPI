export function firstName(name: string | undefined | null): string | undefined {
  const trimmed = name?.trim();
  if (!trimmed) return undefined;
  return trimmed.split(/\s+/)[0];
}

export type GreetingDateParts = {
  weekday: string;
  day: string;
  month: string;
  year: string;
};

export function formatGreetingDateParts(date: Date = new Date()): GreetingDateParts {
  const weekday = new Intl.DateTimeFormat('pt-BR', { weekday: 'long' }).format(date);
  const day = new Intl.DateTimeFormat('pt-BR', { day: '2-digit' }).format(date);
  const month = new Intl.DateTimeFormat('pt-BR', { month: 'long' }).format(date);
  const year = new Intl.DateTimeFormat('pt-BR', { year: 'numeric' }).format(date);

  return { weekday, day, month, year };
}
