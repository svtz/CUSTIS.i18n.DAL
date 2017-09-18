using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CUSTIS.I18N.DAL.Linq;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;

namespace CUSTIS.I18N.DAL.NH.Linq
{
    public sealed class MultiCulturalStringToStringFunctionGenerator : BaseHqlGeneratorForMethod
    {
        public MultiCulturalStringToStringFunctionGenerator()
        {
            SupportedMethods = new[]
            {
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString()),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString()),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(CultureInfo))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(CultureInfo))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(CultureInfo), default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(CultureInfo), default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(IResourceFallbackProcess))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(IResourceFallbackProcess))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(IResourceFallbackProcess), default(CultureInfo))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(IResourceFallbackProcess), default(CultureInfo))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(IResourceFallbackProcess), default(CultureInfo), default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(IResourceFallbackProcess), default(CultureInfo), default(bool)))

            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObjectExpr, ReadOnlyCollection<Expression> arguments,
            HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            var exprs = McsGetStringLinqTranslator.TranslateParametersMcsGetString(method, targetObjectExpr, arguments);

            var parameters = exprs.Select(e => visitor.Visit(e).AsExpression());
            HqlTreeNode result = treeBuilder.MethodCall("mcs_get_string", parameters);

            return result;
        }
    }
}
