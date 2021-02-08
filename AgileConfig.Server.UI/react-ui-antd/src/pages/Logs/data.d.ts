export type LogListItem = {
    id: number;
    appId: string;
    logText: string;
    logTime: string;
    logType: number
  };
  
export type LogListParams = {
  appId?: number;
  logTime?: string;
  pageSize?: number;
  current?: number;
};
  