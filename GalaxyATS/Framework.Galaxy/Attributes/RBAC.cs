using System;

namespace LZAuto.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RBAC : Attribute
    {
    }
}
