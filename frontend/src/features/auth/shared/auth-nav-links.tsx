import Link from 'next/link';
import styles from './auth-page.module.scss';

type AuthNavLinkProps = {
  href: string;
  children: React.ReactNode;
};

export function AuthNavLink({ href, children }: AuthNavLinkProps) {
  return (
    <Link href={href} className={styles.link}>
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
