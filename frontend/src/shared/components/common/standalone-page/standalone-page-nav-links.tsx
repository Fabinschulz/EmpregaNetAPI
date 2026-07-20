import Link from 'next/link';
import styles from './standalone-page.module.scss';

type StandalonePageNavLinkProps = {
  href: string;
  children: React.ReactNode;
  muted?: boolean;
};

export function StandalonePageNavLink({ href, children, muted }: StandalonePageNavLinkProps) {
  return (
    <Link href={href} className={muted ? styles.linkMuted : styles.link}>
      {children}
    </Link>
  );
}

type StandalonePageLinkRowProps = {
  children: React.ReactNode;
};

export function StandalonePageLinkRow({ children }: StandalonePageLinkRowProps) {
  return <div className={styles.linkRow}>{children}</div>;
}
