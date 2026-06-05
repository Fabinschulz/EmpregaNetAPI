'use client';

import React, { type ErrorInfo, type ReactNode } from 'react';
import { ErrorFallback } from '../error-fallback';
import styles from './error-boundary.module.scss';

export interface GracefullyDegradingErrorBoundaryProps {
  children: ReactNode;
  status?: 'error' | 'success' | 'pending';
  error?: Error | null;
  fallback: string;
  statusCode: number | undefined;
  handleOnButton: () => void;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
  errorInfo?: ErrorInfo;
}

export class GracefullyDegradingErrorBoundary extends React.Component<
  GracefullyDegradingErrorBoundaryProps,
  ErrorBoundaryState
> {
  constructor(props: GracefullyDegradingErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error Boundary capturou um erro:', error, errorInfo);
    this.setState({ error, errorInfo });
  }

  render() {
    const { hasError, error, errorInfo } = this.state;
    const { children, status, fallback, statusCode, handleOnButton } = this.props;

    if (hasError || status === 'error') {
      return (
        <div className={styles.wrapper} suppressHydrationWarning>
          <ErrorFallback
            variant="service"
            serviceName={fallback}
            statusCode={statusCode}
            description="Por favor, contate o suporte ou tente novamente mais tarde."
            details={error?.message}
            diagnostic={errorInfo?.componentStack ?? undefined}
            onButtonClick={handleOnButton}
          />
        </div>
      );
    }

    return <>{children}</>;
  }
}
