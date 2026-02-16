namespace Exceptionless.DateTimeExtensions.FormatParsers;

public class PriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}
