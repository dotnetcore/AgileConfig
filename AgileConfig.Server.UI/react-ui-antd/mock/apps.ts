import { Request, Response } from 'express';

const getApps = (req: Request, resp: Response) => {
    const list = [
        {
            createTime: "2021-02-03T16:55:48.0515423",
            enabled: true,
            id: "test_app",
            inheritanced: false,
            inheritancedApps: null,
            name: "测试程序",
            secret: null,
            updateTime: "2021-02-03T16:55:48.0515423"
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
    'GET /app/all': getApps,
  };