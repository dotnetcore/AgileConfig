import { Request, Response } from 'express';

const getConfigs = (req: Request, resp: Response) => {
    const list = [
        {
            appId: "test_app",
            createTime: "2021-02-04T14:49:22.6700506",
            description: "fasdfasdf",
            group: "db",
            id: "0f092b2a1de64936b04d5d0f8b53014e",
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
            id: "0f092b2a1de64936b04d5d0f8b53014e",
            key: "cdn_host",
            onlineStatus: 0,
            status: 1,
            updateTime: "2021-02-09T16:15:00.3602254",
            value: "afsdjfklasjdfkljasdkfl;jaskl",
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

export default {
    'GET /config/search': getConfigs,
  };