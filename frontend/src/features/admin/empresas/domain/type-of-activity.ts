import { TYPE_OF_ACTIVITY_VALUES, type TypeOfActivityValue } from '../service/companies-request-schema';

export { TYPE_OF_ACTIVITY_VALUE_SET, TYPE_OF_ACTIVITY_VALUES } from '../service/companies-request-schema';
export type { TypeOfActivityValue } from '../service/companies-request-schema';

const TYPE_OF_ACTIVITY_LABELS: Readonly<Record<TypeOfActivityValue, { label: string; description: string }>> = {
  Industry: { label: 'Indústria', description: 'Indústria' },
  services: { label: 'Serviços', description: 'Serviços' },
  business: { label: 'Comércio', description: 'Comércio' }
};

export const TYPE_OF_ACTIVITY_OPTIONS = TYPE_OF_ACTIVITY_VALUES.map((value) => ({
  value,
  ...TYPE_OF_ACTIVITY_LABELS[value]
}));

const ACTIVITY_LOOKUP = new Map<string, TypeOfActivityValue>(
  TYPE_OF_ACTIVITY_OPTIONS.flatMap(
    (o): Array<[string, TypeOfActivityValue]> => [
      [o.value.toLowerCase(), o.value],
      [o.description.toLowerCase(), o.value]
    ]
  )
);

export function normalizeTypeOfActivity(input: string | null | undefined): TypeOfActivityValue | '' {
  if (!input) return '';
  return ACTIVITY_LOOKUP.get(input.trim().toLowerCase()) ?? '';
}
