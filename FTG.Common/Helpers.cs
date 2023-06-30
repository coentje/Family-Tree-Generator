using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FTG.Common.Enumerations;

namespace FTG.Common
{
    public static class Helpers
    {
        private readonly static Random _random = new Random();
        public static DateTime GetRandomDateFromYear(int year) => new DateTime(year, 1, 1).AddDays(RollD(365)); // Leapyears do not exist ;-)
        public static int GenerateGrief() => Helpers.RollD(4);
        public static int GetFertility(int age) => age switch
        {
            < 12 => 0,
            < 14 => 10,
            14 => 20,
            15 => 40,
            16 => 60,
            17 => 80,
            < 30 => 98,
            < 35 => 80,
            < 40 => 50,
            < 45 => 10,
            < 50 => 5,
            _ => 0
        };
        public static string GeneratePersonalityType()
        {
            var EI = Helpers.RollD(4) > 1 ? "E" : "I";
            var SN = Helpers.RollD(4) > 1 ? "S" : "N";
            var TF = Helpers.RollD(2) > 1 ? "T" : "F";
            var JP = Helpers.RollD(2) > 1 ? "J" : "P";
            return $"{EI}{SN}{TF}{JP}";
        }
        public static string GetPersonalityTypeName(string personalityType) => personalityType switch
        {
            "ISFP" => "(Artisan/Composer)",
            "ISTP" => "(Artisan/Crafter)",
            "ESFP" => "(Artisan/Performer)",
            "ESTP" => "(Artisan/Promoter)",
            "ISFJ" => "(Guardian/Protector)",
            "ISTJ" => "(Guardian/Inspector)",
            "ESFJ" => "(Guardian/Provider)",
            "ESTJ" => "(Guardian/Supervisor)",
            "INFP" => "(Idealist/Healer)",
            "INFJ" => "(Idealist/Counselor)",
            "ENFP" => "(Idealist/Champion)",
            "ENFJ" => "(Idealist/Teacher)",
            "INTP" => "(Rational/Architect)",
            "INTJ" => "(Rational/Mastermind)",
            "ENTP" => "(Rational/Inventor)",
            "ENTJ" => "(Rational/Field Marshal)",
            _ => " --Oops! I didn't get the type"
        };
        public static int GetAge(DateTime birthDate, DateTime currentDate)
        {
            return (int)Math.Floor((currentDate - birthDate).TotalDays / 365.25);
        }
        public static int RollD(int sides)
        {
            return _random.Next(1, sides);
        }
        /// <summary>
        /// Get a normally distributed value from mean and stdev.
        /// Source:  http://www.protonfish.com/random.shtml
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="StdDev"></param>
        /// <returns></returns>
        public static double Rnd(double mean, double stdDev)
        {
            double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }
        /// <summary>
        /// Simulate a normal distribution with three random numbers:
        /// </summary>
        /// <returns></returns>
        private static double RndSnd()
        {

            return (_random.NextDouble() * 2 - 1) + (_random.NextDouble() * 2 - 1) + (_random.NextDouble() * 2 - 1);
        }
        public static Gender RandomGender() => Helpers.RollD(100) < Constants.Rate.Birth.Male ? Gender.Male : Gender.Female;
        public static Gender OppositeGender(Gender gender) => gender switch
        {
            Gender.Male => Gender.Female,
            Gender.Female => Gender.Male,
            _ => RandomGender()
        };
        public static int GetMarriageAge(Gender gender) => gender switch
        {
            Gender.Male => (int)Math.Floor(Math.Max(Constants.Age.Marriage.Male.Min, Rnd(Constants.Age.Marriage.Male.Mean, Constants.Age.Marriage.Male.StdDev))),
            Gender.Female => (int)Math.Floor(Math.Max(Constants.Age.Marriage.Female.Min, Rnd(Constants.Age.Marriage.Female.Mean, Constants.Age.Marriage.Female.StdDev))),
            _ => throw new ArgumentOutOfRangeException(nameof(gender))
        };
        public static int GetDeathAge(Gender gender)
        {
            int age;
            if (Helpers.RollD(100) < Constants.Rate.Death.ChildHood)
            {
                if (Helpers.RollD(2) == 1)
                {
                    age = 1;
                }
                else if (Helpers.RollD(2) == 1)
                {
                    age = Helpers.RollD(5);
                }
                else
                {
                    age = Helpers.RollD(15);
                }
            }
            else
            {
                age = (int)Math.Floor(gender switch
                {
                    Gender.Male => Rnd(Constants.Age.Male.Mean, Constants.Age.Male.StdDev),
                    Gender.Female => Rnd(Constants.Age.Female.Mean, Constants.Age.Female.StdDev),
                    _ => throw new ArgumentOutOfRangeException(nameof(gender))
                });
                var accident = (int)Math.Floor(age * Constants.Rate.Death.Accident);
                if (Helpers.RollD(100) < accident)
                {
                    age = Helpers.RollD(age);
                }
            }
            return age;
        }
        public static DateTime BirthDateFromMarriageDate(DateTime marriageDate, Gender gender)
        {
            return GetRandomDateFromYear(marriageDate.Year - GetMarriageAge(gender));
        }

    }
}