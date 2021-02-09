import request from '@/utils/request';

export async function queryNodes() {
  return request('/servernode/all', {
  });
}
 
