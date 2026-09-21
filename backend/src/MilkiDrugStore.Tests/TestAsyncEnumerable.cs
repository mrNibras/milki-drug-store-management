using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace MilkiDrugStore.Tests;

/// <summary>
/// Wraps a synchronous queryable to provide async enumeration support,
/// enabling mocked IQueryable&lt;T&gt; instances to be used with EF Core
/// async extension methods like ToListAsync().
/// </summary>
public class TestAsyncEnumerable<T> : IQueryable<T>, IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncEnumerable(IEnumerable<T> inner)
    {
        _inner = inner.AsQueryable();
    }

    public TestAsyncEnumerable(IQueryable<T> inner)
    {
        _inner = inner;
    }

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
}

public class TestAsyncQueryProvider<T> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var queryable = _inner.CreateQuery<TElement>(expression);
        return new TestAsyncEnumerable<TElement>(queryable);
    }

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => Execute<TResult>(expression);

    public IAsyncEnumerable<TResult> ExecuteAsyncEnumerable<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var result = Execute<IEnumerable<TResult>>(expression);
        return new TestAsyncEnumerable<TResult>(result);
    }
}
