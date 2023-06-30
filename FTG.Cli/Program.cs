// See https://aka.ms/new-console-template for more information
using System.Linq;
using FTG.Common;
using FTG.Generators;

Console.WriteLine("Hello, World!");
var fg = new FamilyGenerator();
fg.PopulateLineage(1000, 2023);