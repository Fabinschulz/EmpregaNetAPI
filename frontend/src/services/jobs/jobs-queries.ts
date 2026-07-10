'use client';

import { reportMutationApiError, startRouterTransition } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { withDefaultListParams, type JobsListQueryParams } from '../shared';
import { closeJob, createJob, deleteJob, getJob, listJobs, updateJob } from './jobs-api';
import type { JobFormValues } from './jobs-schema';

export function useJobsListQuery(params?: JobsListQueryParams) {
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.jobs.list(listParams),
    queryFn: () => listJobs(listParams)
  });
}

export function useJobQuery(id: number) {
  return useQuery({
    queryKey: queryKeys.jobs.detail(id),
    queryFn: () => getJob(id),
    enabled: Number.isFinite(id) && id > 0
  });
}

export function useCreateJobMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: JobFormValues) => createJob(formValue),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
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
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
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
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.detail(jobId) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
      startRouterTransition(() => router.push('/recrutamento/vagas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'encerrar vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteJobMutation(jobId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => deleteJob(jobId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.all });
      startRouterTransition(() => router.push('/recrutamento/vagas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir vaga', resource: 'vaga', setApiError });
    }
  });

  return { ...ctx, apiError };
}
