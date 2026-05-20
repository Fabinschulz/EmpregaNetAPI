export { cn } from '../lib';

export function getFieldErrorMessage(path: string, errors: unknown): string | undefined {
  const v = getObjectPropertyValue(path, errors);
  if (v && typeof v === 'object' && v !== null && 'message' in v) {
    const m = (v as { message?: unknown }).message;
    return typeof m === 'string' ? m : undefined;
  }
  return undefined;
}

export function getObjectPropertyValue(path: string, obj: unknown): unknown {
  if (obj == null || typeof obj !== 'object') return undefined;
  const parts = path.split('.').filter(Boolean);
  let current: unknown = obj;
  for (const key of parts) {
    if (current == null || typeof current !== 'object') return undefined;
    current = (current as Record<string, unknown>)[key];
  }
  return current;
}

export const truncateText = (text: string, maxLength = 50) =>  text?.length > maxLength ? `${text.substring(0, maxLength)}...` : text;


export function extractAndConvertFiles(data: any) {
  const allFiles: File[] = [];

  data?.documents!?.forEach((item: any) => {
    const file = item.file as File;
    allFiles.push(file);
  });

  const convertBase64 = (file: File) => {
    return new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  };

  const fileConversions = allFiles.map((file) => convertBase64(file));
  return Promise.all(fileConversions);
}