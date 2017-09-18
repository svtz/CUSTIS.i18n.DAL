using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CUSTIS.I18N.DAL.Linq
{
    /// <summary>
    /// Helpers for translating expression <see cref="O:MultiCulturalString.ToString"/> to database
    /// </summary>
    public sealed class McsGetStringLinqTranslator
    {
        /// <summary> Translates <see cref="O:MultiCulturalString.ToString"/> parameters </summary>
        /// <remarks> Calculates expressions for <paramref name="arguments"/> on client </remarks>
        /// <param name="method">Method</param>
        /// <param name="targetObjectExpr"> Target object expression or <c>this</c>-parameter expression (if <paramref name="method"/> is static)</param>
        /// <param name="arguments"> Arguments expressions except <c>this</c>-parameter </param>
        public static ReadOnlyCollection<Expression> TranslateParametersMcsGetString(MethodInfo method, Expression targetObjectExpr,
            ReadOnlyCollection<Expression> arguments)
        {
            var methodParameters = (method.IsStatic ? method.GetParameters().Skip(1) : method.GetParameters()).ToArray();
            var parametersCount = methodParameters.Length;

            Expression cultureExpr;
            Expression getFallbackProcessExpr;
            Expression useFallbackExpr;

            // XxString()
            if (parametersCount == 0)
            {
                cultureExpr = Expression.Constant(CultureInfo.CurrentUICulture);
                useFallbackExpr = Expression.Constant(true);
                getFallbackProcessExpr =
                    Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(CultureInfo)
            else if (parametersCount == 1
                     && typeof(CultureInfo).IsAssignableFrom(methodParameters[0].ParameterType))
            {
                cultureExpr = arguments[0];
                useFallbackExpr = Expression.Constant(true);
                getFallbackProcessExpr =
                    Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(bool)
            else if (parametersCount == 1
                     && typeof(bool).IsAssignableFrom(methodParameters[0].ParameterType))
            {
                cultureExpr = Expression.Constant(CultureInfo.CurrentUICulture);
                useFallbackExpr = arguments[0];
                getFallbackProcessExpr =
                    Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(IResourceFallbackProcess)
            else if (parametersCount == 1
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
            else if (parametersCount == 2
                     && typeof(CultureInfo).IsAssignableFrom(methodParameters[0].ParameterType)
                     && typeof(bool).IsAssignableFrom(methodParameters[1].ParameterType))
            {
                cultureExpr = arguments[0];
                useFallbackExpr = arguments[1];
                getFallbackProcessExpr =
                    Expression.Constant(GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess);
            }

            // XxString(IResourceFallbackProcess, CultureInfo)
            else if (parametersCount == 2
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

            // XxString(IResourceFallbackProcess, CultureInfo, bool)
            else if (parametersCount == 3
                     && typeof(IResourceFallbackProcess).IsAssignableFrom(methodParameters[0].ParameterType)
                     && typeof(CultureInfo).IsAssignableFrom(methodParameters[1].ParameterType)
                     && typeof(bool).IsAssignableFrom(methodParameters[2].ParameterType))
            {
                cultureExpr = arguments[1];
                useFallbackExpr = arguments[2];
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

            // TODO: cache all compiled expressions

            if (cultureExpr.NodeType != ExpressionType.Constant)
            {
                var cultureInfoResult = (CultureInfo) Expression.Lambda(cultureExpr).Compile().DynamicInvoke();
                cultureExpr = Expression.Constant(cultureInfoResult);
            }

            Expression fallbackChainStringExpr =
                GetFallbackChainStringExpr(cultureExpr, useFallbackExpr, getFallbackProcessExpr);
            var fallbackStrResult = (string) Expression.Lambda(fallbackChainStringExpr).Compile().DynamicInvoke();
            var fallbackStrResultExpr = Expression.Constant(fallbackStrResult, typeof(string));

            Expression cultureNameExpr = GetCultureNameExpr(cultureExpr);
            var cultureNameResultExpr =
                Expression.Constant((string) Expression.Lambda(cultureNameExpr).Compile().DynamicInvoke(), typeof(string));

            var exprs = new ReadOnlyCollection<Expression>(new Expression[]
                {targetObjectExpr, cultureNameResultExpr, fallbackStrResultExpr});
            return exprs;
        }

        private static Expression GetCultureNameExpr(Expression getCultureExpr)
        {
            return Expression.Property(getCultureExpr,
                (PropertyInfo) ReflectionHelper.GetProperty((CultureInfo ci) => ci.Name));
        }

        private static Expression GetFallbackChainStringExpr(Expression getCultureExpr,
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