import request from '@/utils/request';
import { InitPasswordModel } from './data';

export async function initPassword(model:InitPasswordModel) {
  return request('/admin/InitPassword', {
      data: {
          ...model
      },
      method: "POST"
  });
}
 
