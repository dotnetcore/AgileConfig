import request from '@/utils/request';

export async function queryConfigs() {
  return request('/config/search', {
  });
}


