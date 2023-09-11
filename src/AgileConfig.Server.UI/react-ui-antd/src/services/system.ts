import request from '@/utils/request';

export async function sys(): Promise<any> {
  return request('Home/sys');
}