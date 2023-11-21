using System;

namespace LZAuto.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NotificationFramework : Attribute
    {
    }
}
