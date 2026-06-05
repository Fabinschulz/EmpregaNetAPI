'use client';

import { useTheme } from '@/context';
import { useHasMounted } from '@/hooks';
import { getGoogleClientId } from '@/utils/lib/env';
import Script from 'next/script';
import { useEffect, useRef, useState } from 'react';
import styles from './google-sign-in-button.module.scss';

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
              logo_alignment?: string;
              locale?: string;
            }
          ) => void;
        };
      };
    };
  }
}

function GoogleMarkIcon() {
  return (
    <svg className={styles.icon} viewBox="0 0 48 48" aria-hidden focusable="false">
      <path
        fill="#FFC107"
        d="M43.611 20.083H42V20H24v8h11.303c-1.649 4.657-6.08 8-11.303 8-6.627 0-12-5.373-12-12s5.373-12 12-12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 12.955 4 4 12.955 4 24s8.955 20 20 20 20-8.955 20-20c0-1.341-.138-2.65-.389-3.917z"
      />
      <path
        fill="#FF3D00"
        d="M6.306 14.691l6.571 4.819C14.655 15.108 18.961 12 24 12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 16.318 4 9.656 8.337 6.306 14.691z"
      />
      <path
        fill="#4CAF50"
        d="M24 44c5.166 0 9.86-1.977 13.409-5.192l-6.19-5.238C29.211 35.091 26.715 36 24 36c-5.202 0-9.619-3.317-11.283-7.946l-6.522 5.025C9.505 39.556 16.227 44 24 44z"
      />
      <path
        fill="#1976D2"
        d="M43.611 20.083H42V20H24v8h11.303c-.792 2.237-2.231 4.166-4.087 5.571l6.19 5.238C42.022 35.026 44 30.138 44 24c0-1.341-.138-2.65-.389-3.917z"
      />
    </svg>
  );
}

export function GoogleSignInButton({ onCredential, disabled }: GoogleSignInButtonProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const overlayRef = useRef<HTMLDivElement>(null);
  const clientId = getGoogleClientId();
  const { resolvedTheme } = useTheme();
  const themeMounted = useHasMounted();
  const [scriptReady, setScriptReady] = useState(false);
  const [buttonWidth, setButtonWidth] = useState(360);

  const googleButtonTheme = themeMounted && resolvedTheme === 'light' ? 'outline' : 'filled_black';

  const onCredentialRef = useRef(onCredential);
  onCredentialRef.current = onCredential;

  useEffect(() => {
    const root = rootRef.current;
    if (!root) return;

    const updateWidth = () => {
      const width = Math.floor(root.getBoundingClientRect().width);
      if (width > 0) setButtonWidth(width);
    };

    updateWidth();
    const observer = new ResizeObserver(updateWidth);
    observer.observe(root);
    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    if (!scriptReady || !clientId || !overlayRef.current || disabled) return;

    const google = window.google;
    if (!google?.accounts?.id) return;

    overlayRef.current.innerHTML = '';

    google.accounts.id.initialize({
      client_id: clientId,
      callback: (response) => {
        if (response.credential) onCredentialRef.current(response.credential);
      }
    });

    google.accounts.id.renderButton(overlayRef.current, {
      type: 'standard',
      theme: googleButtonTheme,
      size: 'large',
      text: 'signin_with',
      shape: 'pill',
      logo_alignment: 'left',
      width: buttonWidth,
      locale: 'pt-BR'
    });
  }, [clientId, disabled, googleButtonTheme, scriptReady, buttonWidth]);

  if (!clientId) {
    return (
      <p className={styles.unavailable}>
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
        ref={rootRef}
        className={`${styles.root} ${disabled ? styles.disabled : ''}`}
        aria-label="Continuar com o Google"
        aria-busy={disabled}
      >
        <div className={styles.face} aria-hidden>
          <GoogleMarkIcon />
          <span className={styles.label}>Continuar com o Google</span>
        </div>
        <div className={styles.overlay} ref={overlayRef} aria-hidden={disabled} />
      </div>
    </>
  );
}
