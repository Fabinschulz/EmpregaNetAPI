type AuthSessionCheckingProps = {
  message?: string;
};

export function AuthSessionChecking({ message = 'A verificar sessão…' }: AuthSessionCheckingProps) {
  return (
    <div
      style={{
        minHeight: '40vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'var(--muted)'
      }}
      role="status"
    >
      {message}
    </div>
  );
}
