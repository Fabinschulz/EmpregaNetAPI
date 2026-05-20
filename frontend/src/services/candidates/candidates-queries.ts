'use client';

import { useAuth } from '@/context';
import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import { requireAuthToken, withDefaultListParams, type CandidatesListQueryParams } from '../shared';
import { getCandidate, listCandidates } from './candidates-api';

export function useCandidatesListQuery(params?: CandidatesListQueryParams) {
  const { token } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.candidates.list(listParams),
    queryFn: () => listCandidates(requireAuthToken(token), listParams),
    enabled: !!token
  });
}

export function useCandidateQuery(id: number) {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.candidates.detail(id),
    queryFn: () => getCandidate(requireAuthToken(token), id),
    enabled: !!token && Number.isFinite(id) && id > 0
  });
}
