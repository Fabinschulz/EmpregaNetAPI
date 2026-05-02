"use client";

import { FormSubmitButton } from "@/components";
import { useFormContext } from "@/context";

export function RegisterSubmitButton() {
  const { submitting } = useFormContext();
  return (
    <FormSubmitButton variant="primary">
      {submitting ? "Criando..." : "Criar conta"}
    </FormSubmitButton>
  );
}
