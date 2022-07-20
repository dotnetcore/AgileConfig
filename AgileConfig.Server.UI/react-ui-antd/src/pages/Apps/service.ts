import request from '@/utils/request';
import { AppListItem, AppListParams, AppListResult, UserAppAuth } from './data';

export async function queryApps(params:AppListParams) {
  return request<AppListResult>('app/search', {
    params
  });
}
 

export async function addApp(params:AppListItem) {
  return request('app/add', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function editApp(params:AppListItem) {
  return request('app/edit', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
export async function delApp(params:AppListItem) {
  return request('app/delete', {
    method: 'POST',
    params: {
      id: params.id
    }
  });
}
export async function inheritancedApps(currentAppId: string) {
  return request('app/InheritancedApps', {
    params: {
      currentAppId: currentAppId
    }
  });
}

export async function enableOrdisableApp(appId:string) {
  return request('app/DisableOrEanble', {
    method: 'POST',
    params: {
      id: appId
    }
  });
}

export async function saveAppAuth(model:UserAppAuth) {
  return request('app/saveAppAuth', {
    method: 'POST',
    data: model
  });
}

export async function getUserAppAuth(appId:string) {
  return request('app/GetUserAppAuth', {
    method: 'GET',
    params: {
      appId: appId
    }
  });
}

export async function getAppGroups() {
  return request('app/GetAppGroups', {
    method: 'GET',
  });
}