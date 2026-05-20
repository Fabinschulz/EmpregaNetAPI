'use client';

import { useAuth } from '@/context';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import type { AuthIdMutationVars, AuthIdVars, AuthMutationVars } from '../shared';
import { requireAuthToken, withDefaultListParams, type JobsListQueryParams } from '../shared';
import { closeJob, createJob, deleteJob, getJob, listJobs, updateJob } from './jobs-api';

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
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dto }: AuthMutationVars) => createJob(requireAuthToken(token), dto),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
    }
  });
}

export function useUpdateJobMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: AuthIdMutationVars) => updateJob(requireAuthToken(token), id, dto),
    onSuccess: async (_data, { id }) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.detail(id) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
    }
  });
}

export function useCloseJobMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: AuthIdVars) => closeJob(requireAuthToken(token), id),
    onSuccess: async (_data, { id }) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.detail(id) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.lists() });
    }
  });
}

export function useDeleteJobMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: AuthIdVars) => deleteJob(requireAuthToken(token), id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.jobs.all });
    }
  });
}
