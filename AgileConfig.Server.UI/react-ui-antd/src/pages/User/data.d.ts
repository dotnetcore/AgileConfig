export type UserItem = {
    id: string,
    userName: string,
    team: string,
    status: number,
    userRoles: number[]
  };
export type UserListParams = {
    name?: string;
    pageSize?: number;
    current?: number;
};