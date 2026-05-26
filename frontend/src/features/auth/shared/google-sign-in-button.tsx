'use client';

import { getGoogleClientId } from '@/utils/lib/env';
import Script from 'next/script';
import { useEffect, useRef, useState } from 'react';
import styles from './auth-page.module.scss';

type GoogleCredentialResponse = {
  credential?: string;
};

type GoogleSignInButtonProps = {
  onCredential: (idToken: string) => void;
  disabled?: boolean;
};

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
            auto_select?: boolean;
          }) => void;
          renderButton: (
            parent: HTMLElement,
            options: {
              type?: string;
              theme?: string;
              size?: string;
              text?: string;
              shape?: string;
              width?: number;
            }
          ) => void;
        };
      };
    };
  }
}

export function GoogleSignInButton({ onCredential, disabled }: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const clientId = getGoogleClientId();
  const [scriptReady, setScriptReady] = useState(false);

  const onCredentialRef = useRef(onCredential);
  onCredentialRef.current = onCredential;

  useEffect(() => {
    if (!scriptReady || !clientId || !containerRef.current || disabled) return;

    const google = window.google;
    if (!google?.accounts?.id) return;

    containerRef.current.innerHTML = '';

    google.accounts.id.initialize({
      client_id: clientId,
      callback: (response) => {
        if (response.credential) onCredentialRef.current(response.credential);
      }
    });

    google.accounts.id.renderButton(containerRef.current, {
      type: 'standard',
      theme: 'outline',
      size: 'large',
      text: 'continue_with',
      shape: 'rectangular',
      width: 360
    });
  }, [clientId, disabled, scriptReady]);

  if (!clientId) {
    return (
      <p className={styles.description} style={{ textAlign: 'center', margin: 0 }}>
        Login com Google indisponível (configure <code>NEXT_PUBLIC_GOOGLE_CLIENT_ID</code>).
      </p>
    );
  }

  return (
    <>
      <Script
        src="https://accounts.google.com/gsi/client"
        strategy="afterInteractive"
        onReady={() => setScriptReady(true)}
      />
      <div
        className={styles.googleWrap}
        ref={containerRef}
        aria-busy={disabled}
      />
    </>
  );
}
