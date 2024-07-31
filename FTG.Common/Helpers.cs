using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FTG.Common.Enumerations;

namespace FTG.Common
{
    /// <summary>
    /// Provides helper methods for generating random data and performing calculations related to family tree generation.
    /// </summary>
    public static class Helpers
    {
        private readonly static Random _random = new Random();

        /// <summary>
        /// Generates a random date within the specified year.
        /// </summary>
        /// <param name="year">The year for which to generate a random date.</param>
        /// <returns>A random <see cref="DateTime"/> within the specified year.</returns>
        public static DateTime GetRandomDateFromYear(int year) => new DateTime(year, 1, 1).AddDays(RollD(365));

        /// <summary>
        /// Generates a random number representing grief.
        /// </summary>
        /// <returns>A random number representing grief.</returns>
        public static int GenerateGrief() => Helpers.RollD(4);

        /// <summary>
        /// Calculates the fertility based on the age.
        /// </summary>
        /// <param name="age">The age of the person.</param>
        /// <returns>The fertility value based on the age.</returns>
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

        /// <summary>
        /// Generates a random personality type.
        /// </summary>
        /// <returns>A random personality type.</returns>
        public static string GeneratePersonalityType()
        {
            var EI = Helpers.RollD(4) > 1 ? "E" : "I";
            var SN = Helpers.RollD(4) > 1 ? "S" : "N";
            var TF = Helpers.RollD(2) > 1 ? "T" : "F";
            var JP = Helpers.RollD(2) > 1 ? "J" : "P";
            return $"{EI}{SN}{TF}{JP}";
        }

        /// <summary>
        /// Gets the name of the personality type.
        /// </summary>
        /// <param name="personalityType">The personality type.</param>
        /// <returns>The name of the personality type.</returns>
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

        /// <summary>
        /// Calculates the age based on the birth date and current date.
        /// </summary>
        /// <param name="birthDate">The birth date of the person.</param>
        /// <param name="currentDate">The current date.</param>
        /// <returns>The age of the person.</returns>
        public static int GetAge(DateTime birthDate, DateTime currentDate)
        {
            return (int)Math.Floor((currentDate - birthDate).TotalDays / 365.25);
        }

        /// <summary>
        /// Rolls a dice with the specified number of sides.
        /// </summary>
        /// <param name="sides">The number of sides on the dice.</param>
        /// <returns>A random number between 1 and the number of sides.</returns>
        public static int RollD(int sides)
        {
            return _random.Next(1, sides);
        }

        /// <summary>
        /// Generates a random value from a normal distribution with the specified mean and standard deviation.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="stdDev">The standard deviation of the normal distribution.</param>
        /// <returns>A random value from the normal distribution.</returns>
        public static double Rnd(double mean, double stdDev)
        {
            double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }

        /// <summary>
        /// Simulates a normal distribution using three random numbers.
        /// </summary>
        /// <returns>A random value from a simulated normal distribution.</returns>
        private static double RndSnd()
        {
            return (_random.NextDouble() * 2 - 1) + (_random.NextDouble() * 2 - 1) + (_random.NextDouble() * 2 - 1);
        }

        /// <summary>
        /// Generates a random gender.
        /// </summary>
        /// <returns>A random gender.</returns>
        public static Gender RandomGender() => Helpers.RollD(100) < Constants.Rate.Birth.Male ? Gender.Male : Gender.Female;

        /// <summary>
        /// Gets the opposite gender of the specified gender.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>The opposite gender of the specified gender.</returns>
        public static Gender OppositeGender(Gender gender) => gender switch
        {
            Gender.Male => Gender.Female,
            Gender.Female => Gender.Male,
            _ => RandomGender()
        };

        /// <summary>
        /// Gets the marriage age based on the gender.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>The marriage age based on the gender.</returns>
        public static int GetMarriageAge(Gender gender) => gender switch
        {
            Gender.Male => (int)Math.Floor(Math.Max(Constants.Age.Marriage.Male.Min, Rnd(Constants.Age.Marriage.Male.Mean, Constants.Age.Marriage.Male.StdDev))),
            Gender.Female => (int)Math.Floor(Math.Max(Constants.Age.Marriage.Female.Min, Rnd(Constants.Age.Marriage.Female.Mean, Constants.Age.Marriage.Female.StdDev))),
            _ => throw new ArgumentOutOfRangeException(nameof(gender))
        };

        /// <summary>
        /// Gets the death age based on the gender.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>The death age based on the gender.</returns>
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

        /// <summary>
        /// Calculates the birth date based on the marriage date and gender.
        /// </summary>
        /// <param name="marriageDate">The marriage date.</param>
        /// <param name="gender">The gender.</param>
        /// <returns>The birth date based on the marriage date and gender.</returns>
        public static DateTime BirthDateFromMarriageDate(DateTime marriageDate, Gender gender)
        {
            return GetRandomDateFromYear(marriageDate.Year - GetMarriageAge(gender));
        }
    }
}