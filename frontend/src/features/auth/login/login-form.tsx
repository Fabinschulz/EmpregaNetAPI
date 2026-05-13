'use client';

import { useRouter } from 'next/navigation';
import { InputField } from '@/components';
import { FormProvider } from '@/context';
import { login } from '@/services';
import { useAuth } from '@/features/auth';
import { notifyApiError, startRouterTransition, toastSuccess } from '@/utils/lib';
import { LOGIN_FIELDS_GRID_STYLE } from './constants';
import { loginDefaultValues, loginSchema } from './login-schema';
import type { LoginDto } from './login-schema';
import { LoginSubmitButton } from './login-submit-button';

export function LoginForm() {
  const router = useRouter();
  const { setLoggedUser } = useAuth();

  async function handleSubmit(data: LoginDto) {
    try {
      const res = await login(data);
      setLoggedUser(res);
      toastSuccess('Sessão iniciada com sucesso', 'Bem-vindo de volta. Estamos a preparar o seu painel.');
      startRouterTransition(() => router.push('/dashboard'));
    } catch (err) {
      notifyApiError(err, 'Início de sessão');
    }
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
