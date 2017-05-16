using System;

namespace NugetUnicorn.Business.Extensions
{
    public static class Extensions
    {
        public static T IfNull<T>(this T t, Func<T> ifNullFunc)
            where T : class
        {
            return t ?? ifNullFunc();
        }
    }
}