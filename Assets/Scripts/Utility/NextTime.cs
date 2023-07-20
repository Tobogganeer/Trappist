using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NextTime
{
    float time;

    public static implicit operator float(NextTime ts)
    {
        return Time.time - ts.time;
    }
    public static implicit operator NextTime(float ts)
    {
        return new NextTime { time = Time.time - ts };
    }
}

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
