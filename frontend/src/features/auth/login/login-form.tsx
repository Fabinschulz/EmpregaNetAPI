"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { z } from "zod";
import { Alert, InputField } from "@/components";
import { FormProvider } from "@/context";
import { login } from "@/services";
import { useAuth } from "@/features/auth";
import { LOGIN_FIELDS_GRID_STYLE } from "./constants";
import { loginDefaultValues, loginSchema } from "./login-schema";
import type { LoginDto } from "./login-schema";
import { LoginSubmitButton } from "./login-submit-button";

export function LoginForm() {
  const router = useRouter();
  const { setLoggedUser } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);

  async function handleSubmit(data: LoginDto) {
    setApiError(null);
    try {
      const res = await login(data);
      setLoggedUser(res);
      router.push("/dashboard");
    } catch (err) {
      if (err instanceof z.ZodError) setApiError("Resposta do servidor inesperada.");
      else if (err instanceof Error) setApiError(err.message);
      else setApiError("Erro inesperado.");
    }
  }

  return (
    <>
      {apiError ? <Alert variant="destructive" title="Falha ao entrar">{apiError}</Alert> : null}

      <FormProvider validationSchema={loginSchema} defaultValues={loginDefaultValues} onSubmit={handleSubmit}>
        <div style={LOGIN_FIELDS_GRID_STYLE}>
          <InputField name="email" label="E-mail" type="email" autoComplete="email" required />
          <InputField name="password" label="Senha" type="password" autoComplete="current-password" required />
          <LoginSubmitButton />
        </div>
      </FormProvider>
    </>
  );
}
