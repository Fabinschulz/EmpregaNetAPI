'use client';

import { FormSubmitButton, InputField } from '@/components';
import { KeyRound } from 'lucide-react';
import styles from '../conta.module.scss';

export function ChangePasswordFormFields() {
  return (
    <>
      <div className={styles.formGrid}>
        <InputField name="currentPassword" label="Senha atual" type="password" required className={styles.fieldFull} />
        <InputField name="newPassword" label="Nova senha" type="password" required />
        <InputField name="newPasswordConfirmation" label="Confirmar nova senha" type="password" required />
      </div>
      <div className={styles.formFooter}>
        <FormSubmitButton variant="primary">
          <KeyRound aria-hidden />
          Alterar senha
        </FormSubmitButton>
      </div>
    </>
  );
}
