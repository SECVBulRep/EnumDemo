﻿using System.Collections;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();
Console.WriteLine(Enumerable.Select(Enumerable.Where(source, i => i % 2 == 0), i => i * 2).Sum());
Console.WriteLine(Test.SelectCompiler(Test.WhereCompiler(source, i => i % 2 == 0), i => i * 2).Sum());
Console.WriteLine(Test.SelectManual(Test.WhereManual(source, i => i % 2 == 0), i => i * 2).Sum());


// Console.WriteLine(Test.SelectCompiler(source, i => i));
// Console.WriteLine(Test.SelectManual(source, i => i));
// Console.WriteLine(Enumerable.Select(source, i => i));
//
// Console.WriteLine(Enumerable.Select(new List<int>(), i => i));
// Console.WriteLine(Enumerable.Select(new Queue<int>(), i => i));
// Console.WriteLine(Enumerable.Select(Enumerable.Range(1, 1000), i => i));
//
//
// Console.WriteLine(source.Select(i => i % 2).Select(i => i));
// Console.WriteLine(source.Select(i => i % 2).Where(x => x > 1).Select(i => i));

// покажи сайта https://source.dot.net/  и там Iterator Linq

// IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();
// Console.WriteLine(source.Select(x => x * 2).Sum());
// Console.WriteLine(Test.SelectCompiler(source, x => x * 2).Sum());
// Console.WriteLine(Test.SelectManual(source, x => x * 2).Sum());
//
// // не реалзивали Reset %)
// var m = Test.SelectManual(source, x => x * 2);
//
// Console.WriteLine(m.Sum());
// Console.WriteLine(m.Sum());
//
//
// m = Test.SelectCompiler(source, x => x * 2);
//
// Console.WriteLine(m.Sum());
// Console.WriteLine(m.Sum());


[MemoryDiagnoser]
[ShortRunJob]
public class Test
{
    IEnumerable<int> source = Enumerable.Range(0, 1000).ToArray();


    [Benchmark]
    public int SumCompiler()
    {
        int sum = 0;

        foreach (var value in SelectCompiler(source, x => x * 2))
        {
            sum = +1;
        }

        return sum;
    }

    [Benchmark]
    public int SumManual()
    {
        int sum = 0;

        foreach (var value in SelectManual(source, x => x * 2))
        {
            sum = +1;
        }

        return sum;
    }


    [Benchmark]
    public int SumLinq()
    {
        int sum = 0;

        foreach (var value in Enumerable.Select(source, x => x * 2))
        {
            sum = +1;
        }

        return sum;
    }


    public static IEnumerable<TResult> SelectCompiler<TSource, TResult>(IEnumerable<TSource> source,
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


    public static IEnumerable<TSource> WhereCompiler<TSource>(IEnumerable<TSource> source, Func<TSource, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        return Impl(source, filter);

        static IEnumerable<TSource> Impl(IEnumerable<TSource> source, Func<TSource, bool> filter)
        {
            foreach (var i in source)
                if (filter(i))
                    yield return i;
        }
    }


    public static IEnumerable<TResult> SelectManual<TSource, TResult>(IEnumerable<TSource> source,
        Func<TSource, TResult> selector)
    {
        //Этот метод уже не итератором
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        
        if (source is WhereManualEnumerable<TSource> where)
        {
            return new WhereSelectManualEnumerable<TSource, TResult>(where._source, where._filter, selector);
        }
        
        return new SelectManualEnumerable<TSource, TResult>(source, selector);
    }

    public static IEnumerable<TSource> WhereManual<TSource>(IEnumerable<TSource> source,
        Func<TSource, bool> filter)
    {
        //Этот метод уже не итератором
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);
        return new WhereManualEnumerable<TSource>(source, filter);
    }


