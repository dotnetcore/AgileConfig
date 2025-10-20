export type UserItem = {
    id: string,
    userName: string,
    team: string,
    status: number,
    userRoleIds: string[],
    userRoleNames: string[],
    userRoleCodes: string[]
  };
export type UserListParams = {
    name?: string;
    pageSize?: number;
    current?: number;
};