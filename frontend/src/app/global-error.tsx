'use client';

import { ErrorFallback } from '@/components';
import { formatAppErrorUserMessage, type AppRouteError } from '@/utils';
import './globals.scss';

type GlobalErrorProps = Readonly<{
  error: AppRouteError;
  reset: () => void;
}>;

export default function GlobalError({ error, reset }: GlobalErrorProps) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body className="global-error-body">
        <ErrorFallback
          variant="error"
          statusCode={500}
          title="Ops! Algo deu errado."
          description="Ocorreu um erro inesperado. Tente novamente ou volte mais tarde."
          details={formatAppErrorUserMessage(error)}
          onButtonClick={reset}
        />
      </body>
    </html>
  );
}
