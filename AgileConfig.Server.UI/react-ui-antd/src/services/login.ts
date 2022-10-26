import request from '@/utils/request';

export type LoginParamsType = {
  userName: string;
  password: string;
  mobile: string;
  captcha: string;
};

export async function accountLogin(params: LoginParamsType) {
  return request('admin/jwt/login', {
    method: 'POST',
    data: params,
  });
}

