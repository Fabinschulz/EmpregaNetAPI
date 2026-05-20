'use client';

import { InputField } from '@/components';
import { FormProvider } from '@/context';
import { useAuth } from '@/features/auth';
import { useLoginMutation } from '@/services';
import { notifyApiError, startRouterTransition, toastSuccess } from '@/utils';
import { useRouter } from 'next/navigation';
import { LOGIN_FIELDS_GRID_STYLE } from './constants';
import type { LoginDto } from './login-schema';
import { loginDefaultValues, loginSchema } from './login-schema';
import { LoginSubmitButton } from './login-submit-button';

export function LoginForm() {
  const router = useRouter();
  const { setLoggedUser } = useAuth();
  const loginMutation = useLoginMutation();

  async function handleSubmit(data: LoginDto) {
    loginMutation.mutate(data, {
      onSuccess: (res) => {
        setLoggedUser(res);
        toastSuccess('Sessão iniciada com sucesso', 'Bem-vindo de volta. Estamos a preparar o seu painel.');
        startRouterTransition(() => router.push('/dashboard'));
      },
      onError: (err) => {
        notifyApiError(err, 'Início de sessão');
      }
    });
  }

  return (
    <>
      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <div style={LOGIN_FIELDS_GRID_STYLE}>
          <InputField name="email" label="E-mail" type="email" autoComplete="email" required />
          <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
          <LoginSubmitButton />
        </div>
      </FormProvider>
    </>
  );
}
