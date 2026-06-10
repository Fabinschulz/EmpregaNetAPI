'use client';

import { Alert } from '@/components';
import { FormProvider } from '@/context';
import { useResetPasswordMutation } from '@/services';
import { useSearchParams } from 'next/navigation';
import { useMemo } from 'react';
import { AuthNavLink, AuthPage } from '../shared';
import { ResetPasswordFormFields } from './reset-password-form';
import type { ResetPasswordFormValues } from './reset-password-schema';
import { resetPasswordDefaultValues, resetPasswordFormSchema } from './reset-password-schema';

export function ResetPassword() {
  const searchParams = useSearchParams();
  const userIdParam = searchParams.get('userId');
  const token = searchParams.get('token') ?? '';

  const userId = useMemo(() => {
    const parsed = Number(userIdParam);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
  }, [userIdParam]);

  const linkValid = userId > 0 && token.length > 0;
  const defaultValues = useMemo(
    () => (linkValid ? resetPasswordDefaultValues(userId, token) : null),
    [linkValid, token, userId]
  );

  const { apiError, mutateAsync, successMessage, isPending } = useResetPasswordMutation();
  const handleSubmit = async (formValue: ResetPasswordFormValues) => await mutateAsync(formValue);

  return (
    <AuthPage
      title="Nova senha"
      description="Defina uma nova senha para a sua conta EmpregaUAI."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          <AuthNavLink href="/login">Voltar ao login</AuthNavLink>
        </>
      }
    >
      {!linkValid && !successMessage && !isPending ? (
        <Alert variant="destructive" title="Link inválido">
          O link de redefinição está incompleto ou expirou. Solicite um novo em{' '}
          <AuthNavLink href="/forgot-password">recuperar senha</AuthNavLink>.
        </Alert>
      ) : linkValid && defaultValues && !successMessage ? (
        <FormProvider
          key={`${userId}-${token}`}
          validationSchema={resetPasswordFormSchema}
          defaultValues={defaultValues}
          onSubmit={handleSubmit}
        >
          <ResetPasswordFormFields />
        </FormProvider>
      ) : null}
    </AuthPage>
  );
}
