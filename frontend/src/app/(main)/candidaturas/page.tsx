import { MyApplicationsPage } from '@/features/candidaturas/my';
import { Metadata } from 'next';

export const metadata: Metadata = { title: 'Minhas Candidaturas' };
export default function Page() {
  return <MyApplicationsPage />;
}
