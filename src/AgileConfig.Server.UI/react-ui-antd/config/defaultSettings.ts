import type { Settings as ProSettings } from '@ant-design/pro-components';

type DefaultSettings = Partial<ProSettings> & {
  pwa: boolean;
  darkMode: boolean;
};

const proSettings: DefaultSettings ={
  "navTheme": "light",
  "colorPrimary": "#1677ff",
  "layout": "mix",
  "contentWidth": "Fluid",
  "fixedHeader": false,
  "fixSiderbar": true,
  "title": "AgileConfig",
  "pwa": false,
  "darkMode": false,
  "iconfontUrl": "",
  "menu": {
    "locale": true
  }
}

export type { DefaultSettings };

export default proSettings;
