import { ForgotPassword } from '@/features/auth/forgot-password';
import { Metadata } from 'next';

export const metadata: Metadata = { title: 'Recuperar Senha' };
export default function ForgotPasswordPage() {
  return <ForgotPassword />;
}
