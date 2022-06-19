using Bogus;
using System.Collections.Generic;

namespace Mockaco
{
    public interface IFakerFactory
    {
        Faker GetDefaultFaker();

        Faker GetFaker(IEnumerable<string> acceptLanguages);     
    }
}