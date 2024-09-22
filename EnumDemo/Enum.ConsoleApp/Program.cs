// See https://aka.ms/new-console-template for more information

IEnumerable<int> source = Enumerable.Range(1, 10);

foreach (var number in Select(source, x => x * 2))
{
    Console.WriteLine(number);
}


static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
{
    foreach (var i in source)
    {
        yield return selector(i);
    }
}