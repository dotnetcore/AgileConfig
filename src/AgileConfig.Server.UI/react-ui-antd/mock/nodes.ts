import { Request, Response } from 'express';
const list = [
    {
        address: "http://agileconfig.xbaby.xyz:5001",
        createTime: "2021-02-03T16:56:48.5750564",
        lastEchoTime: "2021-02-18T14:39:48.5343426",
        remark: "htis",
        status: 1
    },
    {
        address: "http://agileconfig.xbaby.xyz:5002",
        createTime: "2021-02-03T16:56:48.5750564",
        lastEchoTime: "2021-02-18T14:39:48.5343426",
        remark: "htis",
        status: 0
    }
];
const getNodes = (req: Request, resp: Response) => {
    resp.json({
        current: 1,
        pageSize: 20,
        success: true,
        total:30,
        data: list
    });
}

const addNode = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    list.push(body);
    resp.json({
        message: "",
        success: true
    });
}
const delNode = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    const index = list.findIndex(x=> x.address === body.address);
    list.splice(index, 1);
    resp.json({
        message: "",
        success: true
    });
}
const allClientReload = (req: Request, resp: Response) => {
    const body = req.body;
    console.log(body);
    const index = list.findIndex(x=> x.address === body.address);
    list.splice(index, 1);
    resp.json({
        message: "",
        success: true
    });
}

export default {
    'GET /servernode/all': getNodes,
    'POST /servernode/add': addNode,
    'POST /servernode/delete': delNode,
    'POST /RemoteServerProxy/AllClients_Reload': allClientReload
  };