import request from '@/utils/request';
import { NodeItem } from './data';

export async function queryNodes() {
  return request('/servernode/all', {
  });
}

export async function addNode(params:NodeItem) {
  return request('/servernode/add', {
    method: 'POST',
    data: {
      ...params
    }
  });
}

export async function delNode(params:NodeItem) {
  return request('/servernode/delete', {
    method: 'POST',
    data: {
      ...params
    }
  });
}
 
export async function allClientReload(params:NodeItem) {
  return request('/RemoteServerProxy/AllClients_Reload?address=' + params.address, {
    method: 'POST',
  });
}
 