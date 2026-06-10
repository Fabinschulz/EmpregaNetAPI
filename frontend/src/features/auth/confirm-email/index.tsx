'use client';

import { Alert } from '@/components';
import { useConfirmEmailMutation } from '@/services';
import { useSearchParams } from 'next/navigation';
import { useEffect, useMemo, useRef } from 'react';
import { AuthNavLink, AuthPage } from '../shared';
import type { ConfirmEmailDto } from './confirm-email-schema';

export function ConfirmEmail() {
  const searchParams = useSearchParams();
  const userIdParam = searchParams.get('userId');
  const token = searchParams.get('token') ?? '';
  const submittedRef = useRef(false);

  const payload = useMemo<ConfirmEmailDto | null>(() => {
    const userId = Number(userIdParam);
    if (!Number.isFinite(userId) || userId <= 0 || !token) return null;
    return { userId, token };
  }, [token, userIdParam]);

  const { apiError, mutateAsync, isPending, successMessage } = useConfirmEmailMutation();

  useEffect(() => {
    if (!payload || submittedRef.current) return;
    submittedRef.current = true;
    void mutateAsync(payload);
    window.history.replaceState(null, '', '/confirm-email');
  }, [mutateAsync, payload]);

  return (
    <AuthPage
      title="Confirmar e-mail"
      description="Estamos a validar o seu endereço de e-mail para activar a conta."
      apiError={apiError}
      successMessage={successMessage}
      footer={
        <>
          <AuthNavLink href="/login">Ir para o login</AuthNavLink>
          {' · '}
          <AuthNavLink href="/resend-confirmation">Reenviar e-mail</AuthNavLink>
        </>
      }
    >
      {!payload && !successMessage && !isPending ? (
        <Alert variant="destructive" title="Link inválido">
          O link de confirmação está incompleto. Peça um novo em{' '}
          <AuthNavLink href="/resend-confirmation">reenviar confirmação</AuthNavLink>.
        </Alert>
      ) : payload && isPending && !successMessage && !apiError ? (
        <p role="status" style={{ margin: 0, color: 'var(--muted)', textAlign: 'center' }}>
          A confirmar o seu e-mail…
        </p>
      ) : null}
    </AuthPage>
  );
}
