using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.IService
{
    public interface IEventRegister
    {
        void Init();
    }

    public class EventKeys
    {
        public const string ADD_SYSLOG = "ADD_SYSLOG";

        public const string ADD_RANGE_SYSLOG = "ADD_RANGE_SYSLOG";

    }
}
