'use client';

import { useAuth } from '@/context';
import {
    withDefaultListParams,
    type JobApplicationsAdminListQueryParams,
    type JobApplicationsListQueryParams
} from '@/shared';
import { reportMutationApiError, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { applyToJob, changeStatus, listAll, listMine } from './job-applications-api';
import { applicationStatusLabels, type ApplicationStatus } from './job-applications-schema';

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

/** Move uma candidatura para outro status do processo seletivo (equipe de recrutamento). */
export function useChangeApplicationStatusMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: ({ id, status }: { id: number; status: ApplicationStatus }) => changeStatus(id, { status }),
    onSuccess: async (_res, { status }) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobApplications.all });
      toastSuccess('Status atualizado', `Candidatura movida para "${applicationStatusLabels[status]}".`);
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar o status', resource: 'candidatura', setApiError });
    }
  });

  return { ...ctx, apiError };
}
