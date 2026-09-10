'use client';

import { jobApplicationsKeys } from '@/features/candidaturas/service';
import { withDefaultListParams, type JobsListQueryParams } from '@/shared/schema';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/shared/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { closeJobSuccessCopy } from '../close-job-copy';
import { jobFormToRequest, type JobFormValues } from '../form/job-form-schema';
import { jobsRoutes } from '../jobs-routes';
import { revalidateJobCache } from './jobs-actions';
import {
    closeJob,
    createJob,
    deleteJob,
    getJob,
    getOpenApplicationsCount,
    listJobs,
    listSelectableCompanies,
    updateJob
} from './jobs-api';
import { jobsKeys } from './jobs-keys';

export function useJobsListQuery(params?: JobsListQueryParams) {
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: jobsKeys.list(listParams),
    queryFn: () => listJobs(listParams)
  });
}

export function useJobQuery(id: number) {
  return useQuery({
    queryKey: jobsKeys.detail(id),
    queryFn: () => getJob(id),
    enabled: Number.isFinite(id) && id > 0
  });
}

export function useOpenApplicationsCountQuery(jobId: number, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: jobsKeys.openApplicationsCount(jobId),
    queryFn: () => getOpenApplicationsCount(jobId),
    enabled: (options?.enabled ?? true) && Number.isFinite(jobId) && jobId > 0,
    staleTime: 0,
    gcTime: 0
  });
}

export function useSelectableCompaniesQuery() {
  return useQuery({
    queryKey: jobsKeys.selectableCompanies(),
    queryFn: () => listSelectableCompanies()
  });
}

export function useCreateJobMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: JobFormValues) => createJob(jobFormToRequest(formValue)),
    onSuccess: async (id) => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      toastSuccess('Vaga criada', 'Continue de onde parou para completar a publicação.');
      startRouterTransition(() => router.replace(jobsRoutes.detail(id)));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'criar vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useUpdateJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: JobFormValues) => updateJob(jobId, jobFormToRequest(formValue)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      await revalidateJobCache(jobId);
      toastSuccess('Vaga atualizada', 'As alterações foram gravadas.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}

/**
 * Encerra a vaga. Estado terminal por decisão de produto: não há reabertura, por isso a tela
 * só chama isto depois da confirmação explícita.
 *
 * Além do detalhe e das listas de vagas, invalida as candidaturas: o encerramento move as que
 * estavam em aberto para "Cancelada" na mesma transacção, e a tela de candidatos da vaga
 * mostraria o status antigo até ao próximo refetch.
 */
export function useCloseJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: () => closeJob(jobId),
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      await queryClient.invalidateQueries({ queryKey: jobApplicationsKeys.all });
      // Encerrar é o caso mais crítico: sem revalidar, a vaga segue anunciada como
      // aberta na página pública até o cache expirar.
      await revalidateJobCache(jobId);
      toastSuccess(closeJobSuccessCopy.title, closeJobSuccessCopy.describe(result.affectedApplications));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'encerrar vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteJobMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (id: number) => deleteJob(id),
    onSuccess: async (_data, id) => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.all });
      await revalidateJobCache(id);
      toastSuccess('Vaga excluída', 'A vaga foi removida.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}
