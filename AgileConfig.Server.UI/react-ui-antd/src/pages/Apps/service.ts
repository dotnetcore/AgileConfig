import request from '@/utils/request';

export async function queryApps() {
  return request('/app/all', {
  });
}
 
