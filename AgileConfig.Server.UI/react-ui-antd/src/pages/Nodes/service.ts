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
 
