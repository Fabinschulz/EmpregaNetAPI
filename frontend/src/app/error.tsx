'use client';

import { ErrorFallback } from '@/components';
import { formatAppErrorUserMessage, type AppRouteError } from '@/utils/lib';

type SegmentErrorProps = Readonly<{
  error: AppRouteError;
  reset: () => void;
}>;

export default function Error({ error, reset }: SegmentErrorProps) {
  return (
    <ErrorFallback
      statusCode={500}
      title="Ops! Algo deu errado."
      message={formatAppErrorUserMessage(error)}
      onButtonClick={reset}
    />
  );
}
