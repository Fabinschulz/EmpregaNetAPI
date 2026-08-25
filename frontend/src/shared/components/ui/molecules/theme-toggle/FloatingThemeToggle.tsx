'use client';

import { useHasMounted } from '@/shared/hooks';
import { useTheme } from 'next-themes';
import { actionIcons } from '../../icons';
import { IconButton } from '../icon-button';
import styles from './floating-theme-toggle.module.scss';

/** Botão flutuante para alternar entre tema claro e escuro. */
export function FloatingThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme();
  const themeMounted = useHasMounted();
  const isDark = themeMounted && resolvedTheme === 'dark';

  const toggleLightDark = () => {
    if (!themeMounted) return;
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark');
  };

  return (
    <div className={styles.wrap}>
      <IconButton
        variant="outline"
        icon={isDark ? actionIcons.themeLight : actionIcons.themeDark}
        label={!themeMounted ? 'Alternar tema' : isDark ? 'Alternar para tema claro' : 'Alternar para tema escuro'}
        iconStyleOverrides={styles.icon}
        onClick={toggleLightDark}
      />
    </div>
  );
}
