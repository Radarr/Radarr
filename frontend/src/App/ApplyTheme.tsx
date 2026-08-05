import { useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import themes from 'Styles/Themes';
import AppState from './State/AppState';

const THEME_STORAGE_KEY = 'radarr-ui-theme';
const SYSTEM_THEME_QUERY = '(prefers-color-scheme: dark)';
const themeNames = ['auto', 'light', 'dark', 'oled'] as const;

type ThemeName = (typeof themeNames)[number];
type ResolvedThemeName = Exclude<ThemeName, 'auto'>;

const themeSelector = createSelector(
  (state: AppState) => state.settings.ui.item.theme || window.Radarr.theme,
  (theme) => theme
);

function isThemeName(theme: unknown): theme is ThemeName {
  return themeNames.includes(theme as ThemeName);
}

function resolveTheme(theme: ThemeName): ResolvedThemeName {
  if (theme !== 'auto') {
    return theme;
  }

  return window.matchMedia?.(SYSTEM_THEME_QUERY).matches ? 'dark' : 'light';
}

function ApplyTheme() {
  const selectedTheme = useSelector(themeSelector);
  const theme: ThemeName = isThemeName(selectedTheme) ? selectedTheme : 'auto';

  const updateCSSVariables = useCallback(() => {
    const resolvedTheme = resolveTheme(theme);
    const themeVariables = themes[resolvedTheme] as Record<string, string>;
    const documentElement = document.documentElement;

    Object.entries(themeVariables).forEach(([key, value]) => {
      documentElement.style.setProperty(`--${key}`, value);
    });

    documentElement.dataset.theme = theme;
    documentElement.dataset.resolvedTheme = resolvedTheme.toLowerCase();
    documentElement.style.colorScheme =
      resolvedTheme === 'light' ? 'light' : 'dark';
    documentElement.style.backgroundColor = themeVariables.pageBackground;

    document
      .querySelector<HTMLMetaElement>('meta[name="theme-color"]')
      ?.setAttribute('content', themeVariables.pageBackground);
    document
      .querySelector<HTMLMetaElement>(
        'meta[name="msapplication-navbutton-color"]'
      )
      ?.setAttribute('content', themeVariables.pageHeaderBackgroundColor);

    window.Radarr.theme = theme;

    try {
      window.localStorage.setItem(THEME_STORAGE_KEY, theme);
    } catch {
      // Theme application must continue when storage is blocked or unavailable.
    }
  }, [theme]);

  useEffect(() => {
    updateCSSVariables();

    if (theme !== 'auto') {
      return undefined;
    }

    const systemTheme = window.matchMedia?.(SYSTEM_THEME_QUERY);

    if (!systemTheme) {
      return undefined;
    }

    if (systemTheme.addEventListener) {
      systemTheme.addEventListener('change', updateCSSVariables);

      return () => {
        systemTheme.removeEventListener('change', updateCSSVariables);
      };
    }

    systemTheme.addListener(updateCSSVariables);

    return () => {
      systemTheme.removeListener(updateCSSVariables);
    };
  }, [theme, updateCSSVariables]);

  return null;
}

export default ApplyTheme;
