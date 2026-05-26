import { AppProviders } from '@/components/providers';
import type { Metadata } from 'next';
import './globals.scss';

export const metadata: Metadata = {
  title: 'EmpregaUAI',
  description: 'Plataforma de gestão de candidaturas e vagas de emprego em Extrema/MG.',
  keywords: 'emprego, vagas, candidaturas, recrutamento, gestão de talentos',
  authors: [{ name: 'Fábio Correa', url: 'https://github.com/Fabinschulz' }]
};

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body>
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
