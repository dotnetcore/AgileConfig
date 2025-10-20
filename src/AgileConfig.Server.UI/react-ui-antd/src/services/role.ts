import request from '@/utils/request';

export async function queryRoles() {
  return request('role/list', {
    method: 'GET',
  });
}

export async function fetchSupportedRolePermissions() {
  return request('role/supportedPermissions', {
    method: 'GET',
  });
}

export async function createRole(data: any) {
  return request('role/add', {
    method: 'POST',
    data,
  });
}

export async function updateRole(data: any) {
  return request('role/edit', {
    method: 'POST',
    data,
  });
}

export async function deleteRole(id: string) {
  return request('role/delete', {
    method: 'POST',
    params: {
      id,
    },
  });
}
