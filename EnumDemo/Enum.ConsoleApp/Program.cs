// See https://aka.ms/new-console-template for more information


//Enumerable.Select<int, int>(null, x => x * 2);


Console.WriteLine(0);
IEnumerable<int> e = Select<int, int>(null, x => x * 2);
Console.WriteLine(1);
IEnumerator<int> enumerator = e.GetEnumerator();
Console.WriteLine(2);
enumerator.MoveNext();

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