using System;

namespace Exceptionless.DateTimeExtensions.FormatParsers {
    public class PriorityAttribute : Attribute {
        public PriorityAttribute(int priority) {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }

}
