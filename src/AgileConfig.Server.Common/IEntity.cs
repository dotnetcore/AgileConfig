﻿using System;

namespace AgileConfig.Server.Common
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}
