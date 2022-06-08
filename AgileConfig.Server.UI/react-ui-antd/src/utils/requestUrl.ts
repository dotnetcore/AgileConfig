//const RouterConfig = require('../../config/config').default.routes;

//IIS二级虚拟目录
let iisPrefix = '/configui';
//请求接口
let requestPrefix = 'http://10.120.147.218/configapi';

const { NODE_ENV } = process.env;
if (NODE_ENV === 'development') {
  //console.log("NODE_ENV="+NODE_ENV);
  requestPrefix = 'http://localhost:5000';
  iisPrefix = "";
  //requestPrefix = 'http://agileconfig_server.xbaby.xyz';
}
console.log("NODE_ENV=" + NODE_ENV + ", iisPrefix=" + iisPrefix + ", requestPrefix=" + requestPrefix);

/**
 * 请求API路径前缀
 * @returns 
 */
export function getRootUrl(): string {
  return requestPrefix;
}

/**
 * IIS二级虚拟目录
 * @returns 
 */
export function getIISUrl(): string {
  return iisPrefix;
}
     

