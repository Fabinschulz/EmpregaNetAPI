'use client';

import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useCallback, useLayoutEffect, useRef, useState } from 'react';
import type { ZodType } from 'zod';
import { createUrlSyncState, reconcileQuery, registerWrite, type UrlSyncState } from './url-sync-state';
import { parseUrlSyncedParams, serializeUrlSyncedParams, type UrlSyncedValues } from './url-synced-params-codec';

export type UrlSyncedParams<TValues> = {
  values: TValues;
  resetKey: number;
  onChange: (values: TValues) => void;
  reset: (values: TValues) => void;
};

/**
 * Persiste na URL os valores de um formulário de filtro (React Hook Form), para que recarregar a
 * página ou voltar do detalhe mantenha o filtro.
 *
 * Trabalha em cima do `onChange` que `useFilterFormSync` entrega: a página encadeia o `onChange`
 * deste hook ao seu próprio handler. Paginação fica fora — continua em `usePersistedTablePagination`.
 *
 * O App Router não remonta a página quando só a query muda, então o hook distingue a query que ele
 * mesmo gravou de uma mudança feita por fora (regras em `url-sync-state.ts`, puras e testáveis sem
 * React). Na mudança externa relê a URL e incrementa `resetKey`; a releitura nunca grava na URL, então
 * não há ciclo com `router.replace`. A conciliação roda durante o render, por isso o estado vive em
 * `useState`.
 *
 * @param defaults Valores padrão do formulário — omitidos da URL e usados quando a URL não traz o campo.
 * @param schema Schema do formulário; valor da URL que ele recusa cai no default.
 */
export function useUrlSyncedParams<TValues extends UrlSyncedValues>(
  defaults: TValues,
  schema: ZodType<TValues>
): UrlSyncedParams<TValues> {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const query = searchParams.toString();

  const [state, setState] = useState<UrlSyncState<TValues>>(() =>
    createUrlSyncState(parseUrlSyncedParams(searchParams, defaults, schema), query)
  );

  // Ajuste de estado durante o render (padrão do React para derivar de uma entrada que mudou).
  const reconciled = reconcileQuery(state, query, () => parseUrlSyncedParams(searchParams, defaults, schema));
  if (reconciled.state !== state) setState(reconciled.state);

  const stateRef = useRef(state);
  useLayoutEffect(() => {
    stateRef.current = state;
  });

  const write = useCallback(
    (values: TValues, remount: boolean) => {
      const nextQuery = serializeUrlSyncedParams(values, defaults).toString();
      const { state: next, shouldReplace } = registerWrite(stateRef.current, values, nextQuery, remount);

      stateRef.current = next;
      setState(next);

      if (shouldReplace) {
        router.replace(nextQuery ? `${pathname}?${nextQuery}` : pathname, { scroll: false });
      }
    },
    [defaults, pathname, router]
  );

  const onChange = useCallback((values: TValues) => write(values, false), [write]);
  const reset = useCallback((values: TValues) => write(values, true), [write]);

  return { values: state.values, resetKey: state.resetKey, onChange, reset };
}
