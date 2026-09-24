'use client';

import { useEffect, useRef } from 'react';

/**
 * Sincroniza um formulário de filtro (React Hook Form) com os parâmetros de consulta da URL.
 * O hook compara os parâmetros de consulta por valor, não por identidade, então a página pode criar
 * novos objetos a cada render sem disparar `onChange` desnecessariamente.
 *
 * @param params Parâmetros de consulta já traduzidos. Recriar o objeto a cada render é esperado,
 *   a comparação é por valor, não por identidade, e por isso não há lista de dependências a esquecer.
 * @param onChange Notificado com os parâmetros novos.
 */
export function useFilterFormSync<TParams>(params: TParams, onChange: (params: TParams) => void): void {
  const paramsKey = JSON.stringify(params);
  const lastNotifiedKey = useRef(paramsKey);

  const onChangeRef = useRef(onChange);
  useEffect(() => {
    onChangeRef.current = onChange;
  });

  const paramsRef = useRef(params);
  useEffect(() => {
    paramsRef.current = params;
  });

  useEffect(() => {
    if (lastNotifiedKey.current === paramsKey) return;

    lastNotifiedKey.current = paramsKey;
    onChangeRef.current(paramsRef.current);
  }, [paramsKey]);
}
