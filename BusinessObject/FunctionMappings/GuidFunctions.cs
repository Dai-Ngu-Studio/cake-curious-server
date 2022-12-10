using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;

namespace BusinessObject.FunctionMappings
{
    public static class GuidFunctions
    {
        public static bool IsGreaterThan(this Guid left, Guid right) => left.CompareTo(right) > 0;

        public static void Register(ModelBuilder modelBuilder)
        {
            RegisterFunction(modelBuilder, nameof(IsGreaterThan), ExpressionType.GreaterThan);
        }

        static void RegisterFunction(ModelBuilder modelBuilder, string name, ExpressionType type)
        {
            var method = typeof(GuidFunctions).GetMethod(name, new[] { typeof(Guid), typeof(Guid) });
            modelBuilder.HasDbFunction(method!).HasTranslation(parameters =>
            {
                var left = parameters.ElementAt(0);
                var right = parameters.ElementAt(1);
                return new SqlBinaryExpression(type, left, right, typeof(bool), null);
            });
        }
    }
}
