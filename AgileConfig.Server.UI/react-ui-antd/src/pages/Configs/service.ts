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
