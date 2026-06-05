import Link from 'next/link';
import styles from './auth-page.module.scss';

type AuthNavLinkProps = {
  href: string;
  children: React.ReactNode;
  muted?: boolean;
};

export function AuthNavLink({ href, children, muted }: AuthNavLinkProps) {
  return (
    <Link href={href} className={muted ? styles.linkMuted : styles.link}>
      {children}
    </Link>
  );
}

type AuthLinkRowProps = {
  children: React.ReactNode;
};

export function AuthLinkRow({ children }: AuthLinkRowProps) {
  return <div className={styles.linkRow}>{children}</div>;
}
