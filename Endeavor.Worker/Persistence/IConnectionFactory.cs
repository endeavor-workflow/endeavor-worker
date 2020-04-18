using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Endeavor.Worker.Persistence
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
