using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.AspNet.DependencyInjection
{
    public class OptionsAccessor<TOptions> : IOptionsAccessor<TOptions> where TOptions : new()
    {
        public OptionsAccessor(IEnumerable<IOptionsSetup<TOptions>> setups)
        {
            if (setups == null)
            {
                throw new ArgumentNullException("setups");
            }
            // REVIEW: do we need to defer setup until Options is first accessed?
            Options = setups
                .OrderBy(setup => setup.Order)
                .Aggregate(
                    new TOptions(),
                    (options, setup) =>
                    {
                        setup.Setup(options);
                        return options;
                    });
        }

        public TOptions Options { get; private set; }
    }
}