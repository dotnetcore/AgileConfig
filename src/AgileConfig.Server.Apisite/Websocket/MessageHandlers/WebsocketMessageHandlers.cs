using System.Collections.Generic;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Apisite.Websocket.MessageHandlers;

public class WebsocketMessageHandlers
{
    public readonly List<IMessageHandler> MessageHandlers;

    public WebsocketMessageHandlers(IConfigService configService, IRegisterCenterService registerCenterService,
        IServiceInfoService serviceInfoService)
    {
        MessageHandlers =
        [
            new OldMessageHandler(configService),
            new MessageHandler(configService, registerCenterService, serviceInfoService)
        ];
    }
}