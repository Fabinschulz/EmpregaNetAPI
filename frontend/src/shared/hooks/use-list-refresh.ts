'use client';

import { notifyApiError, refreshedListMessage, toastSuccess } from '@/shared/utils';
import { useCallback } from 'react';

type RefetchResult = {
  data?: { totalItems?: number } | undefined;
  isError: boolean;
  error: unknown;
};

type UseListRefreshOptions = {
  refetch: () => Promise<RefetchResult>;
  /** Recurso no plural, como em `ApiQueryBoundary` (ex.: `vagas`, `candidatos`). */
  resource: string;
};

export function useListRefresh({ refetch, resource }: UseListRefreshOptions) {
  return useCallback(async () => {
    try {
      const result = await refetch();

      if (result.isError) {
        notifyApiError(result.error, 'atualizar a lista', resource);
        return;
      }

      toastSuccess('Lista atualizada', refreshedListMessage(result.data?.totalItems, resource));
    } catch (error) {
      notifyApiError(error, 'atualizar a lista', resource);
    }
  }, [refetch, resource]);
}
