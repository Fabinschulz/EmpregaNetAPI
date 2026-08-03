'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { useDebouncedValue } from './use-debounced-value';

export type DebouncedDraftOptions = {
  /** Valor já confirmado, vindo de fora (formulário, URL, servidor). */
  value: string;
  /** Chamado com o valor aparado quando o rascunho assenta, ou imediatamente via `commitNow`. */
  onCommit: (value: string) => void;
  delayMs?: number;
};

export type DebouncedDraft = {
  /** O que está no campo agora, incluindo o que o utilizador ainda está a digitar. */
  draft: string;
  /** Atualiza o rascunho sem confirmar (ligar ao `onChange` do input). */
  setDraft: (value: string) => void;
  /** Confirma já, sem esperar o debounce (seleção numa lista, submit explícito). */
  commitNow: (value: string) => void;
  /** `true` enquanto o rascunho difere do valor confirmado - serve de indicador de carregamento. */
  isPending: boolean;
};


export function useDebouncedDraft({ value, onCommit, delayMs = 350 }: DebouncedDraftOptions): DebouncedDraft {
  const [draft, setDraft] = useState(value);
  const debounced = useDebouncedValue(draft, delayMs);
  const lastCommittedRef = useRef(value);

  const onCommitRef = useRef(onCommit);
  useEffect(() => {
    onCommitRef.current = onCommit;
  });

  const commitNow = useCallback((next: string) => {
    const trimmed = next.trim();

    setDraft(next);
    lastCommittedRef.current = trimmed;
    onCommitRef.current(trimmed);
  }, []);

  useEffect(() => {
    const trimmed = debounced.trim();

    if (trimmed === lastCommittedRef.current) {
      return;
    }

    lastCommittedRef.current = trimmed;
    onCommitRef.current(trimmed);
  }, [debounced]);

  useEffect(() => {
    if (value === lastCommittedRef.current) {
      return;
    }

    lastCommittedRef.current = value;
    setDraft(value);
  }, [value]);

  return {
    draft,
    setDraft,
    commitNow,
    isPending: draft.trim() !== value
  };
}
