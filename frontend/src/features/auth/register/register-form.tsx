"use client";

import { useState } from "react";
import { z } from "zod";
import { Alert, InputField } from "@/components";
import { FormProvider } from "@/context";
import { register } from "@/services";
import { REGISTER_FIELDS_GRID_STYLE } from "./constants";
import { registerDefaultValues, registerFormSchema } from "./register-schema";
import type { RegisterDto } from "./register-schema";
import { RegisterSubmitButton } from "./register-submit-button";

export function RegisterForm() {
  const [apiError, setApiError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  async function handleSubmit(data: RegisterDto) {
    setApiError(null);
    setSuccess(null);
    try {
      const res = await register(data);
      setSuccess(typeof res === "string" ? res : "Conta criada. Confirme o e-mail para entrar.");
    } catch (err) {
      if (err instanceof z.ZodError) setApiError("Resposta do servidor inesperada.");
      else if (err instanceof Error) setApiError(err.message);
      else setApiError("Erro inesperado.");
    }
  }

  return (
    <>
      {apiError ? <Alert variant="destructive" title="Falha ao criar conta">{apiError}</Alert> : null}
      {success ? <Alert variant="success" title="Sucesso">{success}</Alert> : null}

      <FormProvider validationSchema={registerFormSchema} defaultValues={registerDefaultValues} onSubmit={handleSubmit}>
        <div style={REGISTER_FIELDS_GRID_STYLE}>
          <InputField name="username" label="Nome de usuário" autoComplete="username" required />
          <InputField name="email" label="E-mail" type="email" autoComplete="email" required />
          <InputField name="password" label="Senha" type="password" autoComplete="new-password" required />
          <InputField
            name="passwordConfirmation"
            label="Confirmar senha"
            type="password"
            autoComplete="new-password"
            required
          />
          <RegisterSubmitButton />
        </div>
      </FormProvider>
    </>
  );
}
