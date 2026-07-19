'use client';

import { Button } from '@/components';
import { useHasMounted } from '@/hooks';
import { Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import styles from './floating-theme-toggle.module.scss';

/** Botão flutuante para alternar entre tema claro e escuro. */
export function FloatingThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme();
  const themeMounted = useHasMounted();
  const toggleLightDark = () => {
    if (!themeMounted) return;
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark');
  };
  return (
    <div className={styles.wrap}>
      <Button
        type="button"
        variant="outline"
        size="icon"
        aria-label={
          !themeMounted
            ? 'Alternar tema'
            : resolvedTheme === 'dark'
              ? 'Alternar para tema claro'
              : 'Alternar para tema escuro'
        }
        onClick={toggleLightDark}
      >
        {themeMounted && resolvedTheme === 'dark' ? <Sun className={styles.icon} /> : <Moon className={styles.icon} />}
      </Button>
    </div>
  );
}
