using System.Collections;

IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();

Console.WriteLine(source.Select(x => x * 2).Sum());
Console.WriteLine(SelectCompiler(source, x => x * 2).Sum());
Console.WriteLine(SelectManual(source, x => x * 2).Sum());

static IEnumerable<TResult> SelectCompiler<TSource, TResult>(IEnumerable<TSource> source,
    Func<TSource, TResult> selector)
{
    //Этот метод уже не итератором
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(selector);

    return Impl(source, selector);

    //а вот этот уже итератор
    static IEnumerable<TResult> Impl<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        foreach (var i in source) yield return selector(i);
    }
}

static IEnumerable<TResult> SelectManual<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
{
    //Этот метод уже не итератором
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(selector);

    return new SelectManualEnumerable<TSource, TResult>(source, selector);
}


internal sealed class SelectManualEnumerable<TSource, TResult> : IEnumerable<TResult>
{
    private readonly Func<TSource, TResult> _selector;
    private readonly IEnumerable<TSource> _source;

    public SelectManualEnumerable(IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        _source = source;
        _selector = selector;
    }

    public IEnumerator<TResult> GetEnumerator()
    {
        return new Enumerator(_source, _selector);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<TResult>
    {
        private readonly Func<TSource, TResult> _selector;
        private readonly IEnumerable<TSource> _source;
        private IEnumerator<TSource> _enumerator;
        private int _state = 1;

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

        public TResult Current { get; private set; } = default!;

        //нас не дженерик не интресует, поэтому приравниваем к дженерик 
        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            _state = -1;
            _enumerator?.Dispose();
            //пока не будем реализовывать
        }

        public bool MoveNext()
        {
            switch (_state)
            {
                case 1:
                    _enumerator = _source.GetEnumerator();
                    _state = 2;
                    goto case 2;
                case 2:
                    try
                    {
                        if (_enumerator.MoveNext())
                        {
                            Current = _selector(_enumerator.Current);
                            return true;
                            //yield return _selector(enumerator.Current);
                        }
                    }
                    catch
                    {
                        // _enumerator?.Dispose(); тут уже не нужен final
                        Dispose();
                        throw;
                    }
                    break;
            }
            
            Dispose();
            return false;
        }
    }
}