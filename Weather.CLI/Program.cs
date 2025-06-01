// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging;
using Weather.CLI.Services;

// Console.WriteLine("Hello, World!");
//
// string? haha = "wow";
//
// if (haha is not null)
// {
//     Console.WriteLine(haha);
// }
//
//
// int nice()
// {
//     return 1;
// }
//
// var niceFunc = nice;
//
// Console.WriteLine(niceFunc());
//
// var personTest = new Person("test");
//
// var animal = personTest with { firstName = "fafa" };
//
// Console.WriteLine(animal);
//
// var simpleService = new SimpleService();
// await simpleService.DoThings();
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<SimpleService>();

var simpleService = new SimpleService(logger);

for (int i = 0; i < 1000000; i++)
{
    simpleService.DoThings();
}
