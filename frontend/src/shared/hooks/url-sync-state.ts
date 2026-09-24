export type UrlSyncState<TValues> = {
  readonly values: TValues;
  readonly resetKey: number;
  readonly query: string;
  /** Queries gravadas pelo hook que o router ainda não devolveu, da mais antiga à mais recente. */
  readonly pendingWrites: readonly string[];
};

export type ReconcileQueryResult<TValues> = {
  state: UrlSyncState<TValues>;
  /** `true` quando a query não foi gravada pelo hook e os valores foram relidos dela. */
  external: boolean;
};

export type RegisterWriteResult<TValues> = {
  state: UrlSyncState<TValues>;
  /** `false` quando a query não mudaria (ex.: só um espaço a mais na busca, que a serialização remove). */
  shouldReplace: boolean;
};

export function createUrlSyncState<TValues>(values: TValues, query: string): UrlSyncState<TValues> {
  return { values, resetKey: 0, query, pendingWrites: [] };
}

export function lastKnownQuery<TValues>(state: UrlSyncState<TValues>): string {
  return state.pendingWrites.at(-1) ?? state.query;
}

/**
 * Concilia o estado com a query atual do router.
 *
 * O hook distingue a query que ele mesmo gravou de uma mudança feita por fora (regras aqui, puras e testáveis sem React).
 * Na mudança externa relê a URL e incrementa `resetKey`; a releitura nunca grava na URL, então não há ciclo com `router.replace`.
 * A conciliação roda durante o render, por isso o estado vive em `useState`.
 *
 * @param readValues Lê os valores da URL atual; só é chamado na mudança externa.
 */
export function reconcileQuery<TValues>(
  state: UrlSyncState<TValues>,
  query: string,
  readValues: () => TValues
): ReconcileQueryResult<TValues> {
  if (query === state.query) return { state, external: false };

  const writtenAt = state.pendingWrites.lastIndexOf(query);
  if (writtenAt >= 0) {
    return {
      state: { ...state, query, pendingWrites: state.pendingWrites.slice(writtenAt + 1) },
      external: false
    };
  }

  return {
    state: { values: readValues(), resetKey: state.resetKey + 1, query, pendingWrites: [] },
    external: true
  };
}

/**
 * Registra uma escrita do hook: troca os valores e, se a query muda em relação à última conhecida,
 * enfileira-a como pendente e pede o `router.replace`.
 *
 * A comparação é com a última escrita pendente (e não só com a query atual): trocar e desfazer um
 * filtro antes de o router responder registra as duas escritas, senão a volta pareceria externa.
 *
 * @param remount `true` quando os valores vêm de fora do formulário (`reset`) e ele precisa remontar.
 */
export function registerWrite<TValues>(
  state: UrlSyncState<TValues>,
  values: TValues,
  nextQuery: string,
  remount = false
): RegisterWriteResult<TValues> {
  const shouldReplace = nextQuery !== lastKnownQuery(state);

  return {
    state: {
      values,
      resetKey: remount ? state.resetKey + 1 : state.resetKey,
      query: state.query,
      pendingWrites: shouldReplace ? [...state.pendingWrites, nextQuery] : state.pendingWrites
    },
    shouldReplace
  };
}
