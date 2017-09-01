using NHibernate.Linq.Functions;

namespace CUSTIS.I18N.DAL.NH.Linq
{
    public sealed class McsLinqToHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public McsLinqToHqlGeneratorsRegistry()
        {
            this.Merge(new MultiCulturalStringToStringFunctionGenerator());
        }
    }
}
