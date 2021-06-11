export type UserItem = {
    id: string,
    userName: string,
    team: string,
    status: number,
    userRoles: number[],
    userRoleNames: string[]
  };
export type UserListParams = {
    name?: string;
    pageSize?: number;
    current?: number;
};