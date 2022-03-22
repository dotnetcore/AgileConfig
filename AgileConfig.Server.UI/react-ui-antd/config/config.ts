// https://umijs.org/config/
import { defineConfig } from 'umi';
import defaultSettings from './defaultSettings';
import proxy from './proxy';
import routes from './routes';
//import { getIISUrl } from '@/utils/requestUrl';

const { REACT_APP_ENV } = process.env;
let iisPrefix = '/config_ui';
//if (REACT_APP_ENV === 'development'){
//  iisPrefix = '';
//}
console.log("REACT_APP_ENV=" + REACT_APP_ENV + ", iisPrefix=" + iisPrefix);

export default defineConfig({
  hash: true,
  antd: {},
  dva: {
    hmr: true,
  },
  history: {
    type: 'hash',
  },
  locale: {
    // default zh-CN
    default: 'zh-CN',
    antd: true,
    // default true, when it is true, will use `navigator.language` overwrite default
    baseNavigator: true,
  },
  dynamicImport: {
    loading: '@/components/PageLoading/index',
  },
  targets: {
    ie: 11,
  },
  // umi routes: https://umijs.org/docs/routing
  routes,
  // Theme for antd: https://ant.design/docs/react/customize-theme-cn
  theme: {
    'primary-color': defaultSettings.primaryColor,
  },
  title: false,
  ignoreMomentLocale: true,
  proxy: proxy[REACT_APP_ENV || 'dev'],
  manifest: {
    basePath: '/a/',
  },
  esbuild: {},
  //getIISUrl打包引用不到模块
  base: iisPrefix + '/',
  publicPath: iisPrefix + '/'
});
