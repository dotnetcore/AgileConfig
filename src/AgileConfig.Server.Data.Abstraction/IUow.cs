﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IUow : IDisposable
    {
        void Begin();

        Task<bool> SaveChangesAsync();

        void Rollback();

    }
}
