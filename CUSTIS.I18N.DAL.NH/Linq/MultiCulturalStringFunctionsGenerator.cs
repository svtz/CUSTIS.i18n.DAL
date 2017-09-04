using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(CultureInfo), default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(CultureInfo), default(bool))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(IResourceFallbackProcess))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(IResourceFallbackProcess))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.ToString(default(IResourceFallbackProcess), default(CultureInfo))),
                ReflectionHelper.GetMethodDefinition<MultiCulturalString>(mcs => mcs.GetString(default(IResourceFallbackProcess), default(CultureInfo)))
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObjectExpr, ReadOnlyCollection<Expression> arguments,
            HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            IList<HqlExpression> parameters = null;
            var methodParameters = method.GetParameters();
            Expression cultureExpr;
            Expression getFallbackProcessExpr;
            Expression useFallbackExpr;

            // XxString()
            if (methodParameters.Length == 0)
            {
                cultureExpr = Expression.Constant(CultureInfo.CurrentUICulture);
                useFallbackExpr = Expression.Constant(true);
                getFallbackProcessExpr = Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(CultureInfo)
            else if (methodParameters.Length == 1 
                && typeof(CultureInfo).IsAssignableFrom(methodParameters[0].ParameterType))
            {
                cultureExpr = arguments[0];
                useFallbackExpr = Expression.Constant(true);
                getFallbackProcessExpr = Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }
            
            // XxString(IResourceFallbackProcess)
            else if (methodParameters.Length == 1
                && typeof(IResourceFallbackProcess).IsAssignableFrom(methodParameters[0].ParameterType))
            {
                cultureExpr = Expression.Constant(CultureInfo.CurrentUICulture);
                useFallbackExpr = Expression.Constant(true);
                // See MCS.GetString(): resourceFallbackProcess ?? DefaultResourceFallbackProcess;
                getFallbackProcessExpr =
                    Expression.Condition(
                        Expression.NotEqual(arguments[0], Expression.Constant(null, typeof(IResourceFallbackProcess))),
                        arguments[0],
                        Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess),
                        typeof(IResourceFallbackProcess));
            }

            // XxString(CultureInfo, bool)
            else if (methodParameters.Length == 2 
                && typeof(CultureInfo).IsAssignableFrom(methodParameters[0].ParameterType)
                && typeof(bool).IsAssignableFrom(methodParameters[1].ParameterType))
            {
                cultureExpr = arguments[0];
                useFallbackExpr = arguments[1];
                getFallbackProcessExpr = Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(IResourceFallbackProcess, CultureInfo)
            else if (methodParameters.Length == 2
                && typeof(IResourceFallbackProcess).IsAssignableFrom(methodParameters[0].ParameterType)
                && typeof(CultureInfo).IsAssignableFrom(methodParameters[1].ParameterType))
            {
                cultureExpr = arguments[1];
                useFallbackExpr = Expression.Constant(true);
                // See MCS.GetString(): resourceFallbackProcess ?? DefaultResourceFallbackProcess;
                getFallbackProcessExpr =
                    Expression.Condition(
                        Expression.NotEqual(arguments[0], Expression.Constant(null, typeof(IResourceFallbackProcess))),
                        arguments[0],
                        Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess),
                        typeof(IResourceFallbackProcess));
            }

            else 
                throw new NotSupportedException();

            if (cultureExpr.NodeType != ExpressionType.Constant)
            {
                var cultureInfoResult = (CultureInfo)Expression.Lambda(cultureExpr).Compile().DynamicInvoke();
                cultureExpr = Expression.Constant(cultureInfoResult);
            }
            

            Expression fallbackChainStringExpr = GetFallbackChainStringExpr(cultureExpr, useFallbackExpr, getFallbackProcessExpr);
            var fallbackStrResult = (string)Expression.Lambda(fallbackChainStringExpr).Compile().DynamicInvoke();
            var fallbackStrResultExpr = Expression.Constant(fallbackStrResult, typeof(string));

            Expression cultureNameExpr = getCultureNameExpr(cultureExpr);
            var cultureNameResultExpr = Expression.Constant((string)Expression.Lambda(cultureNameExpr).Compile().DynamicInvoke(), typeof(string));

            parameters = new List<HqlExpression>
                {
                    visitor.Visit(targetObjectExpr).AsExpression(),
                    visitor.Visit(cultureNameResultExpr).AsExpression(),
                    visitor.Visit(fallbackStrResultExpr).AsExpression()
                };
            HqlTreeNode result = treeBuilder.MethodCall("mcs_get_string", parameters);

            return result;
        }

        private Expression getCultureNameExpr(Expression getCultureExpr)
        {
            return Expression.Property(getCultureExpr,
                (PropertyInfo) ReflectionHelper.GetProperty((CultureInfo ci) => ci.Name));
        }

        private Expression GetFallbackChainStringExpr(Expression getCultureExpr,
            Expression useFallbackExpr,
            Expression getFallbackProcessExpr)
        {
            var getFallbackChainMethodExpr = Expression.Call(getFallbackProcessExpr,
                ReflectionHelper.GetMethod<IResourceFallbackProcess>(p => p.GetFallbackChain(default(CultureInfo))),
                getCultureExpr);

            var cultureNameProjectionExpr = Expression.Call(null,
                ReflectionHelper.GetMethod(
                    () => Enumerable.Select(default(IEnumerable<CultureInfo>), default(Func<CultureInfo, string>))),
                getFallbackChainMethodExpr, 
                (Expression<Func<CultureInfo, string>>)(ci => ci.Name));

            var stringJoinMethodExpr = Expression.Call(null,
                ReflectionHelper.GetMethod(
                    () => string.Join(default(string), default(IEnumerable<string>))),
                Expression.Constant(","), 
                cultureNameProjectionExpr);

            return  Expression.Condition(useFallbackExpr, stringJoinMethodExpr, Expression.Constant(null, typeof(string)));
        }
    }
}
