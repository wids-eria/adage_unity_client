using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ADAGE.BaseClass]
public class ADAGEKeyboardEvent : ADAGEData
{
    public List<int> ASCII;

    public ADAGEKeyboardEvent()
    {
        ASCII = new List<int>();
    }

    public ADAGEKeyboardEvent(int[] codes)
    {
        ASCII = new List<int>(codes);
    }

    public void AddCode(int code)
    {
        ASCII.Add(code);
    }
}
