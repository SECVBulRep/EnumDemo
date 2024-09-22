// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

foreach (var i in GetValues())
{
    Console.WriteLine(i);
}

static IEnumerable<int> GetValues()
{
    yield return 1;
    yield return 2;
    yield return 3;
}