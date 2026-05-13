import { AuthLayout } from '@/features/auth';

export default function AuthSegmentLayout({ children }: { children: React.ReactNode }) {
  return <AuthLayout>{children}</AuthLayout>;
}
