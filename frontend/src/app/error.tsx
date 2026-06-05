'use client';

import { ErrorFallback } from '@/components';
import { formatAppErrorUserMessage, type AppRouteError } from '@/utils';

type SegmentErrorProps = Readonly<{
  error: AppRouteError;
  reset: () => void;
}>;

export default function Error({ error, reset }: SegmentErrorProps) {
  return (
    <div className="error-page">
      <ErrorFallback
        variant="error"
        statusCode={500}
        title="Ops! Algo deu errado."
        description="Ocorreu um erro inesperado. Tente novamente ou volte mais tarde."
        details={formatAppErrorUserMessage(error)}
        onButtonClick={reset}
      />
    </div>
  );
}
