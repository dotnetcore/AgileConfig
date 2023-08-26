import request from '@/utils/request';
import { UserItem, UserListParams } from './data';

export async function queryUsers(params: UserListParams) {
  return request('user/search', {
    params:
      {
        ...params
      }
  });
}

export async function addUser(params:UserItem) {
  return request('user/add', {
    method: 'POST',
    data: {
      ...params
    }
  });
}

export async function editUser(params:UserItem) {
  return request('user/edit', {
    method: 'POST',
    data: {
      ...params
    }
  });
}

export async function delUser(userId:string) {
  return request('user/delete', {
    method: 'POST',
    params:{
      userId: userId
    }
  });
}

export async function resetPassword(userId:string) {
  return request('user/resetPassword', {
    method: 'POST',
    params:{
      userId: userId
    }
  });
}

export async function adminUsers() {
  return request('user/adminUsers', {
    method: 'GET',
  });
}

export async function allUsers() {
  return request('user/allUsers', {
    method: 'GET',
  });
}