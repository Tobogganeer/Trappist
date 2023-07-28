using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set it to a value, and then check until it becomes 0
/// </summary>
public struct Counter
{
    float end;

    public static implicit operator float(Counter c)
    {
        return c.end - Time.time;
    }

    public static implicit operator Counter(float seconds)
    {
        return new Counter { end = Time.time + seconds };
    }

    public override string ToString()
    {
        // Implicitly convert to float, then return it
        return (+this).ToString();
    }
}

/*
public struct TimeSince
{
    float time;

    public static implicit operator float(TimeSince ts)
    {
        return Time.time - ts.time;
    }
    public static implicit operator TimeSince(float ts)
    {
        return new TimeSince { time = Time.time - ts };
    }
}
*/

/*
  -Example:

TimeSince ts; 
void Start() 
{ 
    ts = 0; 
}
void Update() 
{ 
    if ( ts > 10 ) 
    { 
        DoSomethingAfterTenSeconds(); 
    } 
}

*/
