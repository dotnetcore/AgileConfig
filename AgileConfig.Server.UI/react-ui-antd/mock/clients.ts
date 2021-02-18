import { Request, Response } from 'express';

const getClients = (req: Request, resp: Response) => {
    const list = [
        {
            appId: "test_app",
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

export default {
    'GET /report/ServerNodeClients': getClients,
  };