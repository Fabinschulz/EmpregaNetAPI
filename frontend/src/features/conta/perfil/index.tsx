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
  PageHeader
} from '@/shared/components';
import { FormProvider } from '@/shared/context';
import { roleLabel } from '@/shared/utils';
import { useMeQuery, useUpdateMyProfileMutation } from '../service';
import { profileFormSchema, profileFormValuesFromResponse, type ProfileFormValues } from './profile-form-schema';
import styles from '../conta.module.scss';
import { ProfileFormFields } from './profile-form-fields';

export function ProfilePage() {
  const { data: user, isPending, isError, error, refetch } = useMeQuery();
  const profileMutation = useUpdateMyProfileMutation();

  const handleProfileSubmit = async (values: ProfileFormValues) => {
    await profileMutation.mutateAsync(values);
  };

  return (
    <ApiQueryBoundary
      fallback="perfil"
      isPending={isPending}
      isError={isError}
      error={error}
      resource="perfil"
      onRetry={refetch}
    >
      <section>
        <PageHeader title="Perfil" description="Atualize os seus dados pessoais." />

        {isPending ? <FormFieldsSkeleton fields={4} /> : null}

        {user ? (
          <div className={styles.content}>
            <Card>
              <CardHeader>
                <div className={styles.cardHeaderRow}>
                  <div>
                    <CardTitle>Dados pessoais</CardTitle>
                    <CardDescription>Atualize o seu nome de usuário, e-mail e telefone.</CardDescription>
                  </div>
                  {user.roles.length ? (
                    <div className={styles.badgeGroup} aria-label="Perfil de acesso">
                      {user.roles.map((role) => (
                        <Badge key={role} variant="secondary">
                          {roleLabel(role)}
                        </Badge>
                      ))}
                    </div>
                  ) : null}
                </div>
              </CardHeader>
              <CardContent>
                {profileMutation.apiError ? (
                  <Alert variant="destructive" title="Erro ao salvar">
                    {profileMutation.apiError}
                  </Alert>
                ) : null}
                <FormProvider
                  key={`profile-${user.id}`}
                  validationSchema={profileFormSchema}
                  defaultValues={profileFormValuesFromResponse(user)}
                  onSubmit={handleProfileSubmit}
                >
                  <ProfileFormFields />
                </FormProvider>
              </CardContent>
            </Card>
          </div>
        ) : null}
      </section>
    </ApiQueryBoundary>
  );
}
