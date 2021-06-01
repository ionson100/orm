using System;

namespace ORM_1_21_
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = true)]
    internal sealed  class PersistentAttribute:System.Attribute
    {
       
    }
}