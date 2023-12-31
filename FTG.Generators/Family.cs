using System;
using System.Linq;
using FTG.Common;
using FTG.Common.Enumerations;
using FTG.Common.ExtensionMethods;

namespace FTG.Generators
{
    public class FamilyGenerator
    {
        private bool _currentYearMode = true;
        private string _familyName = string.Empty;
        private int _startYear = 1000;
        private int _endYear = 0;
        private string _countryCode;
        private int _currentYear = 1000;
        private bool _done = false;
        private Family _family = new Family();

        public IEnumerable<Person> FamilyMembers => _family.People;

        private void GenerateKids(Person person, Person spouse)
        {
            if (_done) return;

            var endMarriage = new[] { person.DeathDate.Value, spouse.DeathDate.Value }.Min(); // Forgetting that divorce exists for the moment.
            var marriageDate = new[] { person.MarriageDate ?? spouse.MarriageDate ?? person.BirthDate.GetMarriedDate(), endMarriage }.Min();

            spouse.MarriageDate = marriageDate;
            person.MarriageDate = marriageDate;

            var yearsMarried = endMarriage.Year - person.MarriageDate.Value.Year;

            var fertilityStart = person.Gender switch
            {
                Gender.Male => spouse.MarriageAge.Value,
                Gender.Female => person.MarriageAge.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(person.Gender))
            };

            var yearsOfMarriage = 0;
            var girl = 0;
            while (yearsOfMarriage < yearsMarried && !_done)
            {
                if (Helpers.RollD(100) <= Helpers.GetFertility(fertilityStart + yearsOfMarriage))
                {
                    var partialKid = new Person()
                    {
                        LastName = _familyName,
                        BirthDate = Helpers.GetRandomDateFromYear(person.MarriageDate.Value.Year + yearsOfMarriage),
                        FatherId = person.Gender == Gender.Male ? person.Id : spouse.Id,
                        MotherId = person.Gender == Gender.Female ? person.Id : spouse.Id,
                        CountryCode = person.CountryCode
                    };

                    partialKid.FatherId = person.Gender == Gender.Male ? person.Id : spouse.Id;
                    partialKid.MotherId = person.Gender == Gender.Female ? person.Id : spouse.Id;
                    var kid = FinishPerson(partialKid);
                    if (kid.Gender == Gender.Female) girl++;
                    _family.People.Add(kid);
                    if (_currentYearMode)
                    { // If we are in "currentYear" mode we need to dive in to the kids family
                        _currentYear = kid.BirthDate.Year;
                        _done = GenerateFamily(kid) || _currentYear > _endYear;
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
                        continue;
                    }
                    yearsOfMarriage += (int)Math.Floor(Helpers.Rnd(Constants.Children.Delay.Mean, Constants.Children.Delay.StdDev));
                }
                else
                {
                    yearsOfMarriage++;
                }
            }
        }

        private bool GenerateFamily(Person newParent, bool mustMarry = false, bool mustHaveKids = false)
        {

            if (newParent.BirthDate.Year >= _endYear || _done)
            {
                return true;
            }



            newParent.MarriageDate = newParent.BirthDate.GetMarriedDate(mustMarry);
            if (newParent.MarriageDate.HasValue)
            {
                var partialSpouse = new Person()
                {
                    ParentNodeId = newParent.Id,
                    BirthDate = newParent.MarriageDate.Value.GetBirthDateFromMarriageDate(Helpers.OppositeGender(newParent.Gender)),
                    SpouseId = newParent.Id,
                    Generation = newParent.Generation,
                    CountryCode = NameGenerator.GetRandomCountryCode(),
                };
                newParent.SpouseId = partialSpouse.Id;

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
                                ParentNodeId = newParent.Id,
                                SpouseId = newParent.Id,
                                Generation = newParent.Generation,
                                CountryCode = NameGenerator.GetRandomCountryCode(),
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
            return false;

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
            _currentYearMode = false;
            PopulateLineage(startYear, startYear + 50);
        }
        public void PopulateLineage(int startYear, int endYear)
        {
            _currentYearMode = true;
            PopulateLineage(startYear, endYear, NameGenerator.GetRandomCountryCode());
        }
        public void PopulateLineage(int startYear, int endYear, string countryCode)
        {
            _currentYearMode = true;
            PopulateLineage(startYear, endYear, countryCode, NameGenerator.GenerateLastName(countryCode));
        }
        public void PopulateLineage(int startYear, int endYear, string countryCode, string familyName)
        {
            _currentYearMode = true;
            _startYear = startYear;
            _endYear = endYear;
            _countryCode = countryCode;
            _familyName = familyName;

            _family = new Family()
            {
                FamilyName = _familyName,
                CountryCode = _countryCode
            };

            // Let's create a patriarch of the family
            var partialPerson = new Person()
            {
                LastName = _familyName,
                CountryCode = _countryCode,
                Gender = Gender.Male,
                BirthDate = Helpers.GetRandomDateFromYear(startYear),
            };
            partialPerson.DeathDate = partialPerson.BirthDate.AddYears(86); // Just trying give the first couple in the line a better change to bear children
            var person = FinishPerson(partialPerson, true);
            _family.People.Add(person);

            GenerateFamily(person);

            // // We need a
            // var partialSpouse = new Person()
            // {
            //     LastName = NameGenerator.GenerateLastName(countryCode),
            //     CountryCode = countryCode,
            //     Gender = Gender.Female,
            //     MarriageDate = person.MarriageDate,
            //     BirthDate = Helpers.BirthDateFromMarriageDate(person.MarriageDate.Value, Gender.Female),
            // };
            // person.SpouseId = partialSpouse.Id;
            // partialSpouse.SpouseId = person.Id;
            // var spouse = FinishPerson(partialSpouse, true);

            // _family.People.Add(spouse);

            // UpdatePerson(person);

            // UpdatePerson(spouse);
            // GenerateKids(person, spouse);

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
                if (mAge > dAge)
                {
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
            if (p != null)
                _family.People.Remove(p);
            _family.People.Add(person);
        }


    }
}
