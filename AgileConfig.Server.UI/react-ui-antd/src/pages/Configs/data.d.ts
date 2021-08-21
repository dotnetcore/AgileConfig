export type ConfigListItem = {
    appId:string
    createTime: Date
    description: string
    group: string
    id: string
    key: string
    onlineStatus: number,
    editStatus: number,
    status: number
    updateTime: Date
    value: string
}

export type ConfigModifyLog =  {
  configId: string,
  group: string,
  id: string,
  key: string,
  modifyTime: Date,
  value: string,
}

export type ConfigListParams = {
  group?: string;
  key?: string;
  pageSize?: number;
  current?: number;
};

export type JsonImportItem =  {
  group: string,
  key: string,
  value: string,
  id: string,
  appId: string
}

export type PublishTimelineNode =  {
  id: string,
  version: number,
  publishUserId: string,
  publishTime: Date
}

export type PublishDetial =  {
  group: string,
  key: string,
  value: string,
  id: string,
  editStatus: number,
  version: number
}

export type PublishDetialNode =  {
  key: string,
  timelineNode: PublishTimelineNode,
  list: PublishDetial[]
}