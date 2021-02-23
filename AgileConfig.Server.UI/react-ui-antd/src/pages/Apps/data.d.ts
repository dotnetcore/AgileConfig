export type AppListItem = {
    id?: string,
    name?: string,
    enabled?: boolean,
    inheritanced?: boolean,
    inheritancedApps?: string[],
    secret?: string,
    createTime?: Date,
    updateTime?: Date
  };