using System.Runtime.CompilerServices;

namespace TanksServer.Interactivity;

public static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
}
