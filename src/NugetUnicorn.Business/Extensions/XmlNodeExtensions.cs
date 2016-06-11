using System;
using System.Xml;

namespace NugetUnicorn.Business.Extensions
{
    public static class XmlNodeExtensions
    {
        public static string GetAttribute(this XmlNode xmlNode, string attributeName, string defaultValue)
        {
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentException("attribute name is not set, and have to be", nameof(attributeName));
            }

            var attribute = xmlNode?.Attributes?[attributeName];
            return attribute?.Value ?? defaultValue;
        }
    }
}