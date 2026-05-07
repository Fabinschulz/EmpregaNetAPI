import { startTransition } from "react";

/**
 * Encapsula navegação do App Router numa Transition, para não bloquear
 * interações urgentes durante a mudança de rota. Ver:
 * https://react.dev/reference/react/useTransition#building-a-suspense-enabled-router
 */
export function startRouterTransition(navigate: () => void): void {
  startTransition(navigate);
}
