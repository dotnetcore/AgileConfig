import request from '@/utils/request';
import { ServiceItem } from './data';

export async function queryService(params: any) {
  return request('service/search', {
    params
  });
}
 
export async function reloadClientConfigs(address: string, clientId: string) {
  return request('RemoteServerProxy/Client_Reload', {
    method: 'POST',
    params:{
      address,
      clientId
    }
  });
}
export async function clientOffline(address: string, clientId: string) {
  return request('RemoteServerProxy/Client_Offline', {
    method: 'POST',
    params:{
      address,
      clientId
    }
  });
}

export async function addService(service: ServiceItem) {
  return request('service/add', {
    method: 'POST',
    data:{
      ...service
    }
  });
}

export async function removeService(service: ServiceItem) {
  return request('service/remove', {
    method: 'POST',
    params:{
      id: service.id
    }
  });
}