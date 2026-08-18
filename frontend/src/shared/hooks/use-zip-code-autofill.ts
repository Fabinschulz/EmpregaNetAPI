'use client';

import { useFormContext } from '@/shared/context';
import { lookupAddressByZipCode } from '@/shared/api';
import { isCompleteZipCode, maskZipCode } from '@/shared/utils';
import { useCallback, useEffect, useRef, useState } from 'react';

export type ZipCodeAutofillFields = {
  zipCode: string;
  street: string;
  neighborhood: string;
  city: string;
  state: string;
};

export type ZipCodeAutofillStatus = 'idle' | 'loading' | 'filled' | 'notFound' | 'error';

const HINTS: Record<ZipCodeAutofillStatus, string | null> = {
  idle: null,
  loading: 'Buscando endereço…',
  filled: 'Endereço preenchido pelo CEP.',
  notFound: 'CEP não encontrado. Preencha o endereço manualmente.',
  error: 'Não foi possível consultar o CEP agora. Preencha o endereço manualmente.'
};

/**
 * Preenche logradouro, bairro, cidade e UF automaticamente a partir do CEP (ViaCEP).
 *
 * <para>Ligue o `onZipCodeChange` ao `onChange` do campo de CEP e use `hint` para dar
 * retorno ao usuário. A consulta dispara sozinha quando o CEP fica completo (8 dígitos).</para>
 *
 * <b>Decisões de comportamento:</b>
 * <para>• <b>Nunca apaga o que já foi digitado.</b> Só escreve nos campos quando a consulta
 * dá certo. Limpar o endereço a cada CEP incompleto faria o usuário perder o que preencheu
 * à mão ao voltar para corrigir um dígito.</para>
 * <para>• <b>Não toca em número e complemento</b>, que o ViaCEP não conhece.</para>
 * <para>• <b>Cancela a consulta anterior</b> a cada nova digitação. Sem isso, uma resposta
 * lenta de um CEP antigo poderia chegar depois e sobrescrever o endereço do CEP atual.</para>
 */
export function useZipCodeAutofill(fields: ZipCodeAutofillFields) {
  const { setValue } = useFormContext();
  const [status, setStatus] = useState<ZipCodeAutofillStatus>('idle');
  const abortRef = useRef<AbortController | null>(null);

  // Evita atualizar o estado depois que o formulário sai da tela.
  useEffect(() => () => abortRef.current?.abort(), []);

  const onZipCodeChange = useCallback(
    async (rawValue: string) => {
      abortRef.current?.abort();

      const masked = maskZipCode(rawValue);
      setValue(fields.zipCode, masked, { shouldDirty: true, shouldValidate: false });

      if (!isCompleteZipCode(masked)) {
        setStatus('idle');
        return;
      }

      const controller = new AbortController();
      abortRef.current = controller;
      setStatus('loading');

      try {
        const address = await lookupAddressByZipCode(masked, controller.signal);

        if (controller.signal.aborted) return;

        if (!address) {
          setStatus('notFound');
          return;
        }

        const apply = (field: string, value: string) => {
          if (value) setValue(field, value, { shouldDirty: true, shouldValidate: true });
        };

        apply(fields.street, address.street);
        apply(fields.neighborhood, address.neighborhood);
        apply(fields.city, address.city);
        apply(fields.state, address.state);

        setStatus('filled');
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setStatus('error');
      }
    },
    [fields.zipCode, fields.street, fields.neighborhood, fields.city, fields.state, setValue]
  );

  return { status, hint: HINTS[status], isLoading: status === 'loading', onZipCodeChange };
}
