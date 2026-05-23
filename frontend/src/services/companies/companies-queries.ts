'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, startRouterTransition } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { requireAuthToken, withDefaultListParams, type CompaniesListQueryParams } from '../shared';
import { createCompany, deleteCompany, getCompany, listCompanies, updateCompany } from './companies-api';
import type { CompanyFormValues } from './companies-schema';

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
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => createCompany(requireAuthToken(token), formValue),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.lists() });
      startRouterTransition(() => router.push('/admin/empresas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'criar empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useUpdateCompanyMutation(companyId: number) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => updateCompany(requireAuthToken(token), companyId, formValue),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.detail(companyId) });
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.lists() });
      startRouterTransition(() => router.push('/admin/empresas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteCompanyMutation(companyId: number) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => deleteCompany(requireAuthToken(token), companyId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.companies.all });
      startRouterTransition(() => router.push('/admin/empresas'));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}
