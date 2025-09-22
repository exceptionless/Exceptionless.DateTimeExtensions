using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Exceptionless.DateTimeExtensions.FormatParsers;

namespace Exceptionless.DateTimeExtensions;

internal static class TypeHelper
{
    internal static IList<Type> SortByPriority(this IEnumerable<Type> types)
    {
        return types.OrderBy(t =>
        {
            var priorityAttribute = t.GetTypeInfo().GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() as PriorityAttribute;
            return priorityAttribute != null ? priorityAttribute.Priority : 0;
        }).ToList();
    }

    internal static IEnumerable<Type> GetDerivedTypes<TAction>(IEnumerable<Assembly> assemblies = null)
    {
        if (assemblies == null)
            assemblies = new[] { typeof(TypeHelper).GetTypeInfo().Assembly };

        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(from type in assembly.GetTypes() where type.GetTypeInfo().IsClass && !type.GetTypeInfo().IsNotPublic && !type.GetTypeInfo().IsAbstract && typeof(TAction).IsAssignableFrom(type) select type);
            }
            catch (ReflectionTypeLoadException ex)
            {
                string loaderMessages = String.Join(", ", ex.LoaderExceptions.ToList().Select(le => le.Message));
                Debug.WriteLine("Unable to search types from assembly \"{0}\" for plugins of type \"{1}\": {2}", assembly.FullName, typeof(TAction).Name, loaderMessages);
            }
        }

        return types;
    }
}
