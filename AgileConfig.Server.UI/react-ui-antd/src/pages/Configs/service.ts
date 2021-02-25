import request from '@/utils/request';
import { ConfigListItem } from './data';

export async function queryConfigs() {
  return request('/config/search', {
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

export async function editConfig(config: ConfigListItem) {
  return request('/config/edit', {
    method: 'POST',
    data: {
      ...config
    }
  });
}