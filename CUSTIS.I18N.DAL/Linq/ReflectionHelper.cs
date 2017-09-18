using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CUSTIS.I18N.DAL.Linq
{
    internal static class ReflectionHelper
    {
        /// <summary>Gets the field or property to be accessed.</summary>
        /// <typeparam name="TSource">The declaring-type of the property.</typeparam>
        /// <typeparam name="TResult">The type of the property.</typeparam>
        /// <param name="property">The expression representing the property getter.</param>
        /// <returns>The <see cref="T:System.Reflection.MemberInfo" /> of the property.</returns>
        public static MemberInfo GetProperty<TSource, TResult>(Expression<Func<TSource, TResult>> property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            return ((MemberExpression)property.Body).Member;
        }

        /// <summary>
        /// Extract the <see cref="T:System.Reflection.MethodInfo" /> from a given expression.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The <see cref="T:System.Reflection.MethodInfo" /> of the method.</returns>
        public static MethodInfo GetMethod(Expression<Action> method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            return ((MethodCallExpression)method.Body).Method;
        }

        /// <summary>
        /// Extract the <see cref="T:System.Reflection.MethodInfo" /> from a given expression.
        /// </summary>
        /// <typeparam name="TSource">The declaring-type of the method.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns>The <see cref="T:System.Reflection.MethodInfo" /> of the method.</returns>
        public static MethodInfo GetMethod<TSource>(Expression<Action<TSource>> method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            return ((MethodCallExpression)method.Body).Method;
        }
    }
}
