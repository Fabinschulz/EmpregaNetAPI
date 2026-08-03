'use client';

import { useEffect, useRef } from 'react';

/**
 * Propaga para quem consulta a API os parâmetros derivados de um formulário de filtros,
 * sempre que eles mudarem **de valor**.
 *
 * O primeiro render é deliberadamente ignorado: os valores iniciais do formulário já vieram de quem
 * chama (URL, `sessionStorage`, defaults), logo notificá-los disparava uma segunda requisição
 * idêntica à que a página acabou de fazer, no carregamento de **toda** tela de listagem.
 *
 * Estava copiado nos quatro formulários de filtro (empresas, usuários, candidatos, vagas), cada um
 * com o seu `isFirstRun` e a sua lista de dependências mantida à mão.
 *
 * @param params Parâmetros de consulta já traduzidos. Recriar o objeto a cada render é esperado,
 *   a comparação é por valor, não por identidade, e por isso não há lista de dependências a esquecer.
 * @param onChange Notificado com os parâmetros novos.
 */
export function useFilterFormSync<TParams>(params: TParams, onChange: (params: TParams) => void): void {
  const isFirstRun = useRef(true);

  const onChangeRef = useRef(onChange);
  useEffect(() => {
    onChangeRef.current = onChange;
  });

  const paramsRef = useRef(params);
  useEffect(() => {
    paramsRef.current = params;
  });

  const paramsKey = JSON.stringify(params);

  useEffect(() => {
    if (isFirstRun.current) {
      isFirstRun.current = false;
      return;
    }

    onChangeRef.current(paramsRef.current);
  }, [paramsKey]);
}
