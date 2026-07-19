'use client';

import { useAuth } from '@/context';
import { withDefaultListParams, type CandidatesListQueryParams } from '@/shared';
import { useQuery } from '@tanstack/react-query';
import { candidatesKeys } from './candidates-keys';
import { getCandidate, listCandidates } from './candidates-api';

export function useCandidatesListQuery(params?: CandidatesListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: candidatesKeys.list(listParams),
    queryFn: () => listCandidates(listParams),
    enabled: isAuthenticated
  });
}

export function useCandidateQuery(id: number) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: candidatesKeys.detail(id),
    queryFn: () => getCandidate(id),
    enabled: isAuthenticated && Number.isFinite(id) && id > 0
  });
}
