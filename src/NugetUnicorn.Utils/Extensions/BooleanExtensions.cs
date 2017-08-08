using System;

namespace NugetUnicorn.Utils.Extensions
{
    public static class BooleanExtensions
    {
        public static void IfTrue(this bool value, Action ifTrueAction)
        {
            if (value)
            {
                ifTrueAction();
            }
        }
    }
}