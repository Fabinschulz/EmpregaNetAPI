import { ResendConfirmation } from '@/features/auth/resend-confirmation';
import { Metadata } from 'next';

export const metadata: Metadata = { title: 'Reenviar Confirmação de Email' };
export default function ResendConfirmationPage() {
  return <ResendConfirmation />;
}
