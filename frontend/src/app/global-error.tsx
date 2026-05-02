"use client";

import "./globals.scss";
import { ErrorFallback } from "@/components";
import { formatAppErrorUserMessage, type AppRouteError } from "@/utils/lib";

type GlobalErrorProps = Readonly<{
  error: AppRouteError;
  reset: () => void;
}>;

export default function GlobalError({ error, reset }: GlobalErrorProps) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body className="global-error-body">
        <ErrorFallback
          statusCode={500}
          title="Ops! Algo deu errado."
          message={formatAppErrorUserMessage(error)}
          onButtonClick={reset}
        />
      </body>
    </html>
  );
}
