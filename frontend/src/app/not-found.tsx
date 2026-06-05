'use client';

import { ErrorFallback } from '@/components';
import { startRouterTransition } from '@/utils';
import { ArrowLeft } from 'lucide-react';
import { useRouter } from 'next/navigation';

export default function NotFoundPage() {
  const router = useRouter();
  return (
    <div className="error-page">
      <ErrorFallback
        variant="not-found"
        statusCode={404}
        title="Ops! Página não encontrada"
        description="A página que você está tentando acessar não existe ou foi removida."
        buttonText="Voltar para a página inicial"
        Icon={ArrowLeft}
        onButtonClick={() => startRouterTransition(() => router.push('/'))}
      />
    </div>
  );
}
