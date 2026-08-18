/** De onde veio a falha, para separar erro de componente de erro de rota nos painéis. */
export type RenderErrorSource = 'error-boundary' | 'route-error' | 'global-error';

export type RenderErrorContext = {
  componentStack?: string;
  digest?: string;
  source: RenderErrorSource;
};

export function reportRenderError(error: unknown, context: RenderErrorContext): void {
  const normalized = error instanceof Error ? error : new Error(String(error));

  try {
    console.error(`[render:${context.source}] ${normalized.message}`, {
      name: normalized.name,
      digest: context.digest,
      stack: normalized.stack,
      componentStack: context.componentStack
    });
  } catch {
    // Reportar nunca pode piorar o erro que estamos a reportar.
  }
}
