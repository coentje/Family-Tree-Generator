using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTG.Common
{
    public static class Constants
    {
        public static class Rate
        {
            public static class Birth
            {
                public const int Male = 51;
            }
            public static class Remarry
            {
                public const int Barren = 15;
                public const int SingleChild = 5;
                public const int MultipleHeirs = 3;
            }
            public static class Death
            {
                public const double ChildBirth = 2.5;
                public const double ChildHood = 35;
                public const double Accident = 0.05;
            }
            public const int Bachelor_ette = 4;

        }
        public static class Age
        {
            public static class Female
            {
                public const int Mean = 78;
                public const int StdDev = 15;
            }
            public static class Male
            {
                public const int Mean = 72;
                public const int StdDev = 15;
            }
            public static class Marriage
            {
                public static class Female
                {
                    public const int Min = 12;
                    public const int Mean = 23;
                    public const int StdDev = 2;
                }
                public static class Male
                {
                    public const int Min = 17;
                    public const int Mean = 27;
                    public const int StdDev = 4;
                }
            }
        }
        public static class Children
        {
            public class Delay
            {
                public const double Mean = 1.5;
                public const double StdDev = 0.25;
            }

        }

    }
}