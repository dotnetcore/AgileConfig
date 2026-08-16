import { Settings as ProSettings } from '@ant-design/pro-layout';

type DefaultSettings = Partial<ProSettings> & {
  pwa: boolean;
  darkMode: boolean;
};

const proSettings: DefaultSettings ={
  "navTheme": "light",
  "primaryColor": "#1890ff",
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
  },
  "headerHeight": 48
}

export type { DefaultSettings };

export default proSettings;
