using System.Collections.Generic;
using System.Linq.Expressions;

namespace ORM_1_21_.Linq
{
    internal class ContainerCastExpression
    {
        public Evolution TypeRevalytion { get; set; }
        public Expression CastomExpression { get; set; }
        public List<object> ParamList { get; set; }
    }
}