using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogHandle : MonoBehaviour
{
    public string HandleName;

    public override string ToString()
    {
        return this.GetType().ToString() + " : " + gameObject.name;
    }
    
    public int CompareHandles(LogHandle other)
    {
        int comparer = 0;
        comparer = this.GetType().ToString().CompareTo(other.GetType().ToString());
        if(comparer != 0)
        {
            return comparer;
        }

        comparer = gameObject.name.CompareTo(other.gameObject.name);
        return comparer;
    }
}