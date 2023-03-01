using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Hazelnut.Nokdu.Exceptions;

namespace Hazelnut.Nokdu;

public partial class NokduActor
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    private readonly struct Command
    {
        public readonly ulong Id;
        public readonly Action Method;
        public readonly DateTime ExecuteTime;

        public Command(ulong id, Action method, TimeSpan after)
        {
            Id = id;
            Method = method;
            ExecuteTime = DateTime.UtcNow + after;
        }
    }

    private ulong _commandId;
    private readonly ConcurrentQueue<Command> _commandQueue = new();
    private readonly ConcurrentQueue<ulong> _cancelQueue = new();

    public void SendMessage(string messageName) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName), TimeSpan.Zero);
    public void SendMessage<T>(string messageName, T arg) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg), TimeSpan.Zero);
    public void SendMessage<T1, T2>(string messageName, T1 arg0, T2 arg1) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3>(string messageName, T1 arg0, T2 arg1, T3 arg2) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5),
            TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5, T7 arg6) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6),
            TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5, T7 arg6, T8 arg7) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7),
            TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8),
            TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string messageName, T1 arg0, T2 arg1,
        T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string messageName, T1 arg0,
        T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string messageName, T1 arg0,
        T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13, T15 arg14) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13, arg14), TimeSpan.Zero);
    public void SendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13, T15 arg14, T16 arg15) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13, arg14, arg15), TimeSpan.Zero);

    public ulong SendMessageAfter(string messageName, TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName), after);
    public ulong SendMessageAfter<T>(string messageName, T arg, TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg), after);
    public ulong SendMessageAfter<T1, T2>(string messageName, T1 arg0, T2 arg1, TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1), after);
    public ulong SendMessageAfter<T1, T2, T3>(string messageName, T1 arg0, T2 arg1, T3 arg2, TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2), after);
    public ulong SendMessageAfter<T1, T2, T3, T4>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5, TimeSpan after) =>
        InternalSendMessage(() => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5),
            after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5, T7 arg6, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6),
            after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4,
        T6 arg5, T7 arg6, T8 arg7, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7),
            after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8),
            after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string messageName, T1 arg0, T2 arg1,
        T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string messageName, T1 arg0,
        T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12,
        TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string messageName, T1 arg0,
        T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13, T15 arg14, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13, arg14), after);
    public ulong SendMessageAfter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11,
        T13 arg12, T14 arg13, T15 arg14, T16 arg15, TimeSpan after) =>
        InternalSendMessage(
            () => FindAndInvokeInstanceSendMessage(messageName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
                arg9, arg10, arg11, arg12, arg13, arg14, arg15), after);

    public void CancelMessage(ulong messageId)
    {
        if (_commandQueue.Any(command => command.Id == messageId))
            _cancelQueue.Enqueue(messageId);
    }
    
    private ulong InternalSendMessage(Action action, TimeSpan after)
    {
        if (Thread.CurrentThread == _thread && after == TimeSpan.Zero)
        {
            action.Invoke();
            return 0;
        }
        
        var currentId = Interlocked.Increment(ref _commandId);
        _commandQueue.Enqueue(new Command(currentId, action, after));
        _thread.Interrupt();

        return currentId;
    }

    private void CheckArguments(string messageName)
    {
        if (!_cachedMethodInfos.ContainsKey(messageName))
            throw new MethodNotFoundException(messageName);
    }

    private void FindAndInvokeInstanceSendMessage(string messageName)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action) method).Invoke();
    }

    private void FindAndInvokeInstanceSendMessage<T>(string messageName, T arg0)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T>) method).Invoke(arg0);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2>(string messageName, T1 arg0, T2 arg1)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2>) method).Invoke(arg0, arg1);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3>(string messageName, T1 arg0, T2 arg1, T3 arg2)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3>) method).Invoke(arg0, arg1, arg2);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3, T4>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4>) method).Invoke(arg0, arg1, arg2, arg3);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        T5 arg4)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5>) method).Invoke(arg0, arg1, arg2, arg3, arg4);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        T5 arg4, T6 arg5)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6>) method).Invoke(arg0, arg1, arg2, arg3, arg4, arg5);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7>(string messageName, T1 arg0, T2 arg1, T3 arg2, T4 arg3,
        T5 arg4, T6 arg5, T7 arg6)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7>) method).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName].FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8>) method).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageName, T1 arg0, T2 arg1, T3 arg2,
        T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>) method).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7,
            arg8);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }

    private void FindAndInvokeInstanceSendMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string messageName,
        T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14, T16 arg15)
    {
        CheckArguments(messageName);
        var method = _cachedMethodInfos[messageName]
            .FirstOrDefault(d => d is Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>);
        if (method == null)
            throw new MethodParameterNotMatchedException(messageName);
        ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>) method)
            .Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }
}