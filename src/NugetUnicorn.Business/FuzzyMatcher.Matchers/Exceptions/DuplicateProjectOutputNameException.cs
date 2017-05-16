using System;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.Exceptions
{
    public class DuplicateProjectOutputNameException : ApplicationException
    {
        public DuplicateProjectOutputNameException(string message)
            : base(message)
        {
        }
    }
}