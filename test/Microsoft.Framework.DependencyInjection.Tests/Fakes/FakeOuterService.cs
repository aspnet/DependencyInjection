// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.DependencyInjection.Tests.Fakes
{
    public class FakeOuterService : IFakeOuterService
    {
        private readonly IFakeService _singleService;
        private readonly IEnumerable<IFakeMultipleService> _multipleServices;

        public FakeOuterService(
            IFakeService singleService,
            IEnumerable<IFakeMultipleService> multipleServices)
        {
            _singleService = singleService;
            _multipleServices = multipleServices;
        }


        public void Interrogate(out string singleValue, out string[] multipleValues)
        {
            singleValue = _singleService.SimpleMethod();

            multipleValues = _multipleServices
                .Select(x => x.SimpleMethod())
                .ToArray();
        }
    }
}