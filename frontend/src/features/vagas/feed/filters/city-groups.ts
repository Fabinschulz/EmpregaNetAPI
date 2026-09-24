import { ufFullLabel } from '@/shared/schema';

/** Grupo de cidades como vem do vocabulário: código da UF + cidades distintas. */
export type CityGroupByState = { state: string; items: string[] };
export type CityGroupOption = { label: string; items: string[] };

/**
 * Grupos de cidade visíveis na seção "Cidade" do feed.
 *
 * Com Estado selecionado, só os grupos dessas UFs; sem Estado, todos (a busca da própria seção
 * cobre o resto). A correspondência é pelo **código** da UF, nunca pelo rótulo, os rótulos do
 * backend e do frontend divergem (ex.: `"Ceara"` / `"Ceará"`). O rótulo exibido vem de `ufFullLabel`.
 *
 * @param cities Grupos do vocabulário (`vocabulary.cities`).
 * @param selectedStates Códigos de UF marcados no filtro de Estado.
 */
export function cityGroupsForStates(
  cities: readonly CityGroupByState[],
  selectedStates: readonly string[]
): CityGroupOption[] {
  const visible = selectedStates.length > 0 ? cities.filter((group) => selectedStates.includes(group.state)) : cities;
  return visible.map((group) => ({ label: ufFullLabel(group.state), items: group.items }));
}
