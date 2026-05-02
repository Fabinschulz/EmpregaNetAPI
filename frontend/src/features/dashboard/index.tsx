import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui";

export function DashboardPage() {
  return (
    <div>
      <h1>Dashboard</h1>
      <Card style={{ marginTop: 16 }}>
        <CardHeader>
          <CardTitle>Bem-vindo</CardTitle>
          <CardDescription>Relatórios de vagas e candidaturas.</CardDescription>
        </CardHeader>
        <CardContent>
          <p style={{ margin: 0, color: "var(--muted)" }}>Use o menu lateral para navegar.</p>
        </CardContent>
      </Card>
    </div>
  );
}

