export const USER_TYPES = [
  { value: 'Candidate', label: 'Candidato' },
  { value: 'Recruiter', label: 'Recrutador' },
  { value: 'Manager', label: 'Gestor' },
  { value: 'Admin', label: 'Administrador' }
] as const;

export type UserTypeValue = (typeof USER_TYPES)[number]['value'];

export const USER_TYPE_OPTIONS = USER_TYPES.map((t) => ({ value: t.value, label: t.label }));

const LABEL_OR_VALUE_TO_VALUE = new Map<string, UserTypeValue>(
  USER_TYPES.flatMap((t) => [
    [t.value.toLowerCase(), t.value],
    [t.label.toLowerCase(), t.value]
  ])
);

export function normalizeUserTypeValue(input: string | null | undefined): UserTypeValue | '' {
  if (!input) return '';
  return LABEL_OR_VALUE_TO_VALUE.get(input.trim().toLowerCase()) ?? '';
}

export function userTypeLabel(input: string | null | undefined): string {
  if (!input) return '—';
  const value = normalizeUserTypeValue(input);
  return USER_TYPES.find((t) => t.value === value)?.label ?? input;
}
