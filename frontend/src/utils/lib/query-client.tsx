'use client';

import { QueryClient, QueryClientProvider, type QueryKey } from '@tanstack/react-query';
import { useState, type ReactNode } from 'react';

const STALE_TIME_MS = 60_000;

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: STALE_TIME_MS,
        retry: 1,
        refetchOnWindowFocus: false
      }
    }
  });
}

export const invalidateAndRefetch = async (queryClient: QueryClient, queryKey: QueryKey) => {
  await queryClient.invalidateQueries({ queryKey });
  await queryClient.refetchQueries({ queryKey });
};

type QueryProviderProps = {
  children: ReactNode;
};

export function QueryProvider({ children }: QueryProviderProps) {
  const [queryClient] = useState(() => createQueryClient());

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
