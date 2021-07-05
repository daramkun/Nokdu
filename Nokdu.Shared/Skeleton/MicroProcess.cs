using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Nokdu.Skeleton
{
    internal enum QueueItemType
    {
        Send,
        Call,
    }

    internal readonly struct QueueItem
    {
        public readonly MethodInfo Method;
        public readonly object[] Arguments;
        public readonly QueueItemType Type;
        public readonly Thread CallReturnThread;

        public QueueItem(MethodInfo method, object[] args, QueueItemType type, Thread callReturnThread)
        {
            Method = method;
            Arguments = args;
            Type = type;
            CallReturnThread = callReturnThread;
        }

        public object DoMethodInvoke(object target)
        {
            return Method.Invoke(target, Arguments);
        }
    }

    public class MicroProcessOptions
    {
        public readonly bool KeepWorkingOnException;

        public MicroProcessOptions(bool keepWorkingOnException = true)
        {
            KeepWorkingOnException = keepWorkingOnException;
        }
    }

    public class MicroProcess
    {
        private static readonly object[] EmptyArguments = new object[0];

        private static readonly ConcurrentDictionary<Guid, MicroProcess> _processes = new();

        public static MicroProcess GetProcess(Guid processId)
        {
            if (_processes.TryGetValue(processId, out var process))
                return process;
            return null;
        }

        private readonly ConcurrentQueue<QueueItem> _messageQueue;
        private readonly ConcurrentDictionary<Thread, object> _callReturnDict;
        private readonly Thread _thread;
        private readonly MicroProcessOptions _options;
        private readonly Type _type;

        public object ProcessTarget { get; }

        public bool IsAlive => _thread.IsAlive;

        public Guid ProcessId { get; } = Guid.NewGuid();

        public MicroProcess(Type processTargetType, MicroProcessOptions options = null)
            : this(Activator.CreateInstance(processTargetType), options)
        {
        }

        public MicroProcess(object processTarget, MicroProcessOptions options = null)
        {
            if (!_processes.TryAdd(ProcessId, this))
            {
                ProcessId = Guid.NewGuid();
                if (!_processes.TryAdd(ProcessId, this))
                    throw new InvalidOperationException();
            }

            ProcessTarget = processTarget;
            _type = processTarget.GetType();

            _messageQueue = new ConcurrentQueue<QueueItem>();
            _callReturnDict = new ConcurrentDictionary<Thread, object>();

            _options = options;

            _thread = new Thread(ProcessBody);
            _thread.Start(this);
        }

        private MethodInfo GetMessage(string methodName) => _type.GetMethod(methodName);

        private MethodInfo GetMatchedMessageMethodInfo(string methodName, Type returnType = null,
            bool returnTypeDontCare = true, params Type[] args)
        {
            foreach (var method in _type.GetMethods())
            {
                if (method.Name != methodName)
                    continue;

                if (method.ReturnType != returnType && !returnTypeDontCare)
                    continue;

                var param = method.GetParameters();
                if ((args?.Length ?? 0) != (param?.Length ?? 0))
                    continue;

                if (args == null || param == null)
                    return method;

                var argsEnum = args.GetEnumerator();
                var paramEnum = param.GetEnumerator();

                var isSame = true;
                while (argsEnum.MoveNext() && paramEnum.MoveNext())
                {
                    var argumentType = argsEnum.Current as Type;
                    var parameterType = (paramEnum.Current as ParameterInfo)?.ParameterType;

                    if (!IsSameType(argumentType, parameterType))
                    {
                        isSame = false;
                        break;
                    }
                }

                if (isSame)
                    return method;
            }

            return null;
        }

        private bool IsSameType(Type argumentType, Type parameterType)
        {
            return argumentType == parameterType ||
                   argumentType.IsSubclassOf(parameterType) ||
                   argumentType.IsAssignableFrom(parameterType);
        }

        private Type[] ArgumentsToTypes(object[] args)
        {
            var types = new Type[args.Length];
            for (var i = 0; i < args.Length; ++i)
                types[i] = args[i].GetType();
            return types;
        }

        public bool IsSendableMessage(string methodName) => GetMessage(methodName) != null;

        public bool IsSendableMessage(string methodName, params Type[] args) =>
            GetMatchedMessageMethodInfo(methodName, null, true, args) != null;

        public void SendMessage(string methodName, params object[] args)
        {
            var method = GetMatchedMessageMethodInfo(methodName, null, false, ArgumentsToTypes(args));
            if (method != null)
            {
                _messageQueue.Enqueue(new QueueItem(method, args, QueueItemType.Send, null));
                _thread.Interrupt();
            }

            throw new InvalidOperationException("Method is not found.");
        }

        public bool IsCallableMessage(string methodName) => GetMessage(methodName)?.ReturnType != null;

        public bool IsCallableMessage(string methodName, Type returnType, params Type[] args) =>
            GetMatchedMessageMethodInfo(methodName, returnType, false, args) != null;

        public object CallMessage(string methodName, params object[] args)
        {
            var method = GetMatchedMessageMethodInfo(methodName, null, false, ArgumentsToTypes(args));
            if (method != null && method.ReturnType != null)
            {
                _messageQueue.Enqueue(new QueueItem(method, args, QueueItemType.Call, Thread.CurrentThread));
                _thread.Interrupt();
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    if (_callReturnDict.TryRemove(Thread.CurrentThread, out var returnValue))
                        return returnValue;
                }

                throw new InvalidOperationException("Process not return value but waiting is done.");
            }

            throw new InvalidOperationException("Method is not found or Method cannot return value.");
        }

        public void Abort()
        {
            SendMessage("Dispose");
        }

        private static void ProcessBody(object state)
        {
            var process = state as MicroProcess;

            if (process == null)
                return;

            if (process.IsSendableMessage("Initialize"))
                process.SendMessage("Initialize");

            while (true)
            {
                if (!process._messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException)
                    {
                        continue;
                    }
                }

                try
                {
                    var result = message.DoMethodInvoke(process.ProcessTarget);
                    if (message.Type == QueueItemType.Call)
                    {
                        process._callReturnDict[message.CallReturnThread] = result;
                        message.CallReturnThread.Interrupt();
                    }
                }
                catch (Exception ex)
                {
                    if (message.Type == QueueItemType.Call)
                        message.CallReturnThread.Interrupt();
                    while (process._messageQueue.TryDequeue(out var restMessage))
                        if (restMessage.Type == QueueItemType.Call)
                            restMessage.CallReturnThread.Interrupt();

                    if (process._options?.KeepWorkingOnException != true)
                    {
                        if (process.IsSendableMessage("Dispose"))
                            process.GetMatchedMessageMethodInfo("Dispose")
                                .Invoke(process.ProcessTarget, EmptyArguments);
                        throw;
                    }
                    else
                    {
                        if (process.IsSendableMessage("CatchException"))
                            process.GetMatchedMessageMethodInfo("CatchException")
                                .Invoke(process.ProcessTarget, new[] {ex});
                    }
                }

                if (message.Method.Name == "Dispose" && message.Method.GetParameters()?.Length is 0 or null)
                    break;
            }

            _processes.TryRemove(process.ProcessId, out var _);
        }
    }
}