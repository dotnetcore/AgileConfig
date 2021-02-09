import request from '@/utils/request';

export async function queryClients() {
  return request('/report/ServerNodeClients', {
  });
}
 
