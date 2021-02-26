export type ConfigListItem = {
    appId:string
    createTime: Date
    description: string
    group: string
    id: string
    key: string
    onlineStatus: number
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