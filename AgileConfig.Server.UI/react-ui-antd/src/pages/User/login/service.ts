import request from '@/utils/request';

export async function PasswordInited() {
  return request('/admin/PasswordInited', {
  });
}
 
