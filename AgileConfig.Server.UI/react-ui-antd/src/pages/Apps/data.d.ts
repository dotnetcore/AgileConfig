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
  };
    
export type AppListParams = {
  id?: string;
  name?: string;
  pageSize?: number;
  current?: number;
};

export type AppListResult = {
    current: number
    data: AppListItem[]
    pageSize: number
    success: boolean
    total: number
}