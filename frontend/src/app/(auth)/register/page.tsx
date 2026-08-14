import { Register } from '@/features/auth/register';
import { Metadata } from 'next';

export const metadata: Metadata = { title: 'Registrar Conta' };
export default function RegisterPage() {
  return <Register />;
}
