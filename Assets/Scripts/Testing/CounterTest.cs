using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterTest : MonoBehaviour
{
    Counter counter;

    void Update()
    {
        if (PlayerInputs.Jump.WasPressedThisFrame())
            counter = 5;
        if (counter > 0)
            Debug.Log(counter);
    }
}
