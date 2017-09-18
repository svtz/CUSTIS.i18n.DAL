using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace CUSTIS.I18N.DAL.EF.Linq
{
    /// <summary> User-defined methods mapped to a database </summary>
    /// <remarks> Usage of extension methods is counterintuitive but more simple way 
    /// than custom provider implementation. In custom provider we can use <see cref="IMethodCallTranslator"/> 
    /// to map <see cref="MultiCulturalString.ToString()"/> on <code>dbo.McsGetString</code> function. </remarks>
    public static class DbUserDefinedMethods
    {
        /// <summary> <see cref="MultiCulturalString.ToString()"/> </summary>
        public static string McsGetString(this string mcs)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(bool)"/> </summary>
        public static string McsGetString(this string mcs, bool useFallback)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(CultureInfo)"/> </summary>
        public static string McsGetString(this string mcs, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(IResourceFallbackProcess)"/> </summary>
        public static string McsGetString(this string mcs, IResourceFallbackProcess resourceFallbackProcess)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(CultureInfo, bool)"/> </summary>
        public static string McsGetString(this string mcs, CultureInfo culture, bool useFallback)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(IResourceFallbackProcess, CultureInfo)"/> </summary>
        public static string McsGetString(this string mcs, IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary> <see cref="MultiCulturalString.ToString(IResourceFallbackProcess, CultureInfo, bool)"/> </summary>
        public static string McsGetString(this string mcs, IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture, bool useFallback)
        {
            throw new NotSupportedException();
        }

    }
}