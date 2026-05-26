import { z } from 'zod';

const envSchema = z.object({
  NEXT_PUBLIC_API_BASE_URL: z.string().url(),
  NEXT_PUBLIC_GOOGLE_CLIENT_ID: z.string().min(1).optional()
});

export type PublicEnv = z.infer<typeof envSchema>;

export function getPublicEnv(): PublicEnv {
  const parsed = envSchema.safeParse({
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
    NEXT_PUBLIC_GOOGLE_CLIENT_ID: process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID
  });

  if (!parsed.success) {
    throw new Error(`Env inválida: ${parsed.error.issues.map((i) => i.path.join('.') + ' ' + i.message).join('; ')}`);
  }

  return parsed.data;
}

export function getGoogleClientId(): string | undefined {
  const id = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID?.trim();
  return id && id.length > 0 ? id : undefined;
}
