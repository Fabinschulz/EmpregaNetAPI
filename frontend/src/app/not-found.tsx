'use client';

import { ErrorFallback } from '@/components';
import { startRouterTransition } from '@/utils';
import { ArrowLeft } from 'lucide-react';
import { useRouter } from 'next/navigation';

export default function NotFoundPage() {
  const router = useRouter();
  return (
    <div className="not-found-page">
      <ErrorFallback
        statusCode={404}
        title="Ops! Página não encontrada"
        buttonText="Voltar para a página inicial"
        message="A página que você está tentando acessar não existe ou foi removida."
        Icon={ArrowLeft}
        onButtonClick={() => startRouterTransition(() => router.push('/'))}
      />
    </div>
  );
}
