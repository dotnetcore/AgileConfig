import { Request, Response } from 'express';

const getLogs = (req: Request, resp: Response) => {
    const list = [];
    for (let index = 0; index < 20; index++) {
        list.push({
            appId:'cc',
            id: index++,
            logTime:'2021-02-07T00:55:51.3888328',
            logTxt: '删除配置【Key:type】【Value：sqlite】【Group：store】【AppId：test_app】',
            logType: 0
        });
    }

    resp.json({
        current: 1,
        pageSize: 20,
        success: true,
        total:30,
        data: list
    });
}

export default {
    'GET /syslog/search': getLogs,
  };