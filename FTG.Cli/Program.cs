// See https://aka.ms/new-console-template for more information
using FTG.Generators;

Console.WriteLine("Hello, World!");
var fg = new FamilyGenerator();
while(fg.FamilyMembers.Count()<3) {
    fg.PopulateLineage(1000, 2023);
}
Console.WriteLine(fg.FamilyMembers);