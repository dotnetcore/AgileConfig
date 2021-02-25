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

export default {
    'GET /config/search': getConfigs,
    'POST /config/offline': offline,
    'POST /config/publish': online,
  };