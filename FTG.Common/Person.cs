using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTG.Common.ExtensionMethods;
using FTG.Common.Enumerations;

namespace FTG.Common
{
    public class Person
    {
        public Guid Id { get; private set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Clan { get; set; } = string.Empty;
        public Gender Gender { get; set; } = Gender.Unknown;
        public DateTime BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }
        public int? DeathAge => DeathDate.HasValue ? Helpers.GetAge(BirthDate, DeathDate.Value) : null;
        public DateTime? MarriageDate { get; set; }
        public int? MarriageAge => MarriageDate.HasValue ? Helpers.GetAge(BirthDate, MarriageDate.Value) : null;
        public int? Generation { get; set; }
        public Guid? SpouseId { get; set; }
        public Guid? FatherId { get; set; }
        public Guid? MotherId { get; set; }
        public Guid? ParentNodeId { get; set; }
        public string? CountryCode { get; set; }
        public bool HasFamily { get; set; }
        public string PersonalityType { get; set; }
        public string PersonalityTypeName => Helpers.GetPersonalityTypeName(PersonalityType);

        public Person() => Id = Guid.NewGuid();
    }
}