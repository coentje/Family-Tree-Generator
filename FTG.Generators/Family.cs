using System;
using System.Linq;
using FTG.Common;
using FTG.Common.Enumerations;
using FTG.Common.ExtensionMethods;

namespace FTG.Generators
{
    public class FamilyGenerator
    {
        private bool _currentYearMode = false;
        private string _familyName = string.Empty;
        private int _startYear = 1000;
        private int _endYear = 0;
        private string _countryCode;

        private Family _family = new Family();



        private void GenerateKids(Person person, Person spouse)
        {

            var endMarriage = new[] { person.DeathDate.Value, spouse.DeathDate.Value }.Min(); // Forgetting that divorce exists for the moment.
            var yearsMarried = (int)Math.Floor((person.MarriageDate.Value - endMarriage).TotalDays / 256.25);
            var fertilityStart = person.Gender switch
            {
                Gender.Male => spouse.MarriageAge.Value,
                Gender.Female => person.MarriageAge.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(person.Gender))
            };

            var yearsOfMarriage = 0;
            var girl = 0;
            while (yearsOfMarriage < yearsMarried)
            {
                if (Helpers.RollD(100) <= Helpers.GetFertility(fertilityStart + yearsOfMarriage))
                {
                    var partialKid = new Person()
                    {
                        LastName = _familyName,
                        BirthDate = Helpers.GetRandomDateFromYear(person.MarriageDate.Value.Year + yearsOfMarriage),
                        FatherId = person.Gender == Gender.Male ? person.Id : spouse.Id,
                        MotherId = person.Gender == Gender.Female ? person.Id : spouse.Id,
                        CountryCode =  person.CountryCode
                    };

                    partialKid.FatherId = person.Gender == Gender.Male ? person.Id : spouse.Id;
                    partialKid.MotherId = person.Gender == Gender.Female ? person.Id : spouse.Id;
                    var kid = FinishPerson(partialKid);
                    if (kid.Gender == Gender.Female) girl++;
                    _family.People.Add(kid);
                    if (_currentYearMode)
                    { // If we are in "currentYear" mode we need to dive in to the kids family
                        GenerateFamily(kid.Id);
                    }

                    // Now see if the mommy survived the birth of her child
                    if ((double)Helpers.RollD(100) < Constants.Rate.Death.ChildBirth)
                    {
                        if (person.Gender == Gender.Male)
                        {
                            spouse.DeathDate = kid.BirthDate;
                            UpdatePerson(spouse);
                        }
                        else
                        {
                            person.DeathDate = kid.BirthDate;
                            UpdatePerson(person);
                        }
                        return;
                    }
                    yearsOfMarriage += (int)Math.Floor(Helpers.Rnd(Constants.Children.Delay.Mean, Constants.Children.Delay.StdDev));
                }
                yearsOfMarriage++;
            }
        }

        private void GenerateFamily(Guid id)
        {
            var spouseCC = NameGenerator.GetRandomCountryCode();
            var newParent = _family.People.First(p => p.Id == id);
            var partialSpouse = new Person()
            {
                ParentNodeId = id,
                SpouseId = id,
                Generation = newParent.Generation,
                MarriageDate = newParent.MarriageDate,
                Gender = Gender.Male,
            };
            var spouse = FinishPerson(partialSpouse);
            _family.People.Add(spouse);
            GenerateKids(newParent, spouse);

            var grief = spouse.DeathDate.Value;

            while (newParent.DeathDate.Value >= grief)
            {
                grief = grief.AddYears(Helpers.GenerateGrief());
                if (grief <= newParent.DeathDate.Value)
                {
                    var offspring = CountKids(newParent);
                    var newChance = offspring switch
                    {
                        0 => (newParent.DeathDate.Value.Year - grief.Year) * Constants.Rate.Remarry.Barren,
                        1 => (newParent.DeathDate.Value.Year - grief.Year) * Constants.Rate.Remarry.SingleChild,
                        _ => (newParent.DeathDate.Value.Year - grief.Year) * Constants.Rate.Remarry.MultipleHeirs,
                    };
                    if (Helpers.RollD(100) < newChance)
                    {
                        partialSpouse = new Person()
                        {
                            ParentNodeId = id,
                            SpouseId = id,
                            Generation = newParent.Generation,
                            MarriageDate = grief,
                        };
                        spouse = FinishPerson(partialSpouse);
                        _family.People.Add(spouse);
                        GenerateKids(newParent, spouse);
                        grief = spouse.DeathDate.Value;
                    }
                }
            }

        }

        private int CountKids(Person person)
        {
            return person.Gender == Gender.Male
                    ? _family.People.Count(k => k.FatherId == person.Id)
                    : _family.People.Count(k => k.MotherId == person.Id);
        }