    internal sealed class SelectManualEnumerable<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        public TResult Current { get; private set; } = default!;
        private readonly Func<TSource, TResult> _selector;
        private readonly IEnumerable<TSource> _source;
        private IEnumerator<TSource> _enumerator;
        private int _state = 0;
        private int _threadId = Environment.CurrentManagedThreadId;


        public SelectManualEnumerable(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            // тут проблема с дотсупом с нескольких ыпотоков к переменной. Два потока могут начать обрабатывать один и тот же итератор одновременно 
            //  и обоих может быть  state = 1.
            // sпервое решение такое
            //if(Interlocked.CompareExchange(ref _state,1,0)==0)
            if (_threadId == Environment.CurrentManagedThreadId &&
                _state == 0) // подобную проверку генерит сам компилятор
            {
                _state = 1;
                return this;
            }

            return new SelectManualEnumerable<TSource, TResult>(_source, _selector) { _state = 1 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Reset()
        {
            //99.99 % енумрайторов не имеют реализации, поэтому оставляем 
            throw new NotSupportedException();
        }


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

    internal sealed class WhereManualEnumerable<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        public TSource Current { get; private set; } = default!;
        internal readonly Func<TSource, bool> _filter;
        internal readonly IEnumerable<TSource> _source;
        private IEnumerator<TSource> _enumerator;
        private int _state = 0;
        private int _threadId = Environment.CurrentManagedThreadId;


        public WhereManualEnumerable(IEnumerable<TSource> source, Func<TSource, bool> filter)
        {
            _source = source;
            _filter = filter;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            // тут проблема с дотсупом с нескольких ыпотоков к переменной. Два потока могут начать обрабатывать один и тот же итератор одновременно 
            //  и обоих может быть  state = 1.
            // sпервое решение такое
            //if(Interlocked.CompareExchange(ref _state,1,0)==0)
            if (_threadId == Environment.CurrentManagedThreadId &&
                _state == 0) // подобную проверку генерит сам компилятор
            {
                _state = 1;
                return this;
            }

            return new WhereManualEnumerable<TSource>(_source, _filter) { _state = 1 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Reset()
        {
            //99.99 % енумрайторов не имеют реализации, поэтому оставляем 
            throw new NotSupportedException();
        }


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
                        // тут нет итерации из вне. Нам нужно все сразу
                        while (_enumerator.MoveNext())
                        {
                            // оптимизация
                            TSource current = _enumerator.Current;
                            if (_filter(current))
                            {
                                Current = current;
                                return true;
                            }
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

    internal sealed class WhereSelectManualEnumerable<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        public TResult Current { get; private set; } = default!;
        private readonly Func<TSource, TResult> _selector;
        private readonly Func<TSource, bool> _filter;
        private readonly IEnumerable<TSource> _source;
        private IEnumerator<TSource> _enumerator;
        private int _state = 0;
        private int _threadId = Environment.CurrentManagedThreadId;


        public WhereSelectManualEnumerable(IEnumerable<TSource> source, Func<TSource, bool> filter,
            Func<TSource, TResult> selector)
        {
            _source = source;
            _selector = selector;
            _filter = filter;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            // тут проблема с дотсупом с нескольких ыпотоков к переменной. Два потока могут начать обрабатывать один и тот же итератор одновременно 
            //  и обоих может быть  state = 1.
            // sпервое решение такое
            //if(Interlocked.CompareExchange(ref _state,1,0)==0)
            if (_threadId == Environment.CurrentManagedThreadId &&
                _state == 0) // подобную проверку генерит сам компилятор
            {
                _state = 1;
                return this;
            }

            return new WhereSelectManualEnumerable<TSource, TResult>(_source, _filter, _selector) { _state = 1 };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Reset()
        {
            //99.99 % енумрайторов не имеют реализации, поэтому оставляем 
            throw new NotSupportedException();
        }


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
                        while (_enumerator.MoveNext())
                        {
                            TSource current = _enumerator.Current;

                            if (_filter(current))
                            {
                                Current = _selector(current);
                                return true;
                            }
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