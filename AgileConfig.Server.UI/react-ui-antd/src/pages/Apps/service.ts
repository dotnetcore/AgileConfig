import request from '@/utils/request';
import { AppListItem, AppListParams, AppListResult } from './data';

export async function queryApps(params:AppListParams) {
  return request<AppListResult>('/app/search', {
    params
  });
}
 

export async function addApp(params:AppListItem) {
  return request('/app/add', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function editApp(params:AppListItem) {
  return request('/app/edit', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function delApp(params:AppListItem) {
  return request('/app/delete', {
    method: 'POST',
    params: {
      id: params.id
    }
  });
}
export async function inheritancedApps(currentAppId: string) {
  return request('/app/InheritancedApps', {
    params: {
      currentAppId: currentAppId
    }
  });
}

export async function enableOrdisableApp(appId:string) {
  return request('/app/DisableOrEanble', {
    method: 'POST',
    params: {
      id: appId
    }
  });
}