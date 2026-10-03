using System.Diagnostics;
using UnityEngine;

public static class RuntimeLog
{
    [Conditional("SUSHI_VERBOSE_LOGS")]
    public static void Write(object message, UnityEngine.Object context = null) => UnityEngine.Debug.Log(message, context);
}
