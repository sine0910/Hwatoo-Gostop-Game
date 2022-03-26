using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtil
{
    public static IEnumerator RunThrowingIterator(IEnumerator enumerator)
    {
        while (true)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false)
                {
                    break;
                }
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                Debug.Log("Coroutine Error: " + ex);
                MultiPlayManager.instance.on_error();
                yield break;
            }
            yield return current;
        }
    }
}
