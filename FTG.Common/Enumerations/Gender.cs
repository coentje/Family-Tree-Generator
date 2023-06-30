using System;
using System.Security.AccessControl;
namespace FTG.Common.Enumerations
{
    
    public enum Gender
    {
        Male, Female, Unknown
    }

    public static class GenderExtensions 
    {
        private static readonly Random _random = new Random();
        public static Gender GetRandomGender(this Gender gender) => (_random.Next(0,1) == 0) ? Gender.Male : Gender.Female;
        public static Gender GetOppositeGender(this Gender gender) => (gender == Gender.Male ? Gender.Female : Gender.Male);
    }
}