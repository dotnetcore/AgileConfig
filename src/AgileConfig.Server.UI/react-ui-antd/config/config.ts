// https://umijs.org/config/
import { defineConfig } from '@umijs/max';
import proxy from './proxy';
import routes from './routes';

const { REACT_APP_ENV } = process.env;
const proxyEnv = (REACT_APP_ENV || 'dev') as keyof typeof proxy;

export default defineConfig({
  hash: true,
  esbuildMinifyIIFE: true,
  define: {
    REACT_APP_ENV: REACT_APP_ENV || false,
    ANT_DESIGN_PRO_ONLY_DO_NOT_USE_IN_YOUR_PRODUCTION: false,
  },
  antd: {},
  dva: {},
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
  // umi routes: https://umijs.org/docs/routing
  routes,
  title: false,
  ignoreMomentLocale: true,
  proxy: proxy[proxyEnv],
  manifest: {
    basePath: '/a/',
  },
  publicPath: '/',
  runtimePublicPath: {},
});
