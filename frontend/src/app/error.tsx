'use client';

import { ErrorFallback } from '@/shared/components';
import { formatAppErrorUserMessage, reportRenderError, type AppRouteError } from '@/shared/utils';
import { useEffect } from 'react';

type SegmentErrorProps = Readonly<{
  error: AppRouteError;
  reset: () => void;
}>;

export default function Error({ error, reset }: SegmentErrorProps) {
  useEffect(() => {
    reportRenderError(error, { source: 'route-error', digest: error.digest });
  }, [error]);

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
