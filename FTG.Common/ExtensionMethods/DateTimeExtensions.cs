using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTG.Common.Enumerations;

namespace FTG.Common.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        private readonly static Random _random = new Random();
        private readonly static Dictionary<int, double> chanceToDie = new Dictionary<int, double>() { { 0, 0.02 }, { 1, 0.01 }, { 2, 0.005 }, { 5, 0.004 }, { 10, 0.003 }, { 16, 0.002 }, { 21, 0.005 }, { 26, 0.002 }, { 32, 0.001 }, { 42, 0.003 }, { 55, 0.008 }, { 65, 0.01 }, { 75, 0.02 }, { 80, 0.03 }, { 85, 0.05 }, { 90, 0.1 }, { 95, 0.2 }, { 100, 0.3 }, { 105, 0.5 }, { 110, 0.8 }, { 125, 1.0 } };

        public static DateTime GetRandomDateFromYear(this DateTime dtm, int year)
        {
            var p = new DateTime(year + 1, 1, 1) - new DateTime(year, 1, 1);
            var days = _random.Next(1, p.Days);
            return new DateTime(year + 1, 1, 1).AddDays(days);
        }

        public static DateTime GetDeathDate(this DateTime dtm)
        {
            var chanceToDie = new Dictionary<int, double>() { { 0, 0.02 }, { 1, 0.01 }, { 2, 0.005 }, { 5, 0.004 }, { 10, 0.003 }, { 16, 0.002 }, { 21, 0.005 }, { 26, 0.002 }, { 32, 0.001 }, { 42, 0.003 }, { 55, 0.008 }, { 65, 0.01 }, { 75, 0.02 }, { 80, 0.03 }, { 85, 0.05 }, { 90, 0.1 }, { 95, 0.2 }, { 100, 0.3 }, { 105, 0.5 }, { 110, 0.8 }, { 125, 1000.0 } };
            var dt = GetDate(dtm, chanceToDie, die:true);
            return dt.Value;
        }

        public static DateTime? GetMarriedDate(this DateTime dtm, bool mustMarry = false)
        {
            var chanceToMarry = new Dictionary<int, double>() { { 0, 0.0 }, { 12, 0.002 }, { 16, 0.003 }, { 17, 0.004 }, { 18, 0.005 }, { 19, 0.05 }, { 25, 0.05 }, { 35, 0.04 }, { 45, 0.02 }, { 55, 0.01 }, { 65, 0.005 }, { 85, 0.004 }, { 90, 0.001 }, { 95, 0.0005 }, { 100, 0.0 } };
            var dt = GetDate(dtm, chanceToMarry);
            if(mustMarry && !dt.HasValue) {
                return dtm.GetRandomDateFromYear(dtm.Year+_random.Next(16,25));
            } else {
                return dt;
            }
        }

        public static DateTime GetBirthDateFromMarriageDate(this DateTime dtm, Gender gender)
        {
            var mAge = Helpers.GetMarriageAge(gender);
            var yearMarried = dtm.Year - mAge;
            return dtm.GetRandomDateFromYear(yearMarried);
        }

        internal static DateTime? GetDate(DateTime dtm, IDictionary<int, double> thresholds, int maxAge = 125, bool die = false)
        {
            var done = false;
            var age = 0;
            var currentYear = dtm.Year;
            var newDate = new DateTime(1900, 1, 1);
            var sortedThresholds = new SortedDictionary<int,double>();
            foreach(var kvp in thresholds) sortedThresholds.Add(kvp.Key, kvp.Value);

            while (!done)
            {
                
                // var chance = thresholds.Where(t => t.Key <= age).OrderByDescending(t => t.Key).First();
                var keys = new List<int>(thresholds.Keys);
                var index = keys.BinarySearch(age);
                if(index<0) {
                    index = index * -1 -2;
                }
                
                var chance = thresholds[keys[index]];
                var luck = _random.NextDouble();

                done = chance > luck || maxAge <= age || currentYear >= 9500;

                if (done)
                {
                    // var b = (int)Math.Floor(age / 10.0);
                    var p = new DateTime(currentYear + 1, 1, 1) - new DateTime(currentYear, 1, 1);
                    var days = _random.Next(1, p.Days);
                    newDate = new DateTime(currentYear + 1, 1, 1).AddDays(days);
                }
                else
                {
                    currentYear++;
                    age++;
                }
                if(age >= maxAge) {
                    if(die) {
                        done = true;
                    } else {
                        return null;
                    }
                }
            }
            return newDate < dtm ? dtm : newDate;
        }
    }
}