import { Request, Response } from 'express';

const getClients = (req: Request, resp: Response) => {
    const list = [
        {
            appId: "cc",
            id: "77d21f83-e7cc-4006-881b-adb8698878b3",
            lastHeartbeatTime: "2021-02-18T14:51:49.8073044+08:00",
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

const reloadClientConfig =  (req: Request, resp: Response) => {
    resp.json({
        success: true,
    });
}

const clientOffline = (req: Request, resp: Response) => {
    resp.json({
        success: true,
    });
}

export default {
    'GET /report/ServerNodeClients': getClients,
    'POST /RemoteServerProxy/Client_Reload': reloadClientConfig,
    'POST /RemoteServerProxy/Client_Offline': clientOffline
  };