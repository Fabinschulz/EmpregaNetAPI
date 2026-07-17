'use client';

import { Alert, ApiQueryBoundary, Card, CardContent, FormFieldsSkeleton, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import {
  adminUserFormValuesFromDto,
  adminUserUpdateFormSchema,
  defaultAdminUserUpdateForm,
  useAdminUserQuery,
  useUpdateAdminUserMutation,
  type AdminUserUpdateFormValues
} from '@/services';
import { useParams } from 'next/navigation';
import { useMemo } from 'react';
import { AdminUserFormFields } from './admin-user-form';
import styles from './admin-user-detail.module.scss';

export function AdminUserDetailPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: user, isPending, isError, error, refetch } = useAdminUserQuery(id);
  const {
    apiError,
    mutateAsync: updateAsync,
    isError: isUpdateError,
    isPending: isUpdating,
    error: updateError
  } = useUpdateAdminUserMutation(id);

  const initial = useMemo<AdminUserUpdateFormValues>(() => {
    if (!user) return defaultAdminUserUpdateForm;
    return adminUserFormValuesFromDto(user);
  }, [user]);

  const handleSubmit = async (formValue: AdminUserUpdateFormValues) => await updateAsync(formValue);

  return (
    <ApiQueryBoundary
      fallback="usuário"
      isPending={isPending || isUpdating}
      isError={isError || isUpdateError}
      error={error || updateError}
      resource="usuário"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Admin: Usuário" description="Detalhe e alteração do tipo de usuário." />
        {apiError ? (
          <Alert variant="destructive" title="Erro">
            {apiError}
          </Alert>
        ) : null}
        {isPending ? <FormFieldsSkeleton fields={6} /> : null}
        {user ? (
          <>
            <Card>
              <CardContent>
                <div className={styles.info}>
                  <p>
                    <strong>ID:</strong> {user.id}
                  </p>
                  <p>
                    <strong>Usuário:</strong> {user.username}
                  </p>
                  <p>
                    <strong>E-mail:</strong> {user.email}
                  </p>
                </div>
              </CardContent>
            </Card>

            <FormProvider
              key={`admin-user-${id}`}
              validationSchema={adminUserUpdateFormSchema}
              defaultValues={initial}
              onSubmit={handleSubmit}
            >
              <AdminUserFormFields />
            </FormProvider>
          </>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
