'use client';

import { useAuth } from '@/context';
import {
  withDefaultListParams,
  type JobApplicationsAdminListQueryParams,
  type JobApplicationsListQueryParams
} from '@/shared/schema';
import { reportMutationApiError, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { jobApplicationsKeys } from './job-applications-keys';
import { applyToJob, changeStatus, deleteApplication, listAll, listByJob, listMine } from './job-applications-api';
import { applicationStatusLabels, type ApplicationStatus } from './job-applications-schema';

export function useMyJobApplicationsQuery(params?: JobApplicationsListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: jobApplicationsKeys.mine(listParams),
    queryFn: () => listMine(listParams),
    enabled: isAuthenticated
  });
}

export function useAllJobApplicationsQuery(params?: JobApplicationsAdminListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: jobApplicationsKeys.allList(listParams),
    queryFn: () => listAll(listParams),
    enabled: isAuthenticated
  });
}

/** Candidaturas de uma vaga específica (equipe de recrutamento). */
export function useApplicationsByJobQuery(jobId: number, params?: JobApplicationsListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: jobApplicationsKeys.byJob(jobId, listParams),
    queryFn: () => listByJob(jobId, listParams),
    enabled: isAuthenticated && Number.isFinite(jobId) && jobId > 0
  });
}

/** Exclui uma candidatura (equipe de recrutamento). */
export function useDeleteApplicationMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (id: number) => deleteApplication(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: jobApplicationsKeys.all });
      toastSuccess('Candidatura excluída', 'A candidatura foi removida.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir', resource: 'candidatura', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useApplyToJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: () => applyToJob({ jobId }),
    onSuccess: async (res) => {
      await queryClient.invalidateQueries({ queryKey: jobApplicationsKeys.all });
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
      await queryClient.invalidateQueries({ queryKey: jobApplicationsKeys.all });
      toastSuccess('Status atualizado', `Candidatura movida para "${applicationStatusLabels[status]}".`);
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar o status', resource: 'candidatura', setApiError });
    }
  });

  return { ...ctx, apiError };
}
