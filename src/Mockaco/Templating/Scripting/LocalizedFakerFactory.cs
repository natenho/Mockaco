using Bogus;
using Bogus.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mockaco
{
    public class LocalizedFakerFactory : IFakerFactory
    {
        private readonly ILogger<LocalizedFakerFactory> _logger;

        public LocalizedFakerFactory(ILogger<LocalizedFakerFactory> logger)
        {
            _logger = logger;
        }

        public Faker GetDefaultFaker()
        {
            var currentCultureBogusLocale = CultureInfo.DefaultThreadCurrentCulture?.ToBogusLocale();

            if (currentCultureBogusLocale != null)
            {
                return new Faker(currentCultureBogusLocale);
            }

            return new Faker();
        }

        public Faker GetFaker(IEnumerable<string> acceptLanguages)
        {
            var supportedBogusLocales = GetSupportedBogusLocales(acceptLanguages);

            var firstSupportedBogusLocale = supportedBogusLocales.FirstOrDefault();

            if (firstSupportedBogusLocale == default)
            {
                return GetDefaultFaker();
            }

            return new Faker(firstSupportedBogusLocale);
        }

        private IEnumerable<string> GetSupportedBogusLocales(IEnumerable<string> acceptLanguages)
        {
            var bogusLocales = new List<string>();

            foreach (var acceptLanguage in acceptLanguages)
            {
                try
                {
                    var cultureInfo = CultureInfo.GetCultureInfo(acceptLanguage);

                    var bogusLocale = cultureInfo.ToBogusLocale();

                    if (bogusLocale == default)
                    {
                        _logger.LogWarning("Accept-Language not supported by Bogus: {language}", acceptLanguage);
                    }

                    bogusLocales.Add(bogusLocale);
                }
                catch (CultureNotFoundException)
                {
                    _logger.LogWarning("Accept-Language not supported: {language}", acceptLanguage);
                }                
            }

            return bogusLocales;
        }
    }
}
