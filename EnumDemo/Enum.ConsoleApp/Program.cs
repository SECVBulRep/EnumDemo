// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
IEnumerable<Person> people = new List<Person>
{
    new() { Name = "John" }, new() { Name = "Jane" }
};

var names = people.Select(x => x.Name);

internal class Person
{
    public string Name { get; set; }
}


static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source)
{
}