'use client';

import { Alert, Button } from '@/components';
import { useAuth } from '@/context';
import {
  StandalonePageFooterPrompt,
  StandalonePageFormActions,
  StandalonePageNavLink,
  StandalonePage
} from '@/components';
import { DEFAULT_POST_LOGIN_PATH, isSafeInternalPath, startRouterTransition } from '@/utils';
import { LayoutDashboard } from 'lucide-react';
import { useRouter, useSearchParams } from 'next/navigation';

export function Unauthorized() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const { username, email } = useAuth();
  const fromParam = searchParams.get('from');
  const blockedPath = isSafeInternalPath(fromParam) ? fromParam : null;
  const displayName = username?.trim() || email?.trim() || 'utilizador';

  const handleGoDashboard = () => {
    startRouterTransition(() => router.replace(DEFAULT_POST_LOGIN_PATH));
  };

  return (
    <StandalonePage
      title="Acesso não autorizado"
      description="A sua conta não tem permissão para acessar a esta área da EmpregaUAI."
      footer={
        <StandalonePageFooterPrompt prompt="Precisa de outra conta?">
          <StandalonePageNavLink href="/login">Iniciar sessão com outro utilizador</StandalonePageNavLink>
        </StandalonePageFooterPrompt>
      }
    >
      <Alert variant="destructive" title="Permissão insuficiente">
        Olá, <strong>{displayName}</strong>. O seu perfil atual não inclui acesso
        {blockedPath ? (
          <>
            {' '}
            a <strong>{blockedPath}</strong>
          </>
        ) : (
          ' a esta secção'
        )}
        . Se acredita que isto é um erro, contacte o administrador da plataforma.
      </Alert>

      <StandalonePageFormActions>
        <Button type="button" variant="primary" startIcon={LayoutDashboard} onClick={handleGoDashboard}>
          Ir para o painel
        </Button>
      </StandalonePageFormActions>
    </StandalonePage>
  );
}
