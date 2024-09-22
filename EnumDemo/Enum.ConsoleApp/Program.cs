IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();

Console.WriteLine(Enumerable.Select(source, x => x*2).Sum());
Console.WriteLine(Select(source, x => x*2).Sum());


static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
{
    //Этот метод уже не итератором
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(selector);

    return Impl(source, selector);
    
    //а вот этот уже итератор
    static IEnumerable<TResult> Impl<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        foreach (var i in source)
        {
            yield return selector(i);
        }
    }
}