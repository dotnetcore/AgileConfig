import { Request, Response } from 'express';
const list = [
    {
        createTime: "2021-02-03T16:55:48.0515423",
        enabled: true,
        id: "cc",
        inheritanced: false,
        inheritancedApps: [],
        name: "测试程序",
        secret: null,
        updateTime: "2021-02-03T16:55:48.0515423"
    },
    {
        createTime: "2021-02-03T16:55:48.0515423",
        enabled: false,
        id: "aa",
        inheritanced: true,
        inheritancedApps: ['2'],
        name: "公共程序1",
        secret: null,
        updateTime: "2021-02-03T16:55:48.0515423"
    }
];

const inheritancedApps = (req: Request, resp: Response) => {
    resp.json({
        current: 1,
        pageSize: 20,
        success: true,
        total:30,
        data: list.filter(x=>x.inheritanced)
    });
}

const getApps = (req: Request, resp: Response) => {
    resp.json({
        current: 1,
        pageSize: 20,
        success: true,
        total:30,
        data: list
    });
}
const addApp = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    list.push(body);
    resp.json({
        message: "",
        success: true
    });
}
const delApp = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    const index = list.findIndex(x=> x.id === body.id);
    list.splice(index, 1);
    resp.json({
        message: "",
        success: true
    });
}
const editApp = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    const app = list.find(x=> x.id === body.id);
    if (app) {
      app.name = body.name;
      app.id = body.id;
      app.secret = body.secret;
      app.inheritanced = body.inheritanced;
      app.inheritancedApps = body.inheritancedApps;
      app.enabled = body.enabled;
    }
    resp.json({
        message: "",
        success: true
    });
}
export default {
    'GET /app/search': getApps,
    'POST /app/add': addApp,
    'POST /app/delete': delApp,
    'POST /app/edit': editApp,
    'GET /app/inheritanced': inheritancedApps
  };