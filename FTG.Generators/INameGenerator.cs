using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTG.Common.Enumerations;

namespace FTG.Generators
{
    public interface INameGenerator
    {
        IEnumerable<string> GetCountryCodes();
        string GetRandomCountryCode();
        string GenerateFirstName(Gender gender, string countryCode);
        string GenerateLastName(string countryCode);
    }
}