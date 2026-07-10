'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import {
  withDefaultListParams,
  type JobApplicationsAdminListQueryParams,
  type JobApplicationsListQueryParams
} from '../shared';
import { applyToJob, listAll, listMine } from './job-applications-api';

export function useMyJobApplicationsQuery(params?: JobApplicationsListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.jobApplications.mine(listParams),
    queryFn: () => listMine(listParams),
    enabled: isAuthenticated
  });
}

export function useAllJobApplicationsQuery(params?: JobApplicationsAdminListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.jobApplications.allList(listParams),
    queryFn: () => listAll(listParams),
    enabled: isAuthenticated
  });
}

export function useApplyToJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: () => applyToJob({ jobId }),
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
