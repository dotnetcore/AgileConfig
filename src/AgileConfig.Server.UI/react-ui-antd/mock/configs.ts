import { Request, Response } from 'express';
const list = [
    {
        appId: "test_app",
        createTime: "2021-02-04T14:49:22.6700506",
        description: "fasdfasdf",
        group: "db",
        id: "sdfsdf",
        key: "cdn_host",
        onlineStatus: 1,
        status: 1,
        updateTime: "2021-02-09T16:15:00.3602254",
        value: "afsdjfklasjdfkljasdkfl;jaskl",
    },
    {
        appId: "test_app",
        createTime: "2021-02-04T14:49:22.6700506",
        description: "fasdfasdf",
        group: "db",
        id: "sdfsdfsdfsdf",
        key: "cdn_host",
        onlineStatus: 0,
        status: 1,
        updateTime: "2021-02-09T16:15:00.3602254",
        value: "afsdjfklasjdfkljasdkfl;jaskl",
    }
];
const getConfigs = (req: Request, resp: Response) => {
    resp.json({
        current: 1,
        pageSize: 20,
        success: true,
        total:30,
        data: list
    });
}

const offline = (req: Request, resp: Response) => {
    const id = req.query['configId'];
    const config = list.find(x=>x.id == id)
    if (config) {
        config.onlineStatus = 0;
    }
    resp.json({
        success: true,
    });
}

const online = (req: Request, resp: Response) => {
    const id = req.query['configId'];
    const config = list.find(x=>x.id == id)
    if (config) {
        config.onlineStatus = 1;
    }
    resp.json({
        success: true,
    });
}

const delConfig = (req: Request, resp: Response) => {
    const id = req.query['id'];
    const idx = list.findIndex(x=>x.id == id)
    list.splice(idx,1);
    resp.json({
        success: true,
    });
}

const addConfig = (req: Request, resp: Response) => {
    const body = req.body;
    body.id = 'sdfsdfsdf';
    list.push(body);
    resp.json({
        success: true,
    });
}

const editConfig = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    const config = list.find(x=> x.id === body.id);
    if (config) {
      config.group = body.group;
      config.key = body.key;
      config.value = body.value;
      config.description = body.description;
    }
    resp.json({
        message: "",
        success: true
    });
}

const modifyLogs = (req: Request, resp: Response) => {
  const list = [
    {
      configId: "0583520cd95f42c58a4ba3e3fb7dcb52",
      group: "",
      id: "8d1c8b76edf84dc6b147978a3065b27d",
      key: "key",
      modifyTime: "2021-02-22T17:16:52.9883836",
      value: "155653",
    },
    {
      configId: "0583520cd95f42c58a4ba3e3fb7dcb52",
      group: "",
      id: "8d1c8b76edf84dc6b147978a3065b27d",
      key: "key",
      modifyTime: "2021-02-22T17:16:52.9883836",
      value: "155653",
    }
  ];
  resp.json({
    current: 1,
    pageSize: 20,
    success: true,
    total:30,
    data: list
  });
}

const rollback = (req: Request, resp: Response) => {
  const body = req.body;
  console.log(body);
  resp.json({
      message: "",
      success: true
  });
}


export default {
    'GET /config/search': getConfigs,
    'POST /config/offline': offline,
    'POST /config/publish': online,
    'POST /config/delete': delConfig,
    'POST /config/add': addConfig,
    'POST /config/edit': editConfig,
    'GET /config/modifylogs': modifyLogs,
    'POST /config/rollback': rollback
  };