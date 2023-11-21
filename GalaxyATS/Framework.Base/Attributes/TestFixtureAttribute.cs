using System;

namespace LZAuto.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TestFixtureAttribute : Attribute
    {
    }
}
