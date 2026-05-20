'use client';

import { useAuth } from '@/context';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import type { AuthMutationVars } from '../shared';
import {
    requireAuthToken, withDefaultListParams,
    type JobApplicationsAdminListQueryParams,
    type JobApplicationsListQueryParams
} from '../shared';
import { applyToJob, listAll, listMine } from './job-applications-api';

export function useMyJobApplicationsQuery(params?: JobApplicationsListQueryParams) {
  const { token } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.jobApplications.mine(listParams),
    queryFn: () => listMine(requireAuthToken(token), listParams),
    enabled: !!token
  });
}

export function useAllJobApplicationsQuery(params?: JobApplicationsAdminListQueryParams) {
  const { token } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.jobApplications.allList(listParams),
    queryFn: () => listAll(requireAuthToken(token), listParams),
    enabled: !!token
  });
}

export function useApplyToJobMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dto }: AuthMutationVars) => applyToJob(requireAuthToken(token), dto),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobApplications.all });
    }
  });
}
