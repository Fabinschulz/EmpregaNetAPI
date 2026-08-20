export function displayNameOrId(name: string | null | undefined, id: number): string {
  return name?.trim() || `#${id}`;
}
