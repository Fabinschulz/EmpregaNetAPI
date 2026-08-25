'use client';

import * as AlertDialogPrimitive from '@radix-ui/react-alert-dialog';
import type { LucideIcon } from 'lucide-react';
import * as React from 'react';
import { Button } from '../../atoms/button';
import { Spinner } from '../../atoms/spinner';
import { actionIcons } from '../../icons';
import styles from './alert-dialog.module.scss';

export type ConfirmDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: React.ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmIcon?: LucideIcon;
  tone?: 'default' | 'destructive';
  loading?: boolean;
  onConfirm: () => void | Promise<void>;
};

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  confirmIcon,
  tone = 'default',
  loading = false,
  onConfirm
}: ConfirmDialogProps): React.JSX.Element {
  const handleConfirm = () => {
    void onConfirm();
  };

  const ConfirmIcon = confirmIcon ?? actionIcons.confirm;

  return (
    <AlertDialogPrimitive.Root open={open} onOpenChange={onOpenChange}>
      <AlertDialogPrimitive.Portal>
        <AlertDialogPrimitive.Overlay className={styles.overlay} />
        <AlertDialogPrimitive.Content className={styles.content}>
          <AlertDialogPrimitive.Title className={styles.title}>{title}</AlertDialogPrimitive.Title>
          {description ? (
            <AlertDialogPrimitive.Description className={styles.description}>
              {description}
            </AlertDialogPrimitive.Description>
          ) : null}

          <div className={styles.actions}>
            <AlertDialogPrimitive.Cancel asChild>
              <Button type="button" variant="outline" startIcon={actionIcons.cancel} disabled={loading}>
                {cancelLabel}
              </Button>
            </AlertDialogPrimitive.Cancel>

            <Button
              type="button"
              variant={tone === 'destructive' ? 'destructive' : 'primary'}
              disabled={loading}
              aria-busy={loading}
              onClick={handleConfirm}
            >
              {loading ? <Spinner size="sm" label={null} /> : <ConfirmIcon aria-hidden />}
              {confirmLabel}
            </Button>
          </div>
        </AlertDialogPrimitive.Content>
      </AlertDialogPrimitive.Portal>
    </AlertDialogPrimitive.Root>
  );
}
