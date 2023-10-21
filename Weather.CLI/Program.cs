// See https://aka.ms/new-console-template for more information

using Weather.CLI.Model;
using Weather.CLI.Services;

Console.WriteLine("Hello, World!");

string? haha = "wow";

if (haha is not null)
{
    Console.WriteLine(haha);
}


int nice()
{
    return 1;
}

var niceFunc = nice;

Console.WriteLine(niceFunc());

var personTest = new Person("test");

var animal = personTest with { firstName = "fafa" };

Console.WriteLine(animal);

var simpleService = new SimpleService();
await simpleService.DoThings();
