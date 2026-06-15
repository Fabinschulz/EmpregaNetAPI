import clsx, { type ClassValue } from 'clsx';

/** Junta classes (padrão shadcn, sem Tailwind — só `clsx`). */
export function cn(...inputs: ClassValue[]) {
  return clsx(inputs);
}