        public void PopulateLineage()
        {
            PopulateLineage(DateTime.Now.AddYears(-50).Year);
        }
        public void PopulateLineage(int startYear)
        {
            PopulateLineage(startYear, startYear + 50);
        }
        public void PopulateLineage(int startYear, int endYear)
        {
            PopulateLineage(startYear, endYear, NameGenerator.GetRandomCountryCode());
        }
        public void PopulateLineage(int startYear, int endYear, string countryCode)
        {
            PopulateLineage(startYear, endYear, countryCode, NameGenerator.GenerateLastName(countryCode));
        }
        public void PopulateLineage(int startYear, int endYear, string countryCode, string familyName)
        {
            _startYear = startYear;
            _endYear = endYear;
            _countryCode = countryCode;
            _familyName = familyName;

            var partialPerson = new Person()
            {
                LastName = familyName,
                CountryCode = countryCode,
                Gender = Gender.Male,
                BirthDate = Helpers.GetRandomDateFromYear(startYear),
            };
            var person = FinishPerson(partialPerson);
            _family.People.Add(person);

            var partialSpouse = new Person()
            {
                LastName = NameGenerator.GenerateLastName(countryCode),
                CountryCode = countryCode,
                Gender = Gender.Female,
                MarriageDate = person.MarriageDate,
                BirthDate = Helpers.BirthDateFromMarriageDate(person.MarriageDate.Value, Gender.Female),
            };
            
            var spouse = FinishPerson(partialSpouse);
            _family.People.Add(spouse);
            person.SpouseId = spouse.Id;
            UpdatePerson(person);
            spouse.SpouseId = person.Id;
            UpdatePerson(spouse);
            GenerateKids(person, spouse);

        }

        private Person FinishPerson(Person person, bool mustLive = false)
        {
            Person? spouse = person.SpouseId.HasValue ? _family.People.FirstOrDefault(s => s.Id == person.SpouseId.Value) : null;
            Person? father = person.FatherId.HasValue ? _family.People.FirstOrDefault(s => s.Id == person.FatherId.Value) : null;
            Person? mother = person.MotherId.HasValue ? _family.People.FirstOrDefault(s => s.Id == person.MotherId.Value) : null;

            if (person.Gender == Gender.Unknown)
            {
                if (spouse != null && spouse.Gender != Gender.Unknown)
                {
                    person.Gender = Helpers.OppositeGender(spouse.Gender);
                }
                person.Gender = Helpers.RandomGender();
            }

            if (father != null)
            {
                person.Clan = father.Clan;
                person.Generation = father.Generation + 1;
            }
            else
            {
                person.Clan = _familyName;
                if (spouse != null && spouse.Gender == Gender.Male)
                {
                    person.Generation = spouse.Generation;
                }
                else
                {
                    person.Generation = 0;
                }
            }

            if (string.IsNullOrWhiteSpace(person.FirstName))
            {
                person.FirstName = NameGenerator.GenerateFirstName(person.Gender, person.CountryCode);
            }

            if (spouse != null && (spouse.MarriageDate.HasValue || person.MarriageDate.HasValue))
            {
                if (person.MarriageDate.HasValue)
                {
                    spouse.MarriageDate = person.MarriageDate;
                }
                else
                {
                    person.MarriageDate = spouse.MarriageDate;
                }
            }
            else
            {
                var mAge = Helpers.GetMarriageAge(person.Gender);

                person.MarriageDate = Helpers.GetRandomDateFromYear(person.BirthDate.Year + mAge);
            }
            if (!person.DeathDate.HasValue)
            {
                var mAge = person.MarriageAge.Value;
                var dAge = Helpers.GetDeathAge(person.Gender);
                if(mAge > dAge) {
                    dAge = mAge;
                }
                person.DeathDate = Helpers.GetRandomDateFromYear(person.BirthDate.Year + dAge);
            }

            if (father != null)
            {
                if ((person.MarriageDate.Value > person.DeathDate.Value) || (Helpers.RollD(100) <= Constants.Rate.Bachelor_ette))
                {
                    person.HasFamily = false;
                }
                else if (person.Gender == Gender.Male)
                {
                    person.HasFamily = Helpers.RollD(100) <= (100 - Constants.Rate.Birth.Male);
                }
                if (!person.HasFamily)
                {
                    person.MarriageDate = null;
                    person.SpouseId = null;
                    spouse = null;
                }
            }

            person.PersonalityType = Helpers.GeneratePersonalityType();

            return person;
        }

        private void UpdatePerson(Person person)
        {
            var p = _family.People.FirstOrDefault(p => p.Id == person.Id);
            if(p != null)
                _family.People.Remove(p);
            _family.People.Add(person);
        }


    }
}
