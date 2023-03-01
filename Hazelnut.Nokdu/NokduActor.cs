using System.Collections.Concurrent;
using System.Reflection;

namespace Hazelnut.Nokdu;

[Serializable]
public abstract partial class NokduActor : PreImplementedDispose
{
    private static readonly ConcurrentDictionary<long, NokduActor> ActorCache = new();

    public static NokduActor? GetActorById(long id) => ActorCache.TryGetValue(id, out var state) ? state : null;
    public static bool TryGetActorById(long id, out NokduActor? output) => ActorCache.TryGetValue(id, out output);
    public static IEnumerable<long> ActorIds => ActorCache.Keys;

    private readonly IReadOnlyDictionary<string, Delegate[]> _cachedMethodInfos = new ConcurrentDictionary<string, Delegate[]>();

    private readonly Thread _thread;

    public long ActorId => _thread.ManagedThreadId;
    public bool IsAlive { get; private set; }

    protected NokduActor(object? arguments = null)
    {
        InitializeWithReflection();
        
        IsAlive = true;
        
        _thread = new Thread(ActorLogic);
        _thread.Start(new Tuple<NokduActor, object?>(this, arguments));
    }

    protected override void Dispose(bool disposing)
    {
        IsAlive = false;

        if (!_thread.IsAlive)
            return;
        
        _thread.Interrupt();
        _thread.Join();
    }

    private void InitializeWithReflection()
    {
        if (_cachedMethodInfos is not ConcurrentDictionary<string, Delegate[]> cachedMethodInfos)
            throw new NullReferenceException("Method Delegate Cache object type is not valid.");
        
        GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(methodInfo => methodInfo.GetParameters().Length <= 16)
            .Where(methodInfo => methodInfo.ReturnType == typeof(void))
            .Select(methodInfo =>
            {
                var parameters = methodInfo.GetParameters();

                var delegateType = parameters.Length switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>),
                    2 => typeof(Action<,>),
                    3 => typeof(Action<,,>),
                    4 => typeof(Action<,,,>),
                    5 => typeof(Action<,,,,>),
                    6 => typeof(Action<,,,,,>),
                    7 => typeof(Action<,,,,,,>),
                    8 => typeof(Action<,,,,,,,>),
                    9 => typeof(Action<,,,,,,,,>),
                    10 => typeof(Action<,,,,,,,,,>),
                    11 => typeof(Action<,,,,,,,,,,>),
                    12 => typeof(Action<,,,,,,,,,,,>),
                    13 => typeof(Action<,,,,,,,,,,,,>),
                    14 => typeof(Action<,,,,,,,,,,,,,>),
                    15 => typeof(Action<,,,,,,,,,,,,,,>),
                    16 => typeof(Action<,,,,,,,,,,,,,,,>),
                    _ => throw new InvalidOperationException()
                };

                delegateType =
                    delegateType != typeof(Action)
                        ? delegateType.MakeGenericType(
                            parameters
                                .Select(parameterInfo => parameterInfo.ParameterType)
                                .ToArray())
                        : delegateType;

                return new Tuple<string, Delegate>(methodInfo.Name, methodInfo.CreateDelegate(delegateType, this));
            })
            .ToLookup(pair => pair.Item1)
            .Select(lookup =>
                new KeyValuePair<string, Delegate[]>(
                    lookup.Key,
                    lookup
                        .Select(pair => pair.Item2)
                        .ToArray()
                )
            )
            .ForEach(kv => cachedMethodInfos.TryAdd(kv.Key, kv.Value));
    }

    protected abstract void OnInitialize(object? arguments);
    protected abstract void OnDestroy();

    protected virtual bool OnCatchException(Exception ex) => false;

    private static void ActorLogic(object? state)
    {
        if (state is not Tuple<NokduActor, object?> tupleState)
            throw new ArgumentException("state is not valid data.", nameof(state));

        var (actorSelf, arguments) = tupleState;
        ActorCache.TryAdd(actorSelf.ActorId, actorSelf);

        Thread.CurrentThread.Name = $"Nokdu.Actor.{actorSelf.GetType().Name}<#{Environment.CurrentManagedThreadId}>";

        do
        {
            actorSelf.IsAlive = true;

            try
            {
                ActorLogicInternal(actorSelf, arguments);
                // Actor Logic Terminated
                break;
            }
            catch (Exception ex)
            {
                if (!actorSelf.OnCatchException(ex))
                    // Exception Proceed and Logic Terminated
                    break;
                // Exception Proceed and Reinitialize Actor Logic
                actorSelf.OnDestroy();
            }
        } while (true);

        actorSelf.OnDestroy();
        actorSelf.IsAlive = false;

        ActorCache.TryRemove(Environment.CurrentManagedThreadId, out _);
    }

    private static void ActorLogicInternal(NokduActor actorSelf, object? arguments)
    {
        actorSelf.OnInitialize(arguments);

        while (actorSelf.IsAlive)
        {
            while (actorSelf._commandQueue.Any(c => c.ExecuteTime <= DateTime.UtcNow) &&
                   actorSelf._commandQueue.TryDequeue(out var command))
            {
                if (!actorSelf._cancelQueue.IsEmpty &&
                    actorSelf._cancelQueue.Any(id => id == command.Id))
                    continue;
                
                if (command.ExecuteTime > DateTime.UtcNow)
                {
                    actorSelf._commandQueue.Enqueue(command);
                    continue;
                }

                command.Method.Invoke();
            }

            actorSelf._cancelQueue.Clear();

            var sleepTime = actorSelf._commandQueue.IsEmpty
                ? Timeout.InfiniteTimeSpan
                : actorSelf._commandQueue
                    .Select(command => command.ExecuteTime - DateTime.UtcNow)
                    .MinBy(executeTime => executeTime);

            if (sleepTime < TimeSpan.Zero)
                continue;

            try
            {
                Thread.Sleep(sleepTime);
            }
            catch (ThreadInterruptedException)
            {
                // Ignore                        
            }
        }
    }
}