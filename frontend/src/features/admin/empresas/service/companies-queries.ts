'use client';

import { useAuth } from '@/context';
import { withDefaultListParams, type CompaniesListQueryParams } from '@/shared/schema';
import { reportMutationApiError, startRouterTransition, toastSuccess } from '@/utils';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { companiesRoutes } from '../companies-routes';
import { companyFormToRequest, type CompanyFormValues } from '../form/company-form-schema';
import { createCompany, deleteCompany, getCompany, listCompanies, updateCompany } from './companies-api';
import { companiesKeys } from './companies-keys';

export function useCompaniesListQuery(params?: CompaniesListQueryParams) {
  const { isAuthenticated } = useAuth();
  const listParams = withDefaultListParams(params);

  return useQuery({
    queryKey: companiesKeys.list(listParams),
    queryFn: () => listCompanies(listParams),
    enabled: isAuthenticated
  });
}

export function useCompanyQuery(id: number) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: companiesKeys.detail(id),
    queryFn: () => getCompany(id),
    enabled: isAuthenticated && Number.isFinite(id) && id > 0
  });
}

export function useCreateCompanyMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);
  const router = useRouter();

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => createCompany(companyFormToRequest(formValue)),
    onSuccess: async (id) => {
      await queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      toastSuccess('Empresa criada', 'Continue de onde parou para completar o cadastro.');
      startRouterTransition(() => router.replace(companiesRoutes.detail(id)));
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'criar empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}

/** Ao salvar, o usuário permanece no formulário; sair da edição é decisão dele, pelo "Voltar". */
export function useUpdateCompanyMutation(companyId: number) {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (formValue: CompanyFormValues) => updateCompany(companyId, companyFormToRequest(formValue)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: companiesKeys.detail(companyId) });
      await queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      toastSuccess('Empresa atualizada', 'As alterações foram gravadas.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'atualizar empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}

export function useDeleteCompanyMutation() {
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const ctx = useMutation({
    mutationFn: (id: number) => deleteCompany(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: companiesKeys.all });
      toastSuccess('Empresa excluída', 'A empresa foi removida.');
    },
    onError: (err) => {
      reportMutationApiError({ err, actionLabel: 'excluir empresa', resource: 'empresa', setApiError });
    }
  });

  return { ...ctx, apiError };
}
