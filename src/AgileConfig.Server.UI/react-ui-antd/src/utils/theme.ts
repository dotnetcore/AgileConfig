const themeStorageKey = 'agileconfig.theme';
export const themeChangeEvent = 'agileconfig.theme.change';

export const getStoredDarkMode = (): boolean => {
  if (typeof window === 'undefined') {
    return false;
  }

  try {
    return window.localStorage.getItem(themeStorageKey) === 'dark';
  } catch {
    return false;
  }
};

export const applyDarkMode = (darkMode: boolean): void => {
  if (typeof document !== 'undefined') {
    document.documentElement.classList.toggle('dark-mode', darkMode);
  }

  if (typeof window !== 'undefined') {
    try {
      window.localStorage.setItem(themeStorageKey, darkMode ? 'dark' : 'light');
    } catch {
      // Storage can be unavailable in private or restricted browsing contexts.
    }

    window.dispatchEvent(new CustomEvent(themeChangeEvent, { detail: { darkMode } }));
  }
};
