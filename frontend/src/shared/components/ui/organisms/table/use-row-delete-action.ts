'use client';

import { usePermissions } from '@/hooks';
import type { Permission } from '@/utils/lib';
import { Trash2 } from 'lucide-react';
import { useCallback, useState } from 'react';
import type { ConfirmDialogProps } from '../../molecules/alert-dialog';
import type { RowAction } from './RowActions';

export type UseRowDeleteActionOptions<TItem> = {
  permission: Permission;
  resource: string;
  getId: (item: TItem) => number;
  getLabel: (item: TItem) => string;
  deleteById: (id: number, options?: { onSuccess?: () => void }) => void;
  isDeleting: boolean;
  getDescription?: (item: TItem) => string;
};

export type UseRowDeleteActionResult<TItem> = {
  getDeleteAction: (item: TItem) => RowAction | null;
  confirmDialogProps: ConfirmDialogProps;
};

/**
 * Ação "Excluir" de uma linha de tabela: só aparece para quem tem a capacidade
 * correspondente à política do endpoint e sempre passa por confirmação.
 */
export function useRowDeleteAction<TItem>({
  permission,
  resource,
  getId,
  getLabel,
  deleteById,
  isDeleting,
  getDescription
}: UseRowDeleteActionOptions<TItem>): UseRowDeleteActionResult<TItem> {
  const { can } = usePermissions();
  const allowed = can(permission);
  const [pending, setPending] = useState<TItem | null>(null);

  const getDeleteAction = useCallback(
    (item: TItem): RowAction | null =>
      allowed
        ? {
            key: 'delete',
            label: 'Excluir',
            icon: Trash2,
            variant: 'destructive',
            disabled: isDeleting,
            onSelect: () => setPending(item)
          }
        : null,
    [allowed, isDeleting]
  );

  const describe = (item: TItem) =>
    getDescription?.(item) ??
    `Confirmar a exclusão de ${resource} "${getLabel(item)}"? Esta ação não pode ser desfeita.`;

  const confirmDialogProps: ConfirmDialogProps = {
    open: pending !== null,
    onOpenChange: (open) => {
      if (!open) setPending(null);
    },
    title: `Excluir ${resource}`,
    description: pending ? describe(pending) : undefined,
    confirmLabel: 'Excluir',
    cancelLabel: 'Cancelar',
    tone: 'destructive',
    loading: isDeleting,
    onConfirm: () => {
      if (!pending) return;
      deleteById(getId(pending), { onSuccess: () => setPending(null) });
    }
  };

  return { getDeleteAction, confirmDialogProps };
}
