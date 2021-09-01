import request from '@/utils/request';
import { ConfigListItem, ConfigListParams, ConfigModifyLog, JsonImportItem, PublishDetial } from './data';

export async function queryConfigs(appId:string, params: ConfigListParams) {
  return request('/config/search', {
    params:
      {
        appId,
        ...params
      }
  });
}

export async function onlineConfig(config: ConfigListItem) {
  return request('/config/publish', {
    method: 'POST',
    params: {
      configId: config.id
    }
  });
}

export async function onlineSomeConfigs(configs: ConfigListItem[]) {
  return request('/config/PublishSome', {
    method: 'POST',
    data: configs.map(c=>c.id)
  });
}
export async function offlineSomeConfigs(configs: ConfigListItem[]) {
  return request('/config/OfflineSome', {
    method: 'POST',
    data: configs.map(c=>c.id)
  });
}
export async function offlineConfig(config: ConfigListItem) {
  return request('/config/offline', {
    method: 'POST',
    params: {
      configId: config.id
    }
  });
}

export async function delConfig(config: ConfigListItem) {
  return request('/config/delete', {
    method: 'POST',
    params: {
      id: config.id
    }
  });
}

export async function addConfig(config: ConfigListItem) {
  return request('/config/add', {
    method: 'POST',
    data: {
      ...config
    }
  });
}

export async function addRangeConfig(list: JsonImportItem[]) {
  return request('/config/AddRange', {
    method: 'POST',
    data: list
  });
}

export async function editConfig(config: ConfigListItem) {
  return request('/config/edit', {
    method: 'POST',
    data: {
      ...config
    }
  });
}

export async function queryConfigPublishedHistory(config: ConfigListItem) {
  return request('/config/ConfigPublishedHistory', {
    method: 'GET',
    params:{
      configId: config.id
    }
  });
}

export async function rollback(publishTimelineId: string) {
  return request('/config/rollback', {
    method: 'POST',
    params:{
      publishTimelineId: publishTimelineId,
    }
  });
}

export async function getWaitPublishStatus(appId: string) {
  return request('/config/WaitPublishStatus', {
    method: 'GET',
    params:{
      appId: appId
    }
  });
}

export async function publish(appId: string) {
  return request('/config/publish', {
    method: 'POST',
    params:{
      appId: appId
    }
  });
}

export async function getPublishHistory(appId: string) {
  return request('/config/publishHistory', {
    method: 'GET',
    params:{
      appId: appId
    }
  });
}

export async function cancelEdit(configId: string) {
  return request('/config/cancelEdit', {
    method: 'POST',
    params:{
      configId: configId
    }
  });
}