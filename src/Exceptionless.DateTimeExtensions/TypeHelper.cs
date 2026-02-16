using System.Diagnostics;
using System.Reflection;
using Exceptionless.DateTimeExtensions.FormatParsers;

namespace Exceptionless.DateTimeExtensions;

internal static class TypeHelper
{
    internal static IList<Type> SortByPriority(this IEnumerable<Type> types)
    {
        return types.OrderBy(t =>
        {
            var priorityAttribute = t.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() as PriorityAttribute;
            return priorityAttribute?.Priority ?? 0;
        }).ToList();
    }

    internal static IEnumerable<Type> GetDerivedTypes<TAction>(IEnumerable<Assembly>? assemblies = null)
    {
        assemblies ??= [typeof(TypeHelper).Assembly];

        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(from type in assembly.GetTypes() where type.IsClass && !type.IsNotPublic && !type.IsAbstract && typeof(TAction).IsAssignableFrom(type) select type);
            }
            catch (ReflectionTypeLoadException ex)
            {
                string loaderMessages = String.Join(", ", ex.LoaderExceptions.Where(le => le is not null).Select(le => le!.Message));
                Debug.WriteLine("Unable to search types from assembly \"{0}\" for plugins of type \"{1}\": {2}", assembly.FullName, typeof(TAction).Name, loaderMessages);
            }
        }

        return types;
    }
}
