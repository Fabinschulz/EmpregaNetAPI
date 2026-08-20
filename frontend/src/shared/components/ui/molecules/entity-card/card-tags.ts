export type CardTag = {
  key: string;
  label: string;
  tone?: 'default' | 'accent';
};

export function toCardTags(labels: readonly string[], tone?: CardTag['tone']): CardTag[] {
  const seen = new Set<string>();
  const tags: CardTag[] = [];

  for (const label of labels) {
    const text = label?.trim();
    if (!text) continue;

    const key = text.toLowerCase();
    if (seen.has(key)) continue;

    seen.add(key);
    tags.push({ key: text, label: text, tone });
  }

  return tags;
}
