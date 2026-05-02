export type AppRouteError = Error & { digest?: string };

export function formatAppErrorUserMessage(error: AppRouteError): string {
  const body =
    error.digest != null ? `${error.message} (digest: ${error.digest})` : error.message;
  return `Erro: ${body}`;
}
