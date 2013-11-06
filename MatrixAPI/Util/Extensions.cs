using System;
using System.Linq;

namespace MatrixAPI.Util
{
    public static class Extensions
    {
        public static Type[] GetTopLevelInterfaces(this Type t)
        {
            Type[] allInterfaces = t.GetInterfaces();
            var selection = allInterfaces
                .Where(x => !allInterfaces.Any(y => y.GetInterfaces().Contains(x)))
                .Except(t.BaseType.GetInterfaces());
            return selection.ToArray();
        }
    }
}