'use client';

import { useQueryApiError } from '@/hooks';
import type { ReactNode } from 'react';
import { GracefullyDegradingErrorBoundary } from './error-boundary';

type ApiQueryBoundaryProps = {
  children: ReactNode;
  /** Nome do serviço/recurso exibido no banner do boundary. */
  fallback: string;
  isPending: boolean;
  isError: boolean;
  error: unknown;
  resource?: string;
  onRetry: () => void;
};

/**
 * Liga TanStack Query + `parseApiError` ao `GracefullyDegradingErrorBoundary`.
 */
export function ApiQueryBoundary({
  children,
  fallback,
  isPending,
  isError,
  error,
  resource,
  onRetry
}: ApiQueryBoundaryProps) {
  const { statusCode } = useQueryApiError(error, resource);
  const status = isPending ? 'pending' : isError ? 'error' : 'success';

  return (
    <GracefullyDegradingErrorBoundary
      status={status}
      fallback={fallback}
      statusCode={statusCode}
      handleOnButton={onRetry}
    >
      {children}
    </GracefullyDegradingErrorBoundary>
  );
}
