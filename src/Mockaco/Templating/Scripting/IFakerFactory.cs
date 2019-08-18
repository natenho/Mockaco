using Bogus;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Mockaco
{
    public interface IFakerFactory
    {
        Faker GetDefaultFaker();
        Faker GetFaker(IEnumerable<string> acceptLanguages);     
    }
}