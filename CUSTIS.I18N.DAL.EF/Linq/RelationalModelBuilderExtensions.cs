using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CUSTIS.I18N.DAL.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace CUSTIS.I18N.DAL.EF.Linq
{
    /// <summary>
    /// Relational database specific extension methods for <see cref="T:Microsoft.EntityFrameworkCore.ModelBuilder" />.
    /// </summary>
    public static class RelationalModelBuilderExtensions
    {
        /// <summary> Configures <see cref="O:DbUserDefinedMethods.McsGetString"/> functions </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="modelBuilder"></param>
        /// <param name="expression"></param>
        public static DbFunctionBuilder HasMcsGetStringDbFunction<TResult>(this ModelBuilder modelBuilder,
            Expression<Func<TResult>> expression)
        {
            MethodCallExpression body = expression.Body as MethodCallExpression;
            MethodInfo methodInfo = body != null ? body.Method : (MethodInfo)null;
            if (methodInfo == null)
                throw new ArgumentException("The provided DbFunction expression is invalid.", nameof(expression));

            return modelBuilder.HasDbFunction(expression)
                .HasName("McsGetString")
                .HasSchema("dbo")
                .HasTranslation(args => GetTranslationForMcsGetString(methodInfo, args));
        }

        private static Expression GetTranslationForMcsGetString(MethodInfo methodInfo, IReadOnlyCollection<Expression> args)
        {
            var targetObjectExpr = methodInfo.IsStatic ? args.FirstOrDefault() : null;
            var arguments = new ReadOnlyCollection<Expression>(args.Skip(1).ToList());
            var newExprs = McsGetStringLinqTranslator.TranslateParametersMcsGetString(methodInfo, targetObjectExpr, arguments);

            return new SqlFunctionExpression("McsGetString", methodInfo.ReturnType, "dbo", newExprs);
        }
    }
}
