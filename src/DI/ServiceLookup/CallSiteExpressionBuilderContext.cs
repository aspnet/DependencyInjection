using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteExpressionBuilderContext
    {
        public ParameterExpression ScopeParameter { get; set; }
        public bool RequiresResolvedServices { get; set; }
    }
}