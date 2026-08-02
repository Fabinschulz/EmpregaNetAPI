'use client';

import { withDefaultListParams, type JobsListQueryParams } from '@/shared/schema';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { revalidateJobCache } from './jobs-actions';
import { closeJob, createJob, deleteJob, getJob, listJobs, listSelectableCompanies, updateJob } from './jobs-api';
import { jobsKeys } from './jobs-keys';
import type { JobFormValues } from './jobs-schema';

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
    mutationFn: (formValue: JobFormValues) => createJob(formValue),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      startRouterTransition(() => router.push('/recrutamento/vagas'));
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
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: JobFormValues) => updateJob(jobId, formValue),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      await revalidateJobCache(jobId);
      startRouterTransition(() => router.push('/recrutamento/vagas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useCloseJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => closeJob(jobId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: jobsKeys.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: jobsKeys.lists() });
      // Encerrar é o caso mais crítico: sem revalidar, a vaga segue anunciada como
      // aberta na página pública até o cache expirar.
      await revalidateJobCache(jobId);
      startRouterTransition(() => router.push('/recrutamento/vagas'));
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
