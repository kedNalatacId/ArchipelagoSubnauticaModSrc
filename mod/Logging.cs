using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Archipelago;
// Unity logging is documented as thread safe, but it's not.
// So this is my own wrapper around it so it doesn't just silently fail.
public static class Logging
{
    public static Thread MainThread = null;
    public static ConcurrentQueue<Tuple<string, bool>> UnityLogQueue = new ConcurrentQueue<Tuple<string, bool>>();

    public static void Initialize()
    {
        MainThread = Thread.CurrentThread;
    }

    public static void Log(string message, bool ingame = true, bool unity_log = true, bool is_error = false)
    {
        if (ingame)
        {
            ErrorMessage.AddMessage(message);
        }

        if (unity_log)
        {
            UnityLogQueue.Enqueue(new Tuple<string, bool>(message, is_error));
        }

        TryUpdateLog();
    }

    public static void LogDebug(string message)
    {
        Log(message, ingame:false, unity_log:true, is_error:false);
    }
    
    public static void LogError(string message, bool ingame = true, bool unity_log = true)
    {
        Log(message, ingame, unity_log, is_error:true);
    }
    
    public static bool TryUpdateLog()
    {
        if (Thread.CurrentThread == MainThread)
        {
            Tuple<string, bool> nextText;
            while (true)
            {
                bool success = UnityLogQueue.TryDequeue(out nextText);
                if (!success)
                {
                    return true;
                }

                if (nextText.Item2)
                {
                    Debug.LogError(nextText.Item1);
                }
                else
                {
                    Debug.Log(nextText.Item1);
                }
            }
        }
        return false;
    }
}