'use client';

import { FormSubmitButton, SelectField } from '@/components';
import { useFormContext } from '@/context';
import { USER_TYPE_OPTIONS } from '@/shared';
import { Save } from 'lucide-react';
import styles from './admin-user-detail.module.scss';

export function AdminUserFormFields() {
  const { submitting } = useFormContext();

  return (
    <div className={styles.form}>
      <SelectField
        name="userType"
        label="Tipo de Usuário"
        options={[...USER_TYPE_OPTIONS]}
        placeholder="Selecione o tipo de usuário"
      />
      <div className={styles.actions}>
        <FormSubmitButton variant="primary">
          <Save aria-hidden />
          {submitting ? 'Salvando...' : 'Salvar'}
        </FormSubmitButton>
      </div>
    </div>
  );
}
