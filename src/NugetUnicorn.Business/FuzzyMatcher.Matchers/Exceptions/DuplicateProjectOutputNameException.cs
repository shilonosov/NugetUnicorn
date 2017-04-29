using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.Exceptions
{
    public class DuplicateProjectOutputNameException : ApplicationException
    {
        public DuplicateProjectOutputNameException(string message) : base(message)
        {
        }
    }
}
