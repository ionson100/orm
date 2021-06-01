using System;
using System.Collections.Generic;
using System.Data;
using NLog;

namespace ORM_1_21_
{
    interface IServiceSessions
    {
        IDbCommand CommandForLinq { get; }
        object Locker { get; }
        Logger Logger { get; set; }
    }
}
