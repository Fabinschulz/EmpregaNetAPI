import { parseApiError } from '../helpers';
import { notifyApiError } from '../lib';

type ReportMutationApiErrorOptions = {
  err: unknown;
  actionLabel: string;
  resource: string;
  setApiError: (message: string | null) => void;
};

/**
 * Feedback uniforme para `onError` de mutations (toast + estado local para `<Alert />`).
 */
export function reportMutationApiError({
  err,
  actionLabel,
  resource,
  setApiError
}: ReportMutationApiErrorOptions): void {
  const parsed = parseApiError(err, resource);
  notifyApiError(err, resource, actionLabel);
  setApiError(parsed.message);
}
