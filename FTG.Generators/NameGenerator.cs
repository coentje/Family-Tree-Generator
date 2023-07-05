using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTG.Common.Enumerations;

namespace FTG.Generators
{
    public static class NameGenerator
    {
        private static readonly Random _random = new Random();
        public static string GenerateFirstName(Gender gender, string countryCode)
        {
            var fileName = $"files/names/names_{countryCode}/{countryCode.ToLowerInvariant()}{gender.ToString().ToLowerInvariant()}s.csv";
            try
            {
                var lines = File.ReadAllLines(fileName);
                if (lines.Any())
                {
                    return lines[_random.Next(0, lines.Length - 1)].Split(',').First();
                }
                else
                {
                    throw new ArgumentException($"'{fileName}' has no lines");
                }

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"'{fileName}' cannot be read.", ex);
            }
        }

        public static string GenerateLastName(string countryCode)
        {
            var fileName = $"files/names/names_{countryCode}/{countryCode.ToLowerInvariant()}surnames.csv";
            try
            {
                var lines = File.ReadAllLines(fileName);
                if (lines.Any())
                {
                    return lines[_random.Next(0, lines.Length - 1)].Split(',').First();
                }
                else
                {
                    throw new ArgumentException($"'{fileName}' has no lines");
                }

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"'{fileName}' cannot be read.", ex);
            }
        }

        public static IEnumerable<string> GetCountryCodes()
        {
            var folders = Directory.GetDirectories("files/names").Where(f => !f.Contains("inter")).ToList();
            var codes = folders.Select(f => f[^2..]);
            return codes;
        }

        public static string GetRandomCountryCode()
        {
            var codes = GetCountryCodes().ToArray<string>();
            return codes[_random.Next(_random.Next(0, codes.Length - 1))];
        }
    }
}