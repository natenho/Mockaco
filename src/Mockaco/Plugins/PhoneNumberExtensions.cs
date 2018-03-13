using Bogus.DataSets;

namespace Bogus
{
    // TODO Move to plugin
    public static class PhoneNumberExtensions
    {
        public static string BrazilianPhoneNumber(this PhoneNumbers phoneNumbers)
        {
            var faker = new Faker("pt_BR");
            string[] brazilianAreaCodes = { "11", "12", "13", "14", "15", "16", "17", "18", "19", "21", "22", "24", "27", "28", "31", "32", "33", "34", "35", "37", "38", "41", "42", "43", "44", "45", "46", "47", "48", "49", "51", "53", "54", "55", "61", "62", "63", "64", "65", "66", "67", "68", "69", "71", "73", "74", "75", "77", "79", "81", "82", "83", "84", "85", "86", "87", "88", "89", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
            var areaCode = faker.PickRandom(brazilianAreaCodes);
            var sufix = faker.Random.Number(50000000, 99999999);

            return $"{areaCode}9{sufix}";
        }
    }
}