import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import createPersonalizedUiSelector from 'Store/Selectors/createPersonalizedUiSelector';
import themes from 'Styles/Themes';

function ApplyTheme() {
  const preferences = useSelector(createPersonalizedUiSelector());
  const [systemTheme, setSystemTheme] = useState(
    window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  );
  const theme = preferences.theme === 'system' ? systemTheme : preferences.theme;

  useEffect(() => {
    const media = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = () => setSystemTheme(media.matches ? 'dark' : 'light');
    media.addEventListener('change', handleChange);
    return () => media.removeEventListener('change', handleChange);
  }, []);

  useEffect(() => {
    Object.entries(themes[theme] ?? themes.dark).forEach(([key, value]) => {
      document.documentElement.style.setProperty(`--${key}`, value);
    });
    document.documentElement.style.setProperty('--accentColor', preferences.accentColor);
    document.documentElement.dataset.theme = theme;
    document.documentElement.dataset.density = preferences.density;
    document.documentElement.dataset.cardStyle = preferences.cardStyle;
    document.documentElement.dataset.posterSize = preferences.posterSize;
    document.documentElement.dataset.animations = preferences.enableAnimations ? 'on' : 'off';
    document.documentElement.dataset.backdrops = preferences.enableBackdrops ? 'on' : 'off';
  }, [preferences, theme]);

  return null;
}

export default ApplyTheme;
