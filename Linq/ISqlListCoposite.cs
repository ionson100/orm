using System.Collections.Generic;
using System.Data;
using ORM_1_21_.Transaction;

namespace ORM_1_21_.Linq
{
    interface ISqlComposite
    {
        Transactionale Transactionale { get; set; }
        ISession Sessione { get; }
        IDbTransaction Transaction { get; set; }
        List<ContainerCastExpression> ListCastExpression { get; }
    }
}
