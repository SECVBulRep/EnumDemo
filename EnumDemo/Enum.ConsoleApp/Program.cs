// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

IEnumerable<int> e = GetValues();
using IEnumerator<int> enumerator = e.GetEnumerator();
Console.WriteLine(enumerator);
while (enumerator.MoveNext())
{
    int i = enumerator.Current;
    Console.WriteLine(i);
}

static IEnumerable<int> GetValues()
{
    yield return 1;
    yield return 2;
    yield return 3;
}