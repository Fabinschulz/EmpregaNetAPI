'use client';

import { Alert, Button, Card, CardContent, CardDescription, CardHeader, CardTitle, PageHeader } from '@/components';
import { FormProvider } from '@/context';
import {
  changeMyPasswordFormSchema,
  defaultChangeMyPasswordForm,
  useChangeMyPasswordMutation,
  useDeleteMyAccountMutation,
  type ChangeMyPasswordFormValues
} from '@/services';
import { Trash2 } from 'lucide-react';
import { useState } from 'react';
import styles from '../conta.module.scss';
import { ChangePasswordFormFields } from './change-password-form-fields';

export function SecurityPage() {
  const passwordMutation = useChangeMyPasswordMutation();
  const deleteMutation = useDeleteMyAccountMutation();
  const [confirmingDelete, setConfirmingDelete] = useState(false);

  const handlePasswordSubmit = async (values: ChangeMyPasswordFormValues) => {
    await passwordMutation.mutateAsync(values);
  };

  const handleDelete = () => {
    if (!confirmingDelete) {
      setConfirmingDelete(true);
      return;
    }
    deleteMutation.mutate();
  };

  return (
    <section>
      <PageHeader title="Segurança" description="Altere a sua senha ou encerre a sua conta." />

      <div className={styles.content}>
        <Card>
          <CardHeader>
            <CardTitle>Alterar senha</CardTitle>
            <CardDescription>
              Por segurança, ao alterar a senha todas as sessões abertas são encerradas.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {passwordMutation.apiError ? (
              <Alert variant="destructive" title="Erro ao alterar senha">
                {passwordMutation.apiError}
              </Alert>
            ) : null}
            <FormProvider
              validationSchema={changeMyPasswordFormSchema}
              defaultValues={defaultChangeMyPasswordForm}
              onSubmit={handlePasswordSubmit}
            >
              <ChangePasswordFormFields />
            </FormProvider>
          </CardContent>
        </Card>

        <Card className={styles.dangerZone}>
          <CardHeader>
            <CardTitle>Encerrar conta</CardTitle>
            <CardDescription>
              A sua conta será desativada e você perderá o acesso à plataforma. Esta ação não pode ser desfeita pela
              própria conta.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {deleteMutation.apiError ? (
              <Alert variant="destructive" title="Erro ao encerrar conta">
                {deleteMutation.apiError}
              </Alert>
            ) : null}
            <div className={styles.dangerActions}>
              <Button type="button" variant="destructive" onClick={handleDelete} disabled={deleteMutation.isPending}>
                <Trash2 aria-hidden />
                {confirmingDelete ? 'Confirmar encerramento' : 'Encerrar minha conta'}
              </Button>
              {confirmingDelete && !deleteMutation.isPending ? (
                <Button type="button" variant="outline" onClick={() => setConfirmingDelete(false)}>
                  Cancelar
                </Button>
              ) : null}
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
