'use client';

import {
  Alert,
  ApiQueryBoundary,
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  FormFieldsSkeleton,
  PageHeader,
  StatusBadge
} from '@/components';
import { FormProvider } from '@/context';
import {
  adminUserFormValuesFromDto,
  adminUserUpdateFormSchema,
  defaultAdminUserUpdateForm,
  useAdminUserQuery,
  useUpdateAdminUserMutation,
  type AdminUserUpdateFormValues
} from '../service';
import { userTypeLabel } from '@/shared';
import { useParams } from 'next/navigation';
import { useMemo, type ReactNode } from 'react';
import styles from './admin-user-detail.module.scss';
import { AdminUserFormFields } from './admin-user-form';

const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'medium', timeStyle: 'short' });

function formatDateTime(value?: string | null): string {
  if (!value) return '—';
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? '—' : dateTimeFormatter.format(parsed);
}

function initialsOf(name: string): string {
  const letters = name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]);
  return letters.join('').toUpperCase() || 'U';
}

function MetaItem({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className={styles.metaItem}>
      <span className={styles.metaLabel}>{label}</span>
      <span className={styles.metaValue}>{value}</span>
    </div>
  );
}

export function AdminUserDetailPage() {
  const params = useParams<{ id: string }>();
  const id = useMemo(() => Number(params.id), [params.id]);
  const { data: user, isPending, isError, error, refetch } = useAdminUserQuery(id);
  const { apiError, mutateAsync: updateAsync } = useUpdateAdminUserMutation(id);

  const initial = useMemo<AdminUserUpdateFormValues>(() => {
    if (!user) return defaultAdminUserUpdateForm;
    return adminUserFormValuesFromDto(user);
  }, [user]);

  const handleSubmit = async (formValue: AdminUserUpdateFormValues) => await updateAsync(formValue);

  return (
    <ApiQueryBoundary
      fallback="usuário"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="usuário"
      onRetry={() => void refetch()}
    >
      <section>
        <PageHeader title="Usuário" description="Detalhe e alteração do tipo de usuário." />

        {isPending ? <FormFieldsSkeleton fields={6} /> : null}

        {user ? (
          <div className={styles.stack}>
            <Card>
              <CardHeader>
                <div className={styles.identityRow}>
                  <div className={styles.identity}>
                    <div className={styles.avatar} aria-hidden>
                      {initialsOf(user.username)}
                    </div>
                    <div className={styles.identityText}>
                      <p className={styles.name}>{user.username}</p>
                      <p className={styles.email}>{user.email}</p>
                      <div className={styles.badges}>
                        <Badge variant="secondary">{userTypeLabel(user.userType)}</Badge>
                        {user.roles.length ? (
                          user.roles.map((role) => (
                            <Badge key={role} variant="default">
                              {role}
                            </Badge>
                          ))
                        ) : (
                          <Badge variant="secondary">Sem papéis</Badge>
                        )}
                      </div>
                    </div>
                  </div>
                  <StatusBadge
                    label={user.isDeleted ? 'Excluído' : 'Ativo'}
                    tone={user.isDeleted ? 'negative' : 'positive'}
                  />
                </div>
              </CardHeader>
              <CardContent>
                <dl className={styles.meta}>
                  <MetaItem label="ID" value={user.id} />
                  <MetaItem label="Telefone" value={user.phoneNumber?.trim() ? user.phoneNumber : '—'} />
                  <MetaItem label="Criado em" value={formatDateTime(user.createdAt)} />
                  <MetaItem label="Atualizado em" value={formatDateTime(user.updatedAt)} />
                  {user.isDeleted ? <MetaItem label="Excluído em" value={formatDateTime(user.deletedAt)} /> : null}
                </dl>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Alterar tipo de usuário</CardTitle>
                <CardDescription>Define o papel do usuário no sistema.</CardDescription>
              </CardHeader>
              <CardContent>
                {user.isDeleted ? (
                  <Alert variant="default" title="Usuário excluído" className={styles.notice}>
                    A edição do tipo de usuário está indisponível para contas excluídas.
                  </Alert>
                ) : null}
                {apiError ? (
                  <Alert variant="destructive" title="Erro ao salvar" className={styles.notice}>
                    {apiError}
                  </Alert>
                ) : null}
                <FormProvider
                  key={`admin-user-${id}`}
                  validationSchema={adminUserUpdateFormSchema}
                  defaultValues={initial}
                  onSubmit={handleSubmit}
                  readOnly={user.isDeleted}
                >
                  <AdminUserFormFields />
                </FormProvider>
              </CardContent>
            </Card>
          </div>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
