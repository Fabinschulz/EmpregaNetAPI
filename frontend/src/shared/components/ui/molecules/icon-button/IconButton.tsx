'use client';

import type { LucideIcon } from 'lucide-react';
import * as React from 'react';
import { Button, type ButtonProps } from '../../atoms/button';
import { Tooltip, TooltipContent, TooltipTrigger } from '../tooltip';

export type IconButtonProps = Omit<ButtonProps, 'size' | 'startIcon' | 'endIcon' | 'children' | 'aria-label'> & {
  /** Ícone da ação - preferir uma chave de `actionIcons`, não um import solto de lucide. */
  icon: LucideIcon;
  label: string;
  /** Texto do tooltip quando ele deve dizer mais do que o nome acessível. */
  tooltip?: string;
  showTooltip?: boolean;
  /** Classe aplicada ao próprio ícone, para o raro caso em que a moldura pede outra medida. */
  iconStyleOverrides?: string;
};

export const IconButton = React.forwardRef<HTMLButtonElement, IconButtonProps>(function IconButton(
  { icon: Icon, label, tooltip, showTooltip = true, iconStyleOverrides, variant = 'ghost', type = 'button', ...props },
  ref
) {
  const button = (
    <Button ref={ref} type={type} variant={variant} size="icon" aria-label={label} {...props}>
      <Icon className={iconStyleOverrides} aria-hidden />
    </Button>
  );

  if (!showTooltip) return button;

  return (
    <Tooltip>
      <TooltipTrigger asChild>{button}</TooltipTrigger>
      <TooltipContent>{tooltip ?? label}</TooltipContent>
    </Tooltip>
  );
});
