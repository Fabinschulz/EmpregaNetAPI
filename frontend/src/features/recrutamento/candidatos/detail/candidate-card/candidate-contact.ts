import { maskBrazilPhone } from '@/shared/utils';
import type { CandidateDetailResponse } from '../../service';

export type CandidateContact = {
  email: string | null;
  phone: string | null;
  mailtoHref: string | null;
  telHref: string | null;
  isEmpty: boolean;
};

export function toCandidateContact(candidate: CandidateDetailResponse): CandidateContact {
  const email = candidate.email || null;
  const rawPhone = candidate.phoneNumber;

  return {
    email,
    phone: rawPhone ? maskBrazilPhone(rawPhone) : null,
    mailtoHref: email ? `mailto:${email}` : null,
    telHref: rawPhone ? `tel:${rawPhone}` : null,
    isEmpty: !email && !rawPhone
  };
}
