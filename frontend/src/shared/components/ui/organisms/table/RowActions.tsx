'use client';

import { cn } from '@/utils/lib';
import { MoreVertical, type LucideIcon } from 'lucide-react';
import Link from 'next/link';
import { useState } from 'react';
import { Button } from '../../atoms/button';
import { Popover, PopoverContent, PopoverTrigger } from '../../molecules/popover';
import styles from './RowActions.module.scss';

export type RowAction = {
  /** Identificador estável da ação (key de renderização). */
  key: string;
  label: string;
  /** Ícone (lucide) exibido à esquerda do rótulo. */
  icon?: LucideIcon;
  /** Navegação (renderiza `Link`). Mutuamente exclusivo com `onSelect`. */
  href?: string;
  /** Ação imperativa (ex.: abrir confirmação de exclusão). */
  onSelect?: () => void;
  /** Estilo destrutivo (ex.: "Remover"). */
  variant?: 'default' | 'destructive';
  disabled?: boolean;
};

export type RowActionsProps = {
  actions: RowAction[];
  /**
   * Quantas ações aparecem como botões diretos na linha; o excedente vai para o
   * menu de reticências (⋮). Padrão: 2 (ex.: 5 ações → 2 visíveis + 3 no menu).
   */
  maxInline?: number;
  className?: string;
};

/**
 * Ações de uma linha de tabela: as primeiras `maxInline` viram botões diretos,
 * as demais ficam num menu aberto pelo botão de reticências.
 */
export function RowActions({ actions, maxInline = 2, className }: RowActionsProps) {
  const [menuOpen, setMenuOpen] = useState(false);

  // Se o excedente for só 1 ação, exibi-la direto ocupa o mesmo espaço que o botão do menu.
  const inlineCount = actions.length <= maxInline + 1 ? actions.length : maxInline;
  const inlineActions = actions.slice(0, inlineCount);
  const menuActions = actions.slice(inlineCount);

  return (
    <div className={cn(styles.root, className)}>
      {inlineActions.map((action) => {
        const Icon = action.icon;
        return action.href ? (
          <Button
            key={action.key}
            asChild
            size="sm"
            variant={action.variant === 'destructive' ? 'destructive' : 'default'}
          >
            <Link href={action.href}>
              {Icon ? <Icon aria-hidden /> : null}
              {action.label}
            </Link>
          </Button>
        ) : (
          <Button
            key={action.key}
            type="button"
            size="sm"
            variant={action.variant === 'destructive' ? 'destructive' : 'default'}
            onClick={action.onSelect}
            disabled={action.disabled}
          >
            {Icon ? <Icon aria-hidden /> : null}
            {action.label}
          </Button>
        );
      })}

      {menuActions.length > 0 ? (
        <Popover open={menuOpen} onOpenChange={setMenuOpen}>
          <PopoverTrigger asChild>
            <Button type="button" variant="ghost" size="icon" aria-label="Mais ações">
              <MoreVertical aria-hidden />
            </Button>
          </PopoverTrigger>
          <PopoverContent align="end" className={styles.menu}>
            {menuActions.map((action) => {
              const Icon = action.icon;
              return action.href ? (
                <Link
                  key={action.key}
                  href={action.href}
                  className={cn(styles.menuItem, action.variant === 'destructive' && styles.menuItemDestructive)}
                  onClick={() => setMenuOpen(false)}
                >
                  {Icon ? <Icon className={styles.menuItemIcon} aria-hidden /> : null}
                  {action.label}
                </Link>
              ) : (
                <button
                  key={action.key}
                  type="button"
                  className={cn(styles.menuItem, action.variant === 'destructive' && styles.menuItemDestructive)}
                  onClick={() => {
                    setMenuOpen(false);
                    action.onSelect?.();
                  }}
                  disabled={action.disabled}
                >
                  {Icon ? <Icon className={styles.menuItemIcon} aria-hidden /> : null}
                  {action.label}
                </button>
              );
            })}
          </PopoverContent>
        </Popover>
      ) : null}
    </div>
  );
}
