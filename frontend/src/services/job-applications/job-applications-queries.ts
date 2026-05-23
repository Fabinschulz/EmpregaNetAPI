'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import {
  requireAuthToken,
  withDefaultListParams,
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

export function useApplyToJobMutation(jobId: number) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: () => applyToJob(requireAuthToken(token), { jobId }),
    onSuccess: async (res) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobApplications.all });
      const message = typeof res === 'string' ? res : 'Candidatura enviada.';
      toastSuccess('Candidatura enviada', message);
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'candidatar-se', resource: 'candidatura', setApiError });
    }
  });

  return { ...ctx, apiError };
}
