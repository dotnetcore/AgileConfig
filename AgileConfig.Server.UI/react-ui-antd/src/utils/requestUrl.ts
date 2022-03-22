//const RouterConfig = require('../../config/config').default.routes;

//IIS二级虚拟目录
let iisPrefix = '/config_ui';
//请求接口
let requestPrefix = 'http://127.0.0.1/config_api';

const { NODE_ENV } = process.env;
if (NODE_ENV === 'development') {
  //console.log("NODE_ENV="+NODE_ENV);
  //requestPrefix = 'http://localhost:5000';
  requestPrefix = 'http://localhost:54550';
  iisPrefix = "";
  //requestPrefix = 'http://agileconfig_server.xbaby.xyz';
}
console.log("NODE_ENV=" + NODE_ENV + ", iisPrefix=" + iisPrefix + ", requestPrefix=" + requestPrefix);

/**
 * 请求路径前缀
 * @returns 
 */
export function getRootUrl(): string {
  return requestPrefix;
}

/**
 * IIS前缀
 * @returns 
 */
export function getIISUrl(): string {
  return iisPrefix;
}
     

