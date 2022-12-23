// See https://aka.ms/new-console-template for more information

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
