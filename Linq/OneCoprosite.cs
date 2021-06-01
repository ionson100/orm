using System;

namespace ORM_1_21_.Linq
{
    internal class OneComprosite
    {
       

        internal Evolution Operand { get; set; }
        private string _body;

        public object NewConctructor { get; set; }

        public bool IsAgregate { get; set; }

        public string Body
        {
            get { return _body; }
            set { _body = value.TrimEnd(','); }
        }
        public Delegate ExpressionDelegate { get; set; }
    }

   
}
