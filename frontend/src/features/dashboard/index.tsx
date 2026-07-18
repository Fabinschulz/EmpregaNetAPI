'use client';

import { Button, Card, CardContent, CardHeader, CardTitle, PageHeader, Skeleton } from '@/components';
import { useAuth } from '@/features/auth';
import {
    useAdminUsersListQuery,
    useAllJobApplicationsQuery,
    useCompaniesListQuery,
    useJobsListQuery,
    useMyJobApplicationsQuery
} from '@/services';
import { isAdmin, isRecruitmentStaff } from '@/shared';
import { Briefcase, Building2, FileText, LayoutDashboard, Users, type LucideIcon } from 'lucide-react';
import Link from 'next/link';
import styles from './dashboard.module.scss';

const numberFormatter = new Intl.NumberFormat('pt-BR');

type MetricLink = { href: string; label: string };

type MetricCardProps = {
  icon: LucideIcon;
  title: string;
  value: number | undefined;
  isPending: boolean;
  isError?: boolean;
  hint?: string;
  links?: MetricLink[];
};

function MetricCard({ icon: Icon, title, value, isPending, isError = false, hint, links }: MetricCardProps) {
  return (
    <Card className={styles.card}>
      <CardHeader className={styles.cardHead}>
        <span className={styles.iconWrap}>
          <Icon aria-hidden />
        </span>
        <CardTitle className={styles.cardTitle}>{title}</CardTitle>
      </CardHeader>
      <CardContent className={styles.cardBody}>
        {isPending ? (
          <Skeleton className={styles.metricSkeleton} aria-hidden />
        ) : (
          <p className={styles.metric}>{isError || value === undefined ? '—' : numberFormatter.format(value)}</p>
        )}
        {hint ? <p className={styles.metricHint}>{hint}</p> : null}
        {links && links.length > 0 ? (
          <div className={styles.actions}>
            {links.map((link) => (
              <Button key={link.href} asChild variant="outline" size="sm">
                <Link href={link.href}>{link.label}</Link>
              </Button>
            ))}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}

/** Visão padrão do candidato: acompanhamento das próprias candidaturas. */
function CandidateSection() {
  const { data, isPending, isError } = useMyJobApplicationsQuery();

  return (
    <section className={styles.section} aria-label="Minha atividade">
      <h2 className={styles.sectionTitle}>Minha atividade</h2>
      <div className={styles.grid}>
        <MetricCard
          icon={FileText}
          title="Minhas candidaturas"
          value={data?.totalItems}
          isPending={isPending}
          isError={isError}
          hint="Candidaturas que você enviou."
          links={[
            { href: '/candidaturas', label: 'Ver candidaturas' },
            { href: '/vagas', label: 'Explorar vagas' }
          ]}
        />
      </div>
    </section>
  );
}

/** Visão da equipe de recrutamento: vagas ativas e volume de candidaturas. */
function RecruitmentSection() {
  const activeJobs = useJobsListQuery({ isActive: true });
  const applications = useAllJobApplicationsQuery();

  return (
    <section className={styles.section} aria-label="Recrutamento">
      <h2 className={styles.sectionTitle}>Recrutamento</h2>
      <div className={styles.grid}>
        <MetricCard
          icon={Briefcase}
          title="Vagas ativas"
          value={activeJobs.data?.totalItems}
          isPending={activeJobs.isPending}
          isError={activeJobs.isError}
          hint="Vagas atualmente abertas."
          links={[{ href: '/recrutamento/vagas', label: 'Gerir vagas' }]}
        />
        <MetricCard
          icon={FileText}
          title="Candidaturas"
          value={applications.data?.totalItems}
          isPending={applications.isPending}
          isError={applications.isError}
          hint="Total de candidaturas recebidas."
          links={[{ href: '/recrutamento/candidaturas', label: 'Ver candidaturas' }]}
        />
      </div>
    </section>
  );
}

/** Seção de gestão do Admin: distribuição de usuários e empresas. */
function AdminManagementSection() {
  const users = useAdminUsersListQuery();
  const companies = useCompaniesListQuery();

  return (
    <section className={styles.section} aria-label="Gestão">
      <h2 className={styles.sectionTitle}>Gestão</h2>
      <div className={styles.grid}>
        <MetricCard
          icon={Users}
          title="Usuários"
          value={users.data?.totalItems}
          isPending={users.isPending}
          isError={users.isError}
          hint="Usuários cadastrados na plataforma."
          links={[{ href: '/admin/usuarios', label: 'Gerir usuários' }]}
        />
        <MetricCard
          icon={Building2}
          title="Empresas"
          value={companies.data?.totalItems}
          isPending={companies.isPending}
          isError={companies.isError}
          hint="Empresas cadastradas."
          links={[{ href: '/admin/empresas', label: 'Gerir empresas' }]}
        />
      </div>
    </section>
  );
}

/** Destaque de gestão para o Manager (não-admin): visão consolidada do pipeline. */
function ManagerManagementSection() {
  return (
    <section className={styles.section} aria-label="Visão de gestão">
      <h2 className={styles.sectionTitle}>Gestão</h2>
      <div className={styles.grid}>
        <Card className={styles.card}>
          <CardHeader className={styles.cardHead}>
            <span className={styles.iconWrap}>
              <LayoutDashboard aria-hidden />
            </span>
            <CardTitle className={styles.cardTitle}>Visão de gestão</CardTitle>
          </CardHeader>
          <CardContent className={styles.cardBody}>
            <p className={styles.metricHint}>
              Acompanhe o processo seletivo de ponta a ponta: gerencie as vagas abertas e avance as candidaturas pelo
              funil.
            </p>
            <div className={styles.actions}>
              <Button asChild variant="outline" size="sm">
                <Link href="/recrutamento/vagas">Vagas</Link>
              </Button>
              <Button asChild variant="outline" size="sm">
                <Link href="/recrutamento/candidaturas">Candidaturas</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}

export function DashboardPage() {
  const { roles, username } = useAuth();
  const recruitmentStaff = isRecruitmentStaff(roles);
  const admin = isAdmin(roles);

  const welcome = username ? `Bem-vindo(a), ${username}.` : 'Bem-vindo(a).';

  return (
    <div>
      <PageHeader title="Dashboard" description={welcome} />

      {recruitmentStaff ? <RecruitmentSection /> : <CandidateSection />}
      {recruitmentStaff ? admin ? <AdminManagementSection /> : <ManagerManagementSection /> : null}
    </div>
  );
}
