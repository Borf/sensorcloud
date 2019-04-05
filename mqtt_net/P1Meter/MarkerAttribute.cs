using System;

namespace P1Meter
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal class MarkerAttribute : Attribute
    {
        public string marker { get; private set; }
        public MarkerAttribute(string marker)
        {
            this.marker = marker;
        }
    }
}