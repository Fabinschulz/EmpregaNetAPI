import { formatCount } from '../shared/dashboard-format';

/**
 * Alternativa textual de um gráfico, para leitores de tela.
 */
export type ChartA11yRow = {
  key: string;
  label: string;
  value: number;
};

export function ChartA11yTable({ caption, rows }: { caption: string; rows: ChartA11yRow[] }) {
  if (rows.length === 0) return null;

  return (
    <div className="sr-only">
      <table>
        <caption>{caption}</caption>
        <thead>
          <tr>
            <th scope="col">Categoria</th>
            <th scope="col">Valor</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.key}>
              <th scope="row">{row.label}</th>
              <td>{formatCount(row.value)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
