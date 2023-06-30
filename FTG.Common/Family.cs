using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTG.Common
{
    public class Family
    {
        public string CountryCode { get; set; }
        public string FamilyName { get; set; }
        public List<Person> People { get; set; } = new List<Person>();

    }
}