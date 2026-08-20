import type { JobFeedItemResponse } from '@/features/vagas/service';
import { InfoItem, InfoList } from '@/shared/components';
import { Banknote, Briefcase, Building2, House, MapPin, type LucideIcon } from 'lucide-react';
import { toJobFacts, type JobFactIcon } from './job-facts';

const ICONS: Record<JobFactIcon, LucideIcon> = {
  location: MapPin,
  salary: Banknote,
  remote: House,
  hybrid: Building2,
  onSite: MapPin,
  contract: Briefcase
};

export function JobCardFacts({ job }: { job: JobFeedItemResponse }) {
  return (
    <InfoList ariaLabel="Dados da vaga">
      {toJobFacts(job).map((fact) => (
        <InfoItem
          key={fact.key}
          icon={ICONS[fact.icon]}
          srLabel={fact.srLabel}
          title={fact.label}
          strong={fact.strong}
        >
          {fact.label}
        </InfoItem>
      ))}
    </InfoList>
  );
}
