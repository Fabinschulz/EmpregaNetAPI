'use client';

import { useAuth } from '@/context';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../query-keys';
import type { AuthIdMutationVars, AuthIdVars, AuthMutationVars } from '../shared';
import { requireAuthToken, withDefaultListParams, type CompaniesListQueryParams } from '../shared';
import { createCompany, deleteCompany, getCompany, listCompanies, updateCompany } from './companies-api';

export function useCompaniesListQuery(params?: CompaniesListQueryParams) {
  const { token } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.companies.list(listParams),
    queryFn: () => listCompanies(requireAuthToken(token), listParams),
    enabled: !!token
  });
}

export function useCompanyQuery(id: number) {
  const { token } = useAuth();

  return useQuery({
    queryKey: queryKeys.companies.detail(id),
    queryFn: () => getCompany(requireAuthToken(token), id),
    enabled: !!token && Number.isFinite(id) && id > 0
  });
}

export function useCreateCompanyMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dto }: AuthMutationVars) => createCompany(requireAuthToken(token), dto),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.lists() });
    }
  });
}

export function useUpdateCompanyMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: AuthIdMutationVars) => updateCompany(requireAuthToken(token), id, dto),
    onSuccess: async (_data, { id }) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.detail(id) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.lists() });
    }
  });
}

export function useDeleteCompanyMutation() {
  const { token } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: AuthIdVars) => deleteCompany(requireAuthToken(token), id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.all });
    }
  });
}
