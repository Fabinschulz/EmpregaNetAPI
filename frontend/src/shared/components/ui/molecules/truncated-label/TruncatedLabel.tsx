'use client';

import { truncateText } from '@/shared/utils';
import { Tooltip, TooltipContent, TooltipTrigger } from '../tooltip';

/** Acima deste comprimento o rótulo é cortado e o texto completo passa para o tooltip. */
const TOOLTIP_THRESHOLD = 215;
const MAX_LABEL_LENGTH = 210;

export type TruncatedLabelProps = {
  text: string;
  threshold?: number;
  maxLength?: number;
};

/**
 * Rótulo de opção que só vira tooltip quando de facto não cabe.
 *
 * <para>Precisa de um `TooltipProvider` acima na árvore - as listas que usam isto já o têm.</para>
 */
export function TruncatedLabel({
  text,
  threshold = TOOLTIP_THRESHOLD,
  maxLength = MAX_LABEL_LENGTH
}: TruncatedLabelProps) {
  if (text.length <= threshold) {
    return <span>{text}</span>;
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span>{truncateText(text, maxLength)}</span>
      </TooltipTrigger>
      <TooltipContent>{text}</TooltipContent>
    </Tooltip>
  );
}
