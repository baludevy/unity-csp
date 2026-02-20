using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour {
    private static readonly List<ScheduledAction> executeOnMainThread = new();
    private static readonly List<ScheduledAction> executeCopiedOnMainThread = new();
    private static bool actionToExecuteOnMainThread;

    private void Update() {
        UpdateMain();
    }

    public static void ExecuteOnMainThread(Action action) {
        if (action == null) {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        ExecuteOnMainThread(o => action(), null);
    }

    public static void ExecuteOnMainThread(Action<object> action, object state) {
        if (action == null) {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread) {
            executeOnMainThread.Add(new ScheduledAction(action, state));
            actionToExecuteOnMainThread = true;
        }
    }

    public static void UpdateMain() {
        if (actionToExecuteOnMainThread) {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread) {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (var i = 0; i < executeCopiedOnMainThread.Count; i++)
                executeCopiedOnMainThread[i].action(executeCopiedOnMainThread[i].state);
        }
    }

    private struct ScheduledAction {
        public readonly Action<object> action;
        public readonly object state;

        public ScheduledAction(Action<object> action, object state) {
            this.action = action;
            this.state = state;
        }
    }
}