using System.Collections;
using System.Reflection.Metadata;

IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();

Console.WriteLine(Enumerable.Select(source, x => x*2).Sum());
Console.WriteLine(SelectCompiler(source, x => x*2).Sum());


static IEnumerable<TResult> SelectCompiler<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
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

static IEnumerable<TResult> SelectManual<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
{
    //Этот метод уже не итератором
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(selector);

    return new SelectManualEnumerable<TSource, TResult>(source, selector);

}


sealed class SelectManualEnumerable<TSource, TResult> : IEnumerable<TResult>
{
    private readonly IEnumerable<TSource> _source;
    private readonly Func<TSource, TResult> _selector;

    public SelectManualEnumerable(IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        _source = source;
        _selector = selector;
        
    }

    public IEnumerator<TResult> GetEnumerator()
    {
        return new Enumerator(_source,_selector);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    private sealed class Enumerator : IEnumerator<TResult>
    {
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, TResult> _selector;
        private  TResult _current =default!;

        public Enumerator(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

       
        public void Reset()
        {
            //99.99 % енумрайторов не имеют реализации, поэтому оставляем 
            throw new NotSupportedException();
        }

        public TResult Current => _current;

        //нас не дженерик не интресует, поэтому приравниваем к дженерик 
        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            //пока не будем реализовывать
        }
        
        public bool MoveNext()
        {
            IEnumerator<TSource> enumerator = _source.GetEnumerator();

            try
            {
                if (enumerator.MoveNext())
                {
                    _current = _selector(enumerator.Current);
                    return true;
                    //yield return _selector(enumerator.Current);
                }
            }
            finally
            {
                enumerator?.Dispose();
            }

            return false;
        }

    }
}

