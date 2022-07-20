import request from '@/utils/request';

export async function current(): Promise<any> {
  return request('Home/current');
}