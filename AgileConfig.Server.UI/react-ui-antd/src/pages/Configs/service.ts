import request from '@/utils/request';
import { ConfigListItem, ConfigListParams, JsonImportItem } from './data';

export async function queryConfigs(appId:string,env:string, params: ConfigListParams) {
  return request('config/search', {
    params:
      {
        appId,
        env,
        ...params
      }
  });
}

export async function onlineConfig(config: ConfigListItem, env: string) {
  return request('config/publish?env=' + env, {
    method: 'POST',
    params: {
      configId: config.id
    }
  });
}

export async function onlineSomeConfigs(configs: ConfigListItem[], env: string) {
  return request('config/PublishSome?env=' + env, {
    method: 'POST',
    data: configs.map(c=>c.id)
  });
}
export async function offlineSomeConfigs(configs: ConfigListItem[], env: string) {
  return request('config/OfflineSome?env=' + env, {
    method: 'POST',
    data: configs.map(c=>c.id)
  });
}
export async function offlineConfig(config: ConfigListItem, env:string) {
  return request('config/offline?env=' + env, {
    method: 'POST',
    params: {
      configId: config.id
    }
  });
}

export async function delConfig(config: ConfigListItem, env:string) {
  return request('config/delete?env=' + env, {
    method: 'POST',
    params: {
      id: config.id
    }
  });
}

export async function delConfigs(configs: ConfigListItem[], env:string) {
  return request('config/deleteSome?env=' + env, {
    method: 'POST',
    data: configs.map(c=>c.id)
  });
}

export async function addConfig(config: ConfigListItem, env:string) {
  return request('config/add?env=' + env, {
    method: 'POST',
    data: {
      ...config
    }
  });
}

export async function addRangeConfig(list: JsonImportItem[], env: string) {
  return request('config/AddRange?env=' + env, {
    method: 'POST',
    data: list
  });
}

export async function editConfig(config: ConfigListItem, env: string) {
  return request('config/edit?env=' + env, {
    method: 'POST',
    data: {
      ...config
    }
  });
}

export async function queryConfigPublishedHistory(config: ConfigListItem, env: string) {
  return request('config/ConfigPublishedHistory?env=' + env, {
    method: 'GET',
    params:{
      configId: config.id
    }
  });
}

export async function rollback(publishTimelineId: string, env:string) {
  return request('config/rollback?env=' + env, {
    method: 'POST',
    params:{
      publishTimelineId: publishTimelineId,
    }
  });
}

export async function getWaitPublishStatus(appId: string, env: string) {
  return request('config/WaitPublishStatus', {
    method: 'GET',
    params:{
      appId: appId,
      env: env
    }
  });
}

export async function publish(appId: string, publistLog: string, env:string) {
  return request('config/publish?env=' + env, {
    method: 'POST',
    data:{
      log:publistLog,
      appId: appId
    }
  });
}

export async function getPublishHistory(appId: string, env:string) {
  return request('config/publishHistory?env=' + env, {
    method: 'GET',
    params:{
      appId: appId
    }
  });
}

export async function cancelEdit(configId: string, env:string) {
  return request('config/cancelEdit?env=' + env, {
    method: 'POST',
    params:{
      configId: configId
    }
  });
}

export async function cancelSomeEdit(ids: string[], env:string) {
  return request('config/cancelSomeEdit?env=' + env, {
    method: 'POST',
    data: ids
  });
}

export async function exportJson(appId: string, env:string) {
  return request('config/ExportJson?env=' + env, {
    method: 'POST',
    params:{
      appId: appId
    },
    responseType: "blob"
  });
}

export async function envSync(appId: string, currentEnv:string, toEnvs:string[]) {
  return request('config/syncenv?currentEnv=' + currentEnv, {
    method: 'POST',
    params:{
      appId: appId
    },
    data: toEnvs
  });
}

export async function getConfigsKvList(appId: string, env:string) {
  return request('config/getKvList?env=' + env, {
    method: 'GET',
    params:{
      appId: appId
    }
  });
}

export async function getConfigJson(appId: string, env:string) {
  return request('config/getjson?env=' + env, {
    method: 'GET',
    params:{
      appId: appId
    }
  });
}

export async function saveJson(appId: string, env:string, json:string) {
  return request('config/saveJson?env=' + env, {
    method: 'POST',
    params:{
      appId: appId
    },
    data: {
      json: json
    }
  });
}

export async function saveKvList(appId: string, env:string, kvstr:string) {
  return request('config/saveKvList?env=' + env, {
    method: 'POST',
    params:{
      appId: appId
    },
    data: {
      str: kvstr
    }
  });
}