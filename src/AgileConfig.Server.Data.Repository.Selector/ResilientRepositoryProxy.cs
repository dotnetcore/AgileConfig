using System.Reflection;
using System.Runtime.ExceptionServices;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Data.Repository.Selector;

// Keeps sync initialization and async, provider-specific repositories behind the same circuit.
public class ResilientRepositoryProxy : DispatchProxy
{
    private readonly object _gate = new();
    private Func<object> _create;
    private object _target;
    private DatabaseCircuitBreaker _circuit;
    private static readonly MethodInfo GenericTaskMethod = typeof(ResilientRepositoryProxy)
        .GetMethod(nameof(InvokeTaskAsync), BindingFlags.Instance | BindingFlags.NonPublic);

    public static object Wrap(Type contract, Func<object> create, DatabaseCircuitBreaker circuit)
    {
        var proxy = (ResilientRepositoryProxy)Create(contract, typeof(ResilientRepositoryProxy));
        proxy._create = create;
        proxy._circuit = circuit;
        return proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.Name == nameof(IDisposable.Dispose))
        {
            if (_target != null) return InvokeTarget(targetMethod, args);
            return null;
        }

        // Assigning a unit of work is not a successful database probe.
        if (targetMethod.IsSpecialName) return _circuit.CreateResource(() => InvokeTarget(targetMethod, args));
        if (targetMethod.ReturnType == typeof(Task)) return InvokeVoidTaskAsync(targetMethod, args);
        if (targetMethod.ReturnType.IsGenericType &&
            targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            return GenericTaskMethod.MakeGenericMethod(targetMethod.ReturnType.GenericTypeArguments[0])
                .Invoke(this, new object[] { targetMethod, args });

        return _circuit.Execute(() => InvokeTarget(targetMethod, args));
    }

    private Task<T> InvokeTaskAsync<T>(MethodInfo method, object[] args)
    {
        return _circuit.ExecuteAsync(() => (Task<T>)InvokeTarget(method, args));
    }

    private async Task InvokeVoidTaskAsync(MethodInfo method, object[] args)
    {
        await _circuit.ExecuteAsync(async () =>
        {
            await (Task)InvokeTarget(method, args);
            return true;
        });
    }

    private object InvokeTarget(MethodInfo method, object[] args)
    {
        try
        {
            lock (_gate)
            {
                // Unlike Lazy<T>, a transient constructor failure must not be cached forever.
                _target ??= _create();
            }
            return method.Invoke(_target, args);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
