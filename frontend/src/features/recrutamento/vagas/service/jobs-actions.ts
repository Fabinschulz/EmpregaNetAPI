'use server';

import { updateTag } from 'next/cache';

export async function revalidateJobCache(id: number): Promise<void> {
  if (!Number.isFinite(id) || id <= 0) return;

  try {
    updateTag(`job:${id}`);
  } catch (err) {
    console.error(`Falha ao revalidar o cache da vaga ${id}`, err);
  }
}
