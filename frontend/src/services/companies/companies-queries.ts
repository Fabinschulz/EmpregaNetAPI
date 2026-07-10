'use client';

import { useAuth } from '@/context';
import { reportMutationApiError, startRouterTransition } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { queryKeys } from '../query-keys';
import { withDefaultListParams, type CompaniesListQueryParams } from '../shared';
import { createCompany, deleteCompany, getCompany, listCompanies, updateCompany } from './companies-api';
import type { CompanyFormValues } from './companies-schema';

export function useCompaniesListQuery(params?: CompaniesListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: queryKeys.companies.list(listParams),
    queryFn: () => listCompanies(listParams),
    enabled: isAuthenticated
  });
}

export function useCompanyQuery(id: number) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: queryKeys.companies.detail(id),
    queryFn: () => getCompany(id),
    enabled: isAuthenticated && Number.isFinite(id) && id > 0
  });
}

export function useCreateCompanyMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => createCompany(formValue),
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
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => updateCompany(companyId, formValue),
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
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: () => deleteCompany(companyId),
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
