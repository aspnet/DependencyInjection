using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.AspNet.DependencyInjection
{
    public class OptionsAccessor<TOptions> : IOptionsAccessor<TOptions> where TOptions : new()
    {
        public OptionsAccessor(IEnumerable<IOptionsSetup<TOptions>> setups)
        {
            // REVIEW: do we need to defer setup until Options is first accessed?
            if (setups == null)
            {
                Options = new TOptions();
            }
            else
            {
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
        }

        public TOptions Options { get; private set; }
    }
}