using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEErrorEvent : ADAGEPlayerEvent
{
    public string message;

    public ADAGEErrorEvent()
    {
        this.message = "";
    }

    public ADAGEErrorEvent(string message)
    {
        this.message = message;
    }
}
