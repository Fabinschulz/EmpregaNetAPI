'use client';

import { AlertTriangle, RefreshCw } from 'lucide-react';
import React, { type ErrorInfo, type ReactNode } from 'react';
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui';
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
  private contentRef: React.RefObject<HTMLDivElement | null>;

  constructor(props: GracefullyDegradingErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
    this.contentRef = React.createRef();
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
        <div
          className={styles.wrapper}
          dangerouslySetInnerHTML={{
            __html: this.contentRef.current?.innerHTML ?? ''
          }}
          ref={this.contentRef}
          suppressHydrationWarning
        >
          <Card className={styles.card}>
            <CardHeader className={styles.cardHeader}>
              <div className={styles.iconBadge} aria-hidden>
                <AlertTriangle className={styles.alertIcon} />
              </div>
              <h4 className={styles.bannerTitle}>
                O serviço <span className={styles.bannerStrong}>{fallback ?? ''}</span> encontra-se temporariamente
                indisponível no momento.
              </h4>
              <CardTitle>Algo deu errado</CardTitle>
              <CardDescription>
                Ocorreu um erro ao tentar carregar as informações. Por favor, tente novamente mais tarde.
              </CardDescription>
            </CardHeader>
            <CardContent className={styles.content}>
              {statusCode ? (
                <p className={styles.statusLine}>
                  Status code: <span className={styles.statusCode}>{statusCode}</span>
                </p>
              ) : null}

              {error ? (
                <details className={styles.details}>
                  <summary className={styles.summary}>Detalhes do erro</summary>
                  <p className={styles.errorMessage}>{error.message}</p>
                  {errorInfo ? <pre className={styles.stack}>{errorInfo.componentStack}</pre> : null}
                </details>
              ) : null}

              <div className={styles.action}>
                <Button type="button" variant="primary" onClick={handleOnButton} startIcon={RefreshCw}>
                  Recarregar página
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      );
    }

    return <>{children}</>;
  }
}
