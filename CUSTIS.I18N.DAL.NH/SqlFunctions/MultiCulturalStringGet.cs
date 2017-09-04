using System;
using NHibernate;
using NHibernate.Dialect.Function;

namespace CUSTIS.I18N.DAL.NH.SqlFunctions
{
    [Serializable]
    public sealed class MultiCulturalStringGet : StandardSafeSQLFunction
    {
        private const int AllowedArgsCount = 3;
        public const string FunctionName = "mcs_get_string";

        public MultiCulturalStringGet() : base(FunctionName, NHibernateUtil.String, AllowedArgsCount)
        {
        }
    }
}
