export type AppListItem = {
    id: string,
    name?: string,
    enabled?: boolean,
    inheritanced: boolean,
    inheritancedApps?: string[],
    inheritancedAppNames?: string[],
    secret?: string,
    createTime?: Date,
    updateTime?: Date
    appAdmin: string,
    appAdminName: string
  };
    
export type AppListParams = {
  id?: string;
  name?: string;
  pageSize?: number;
  current?: number;
  sortField: string;
  ascOrDesc: string;
  tableGrouped: boolean;
};

export type AppListResult = {
    current: number
    data: AppListItem[]
    pageSize: number
    success: boolean
    total: number
}

export type UserAppAuth = {
  appId: string,
  editConfigPermissionUsers?: string[],
  publishConfigPermissionUsers?: string[]
}