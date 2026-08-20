'use client';

import { toastError, toastSuccess } from '@/shared/utils';
import { useCallback } from 'react';

type CopyFeedback = {
  successTitle: string;
  successDescription?: string;
};


export function useCopyToClipboard({ successTitle, successDescription }: CopyFeedback) {
  return useCallback(
    async (text: string) => {
      try {
        await navigator.clipboard.writeText(text);
        toastSuccess(successTitle, successDescription);
      } catch {
        toastError('Não foi possível copiar', 'O navegador bloqueou o acesso à área de transferência.');
      }
    },
    [successTitle, successDescription]
  );
}
