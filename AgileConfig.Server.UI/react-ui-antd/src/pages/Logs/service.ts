import request from '@/utils/request';
import { LogListParams } from './data';

export async function queryLogs(params?: LogListParams) {
  console.log(params);
  return request('/syslog/search', {
    params,
  });
}
 
