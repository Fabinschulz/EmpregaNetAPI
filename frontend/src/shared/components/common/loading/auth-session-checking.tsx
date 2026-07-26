import { LoadingState } from './loading-state';

type AuthSessionCheckingProps = {
  message?: string;
};

export function AuthSessionChecking({ message = 'Verificando sessão…' }: AuthSessionCheckingProps) {
  return <LoadingState label={message} />;
}
