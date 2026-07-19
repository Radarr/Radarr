import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import createPersonalizedUiSelector from 'Store/Selectors/createPersonalizedUiSelector';
import themes from 'Styles/Themes';

const useTheme = () => {
  const { theme: selectedTheme } = useSelector(createPersonalizedUiSelector());
  const [resolvedTheme, setResolvedTheme] = useState(selectedTheme);

  useEffect(() => {
    if (selectedTheme !== 'system') {
      setResolvedTheme(selectedTheme);
      return;
    }

    const applySystemTheme = () => {
      setResolvedTheme(
        window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light'
      );
    };

    applySystemTheme();

    window
      .matchMedia('(prefers-color-scheme: dark)')
      .addEventListener('change', applySystemTheme);

    return () => {
      window
        .matchMedia('(prefers-color-scheme: dark)')
        .removeEventListener('change', applySystemTheme);
    };
  }, [selectedTheme]);

  return resolvedTheme;
};

export default useTheme;

export const useThemeColor = (color: string) => {
  const theme = useTheme();
  const themeVariables = themes[theme];

  // @ts-expect-error - themeVariables is a string indexable type
  return themeVariables[color];
};
