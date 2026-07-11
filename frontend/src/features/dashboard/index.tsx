import { Card, CardContent, CardDescription, CardHeader, CardTitle, PageHeader } from '@/components/ui';

export function DashboardPage() {
  return (
    <div>
      <PageHeader title="Dashboard" />
      <Card style={{ marginTop: 16 }}>
        <CardHeader>
          <CardTitle>Bem-vindo</CardTitle>
          <CardDescription>Relatórios de vagas e candidaturas.</CardDescription>
        </CardHeader>
        <CardContent>
          <p style={{ margin: 0, color: 'var(--muted)' }}>Use o menu lateral para navegar.</p>
        </CardContent>
      </Card>
    </div>
  );
}
