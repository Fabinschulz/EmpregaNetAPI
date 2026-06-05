'use client';

import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components';
import type { LucideIcon } from 'lucide-react';
import { AlertTriangle, FileQuestion, RefreshCw } from 'lucide-react';
import styles from './error-fallback.module.scss';

export type ErrorFallbackVariant = 'error' | 'service' | 'not-found';

export type ErrorFallbackProps = {
  variant?: ErrorFallbackVariant;
  statusCode?: number;
  title?: string;
  description?: string;
  /** Nome do recurso/serviço (variant `service`). */
  serviceName?: string;
  /** Texto em painel recolhível (mensagem técnica ou de apoio). */
  details?: string;
  /** Stack ou diagnóstico adicional (boundary / erro de rota). */
  diagnostic?: string;
  buttonText?: string;
  Icon?: LucideIcon;
  onButtonClick: () => void;
};

const VARIANT_DEFAULTS: Record<
  ErrorFallbackVariant,
  { Icon: LucideIcon; buttonText: string; role: 'alert' | 'status' }
> = {
  error: { Icon: RefreshCw, buttonText: 'Tentar novamente', role: 'alert' },
  service: { Icon: RefreshCw, buttonText: 'Recarregar página', role: 'alert' },
  'not-found': { Icon: RefreshCw, buttonText: 'Voltar para a página inicial', role: 'status' }
};

const STATUS_ICON: Record<ErrorFallbackVariant, LucideIcon> = {
  error: AlertTriangle,
  service: AlertTriangle,
  'not-found': FileQuestion
};

export function ErrorFallback({
  variant = 'error',
  statusCode,
  title,
  description,
  serviceName,
  details,
  diagnostic,
  buttonText,
  Icon,
  onButtonClick
}: ErrorFallbackProps) {
  const defaults = VARIANT_DEFAULTS[variant];
  const resolvedButtonText = buttonText ?? defaults.buttonText;
  const ActionIcon = Icon ?? defaults.Icon;
  const StatusIcon = STATUS_ICON[variant];
  const showServiceBanner = variant === 'service' && serviceName != null && serviceName.length > 0;
  
  const showDiagnostics = process.env.NODE_ENV === 'development';
  const hasDetails = showDiagnostics && Boolean(details?.trim());
  const hasDiagnostic = showDiagnostics && Boolean(diagnostic?.trim());
  const isDestructiveVariant = variant === 'error' || variant === 'service';

  return (
    <Card
      className={styles.root}
      data-variant={variant}
      role={defaults.role}
      aria-labelledby="error-fallback-title"
    >
      <CardHeader className={styles.header}>
        <div className={styles.iconBadge} aria-hidden>
          <StatusIcon className={styles.icon} />
        </div>

        {showServiceBanner ? (
          <p className={styles.banner}>
            O serviço <span className={styles.bannerStrong}>{serviceName}</span> encontra-se temporariamente
            indisponível no momento.
          </p>
        ) : null}

       {title && (
         <CardTitle id="error-fallback-title" className={styles.title}>
           {title}
         </CardTitle>
       )}
       {description && (
         <CardDescription className={styles.description}>
           {description}
         </CardDescription>
       )}
      </CardHeader>

      <CardContent className={styles.content}>
        {statusCode != null ? (
          <p className={styles.statusLine}>
            Código: <span className={styles.statusCode}>{statusCode}</span>
          </p>
        ) : null}

        {isDestructiveVariant && (hasDetails || hasDiagnostic) ? (
          <details className={styles.details}>
            <summary className={styles.summary}>Detalhes do erro</summary>
            {hasDetails ? <p className={styles.detailBody}>{details}</p> : null}
            {hasDiagnostic ? <pre className={styles.stack}>{diagnostic}</pre> : null}
          </details>
        ) : null}

        <div className={styles.action}>
          <Button type="button" variant="primary" onClick={onButtonClick} startIcon={ActionIcon}>
            {resolvedButtonText}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
