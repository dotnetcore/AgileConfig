import React, { PropsWithChildren, useEffect, useMemo, useState } from 'react';
import { App as AntdApp, ConfigProvider, theme } from 'antd';
import defaultSettings from '../config/defaultSettings';
import { getStoredDarkMode, themeChangeEvent } from '@/utils/theme';

const buildTheme = (darkMode: boolean) => ({
  algorithm: darkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
  cssVar: { key: 'agileconfig' },
  token: {
    borderRadius: 4,
    colorPrimary: defaultSettings.colorPrimary,
  },
});

const AppThemeProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const [darkMode, setDarkMode] = useState(getStoredDarkMode);
  const themeConfig = useMemo(() => buildTheme(darkMode), [darkMode]);

  useEffect(() => {
    const handleThemeChange = (event: Event) => {
      setDarkMode((event as CustomEvent<{ darkMode: boolean }>).detail.darkMode);
    };

    window.addEventListener(themeChangeEvent, handleThemeChange);
    return () => window.removeEventListener(themeChangeEvent, handleThemeChange);
  }, []);

  useEffect(() => {
    ConfigProvider.config({
      holderRender: (children) => (
        <ConfigProvider theme={themeConfig}>{children}</ConfigProvider>
      ),
    });
  }, [themeConfig]);

  return (
    <ConfigProvider theme={themeConfig}>
      <AntdApp>{children}</AntdApp>
    </ConfigProvider>
  );
};

export function rootContainer(container: React.ReactNode): React.ReactNode {
  return <AppThemeProvider>{container}</AppThemeProvider>;
}
