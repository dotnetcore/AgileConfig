import request from '@/utils/request';
import { AppListParams, AppListResult } from './data';

export async function queryApps(params:AppListParams) {
  return request<AppListResult>('/app/search', {
    params
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
export async function editApp(params:any) {
  return request('/app/edit', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function delApp(params:any) {
  return request('/app/delete', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function inheritancedApps() {
  return request('/app/inheritanced', {
  });
}