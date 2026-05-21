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
    creator?: string
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
  authorizedUsers?: string[]
}

export type AppImportPreviewItem = {
  appId: string;
  name: string;
  group?: string;
  enabled: boolean;
  inheritanced: boolean;
  inheritancedApps: string[];
  envCount: number;
  configCount: number;
  order: number;
}

export type AppImportPreviewResult = {
  apps: AppImportPreviewItem[];
  errors: string[];
}

export type AppImportFile = {
  schemaVersion: number;
  exportedAt: string;
  apps: {
    app: {
      id: string;
      name: string;
      group?: string;
      secret?: string;
      enabled: boolean;
      type: number;
      inheritanced: boolean;
      inheritancedApps: string[];
    };
    envs: Record<string, { group?: string; key: string; value?: string; description?: string }[]>;
  }[];
}
