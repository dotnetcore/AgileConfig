import request from '@/utils/request';

export async function queryApps() {
  return request('/app/all', {
  });
}
 

export async function addApp(params:any) {
  return request('/app/add', {
    method: 'POST',
    data: {
      ...params
    }
  });
}

export async function delNode(params:any) {
  return request('/app/delete', {
    method: 'POST',
    data: {
      ...params
    }
  });
}