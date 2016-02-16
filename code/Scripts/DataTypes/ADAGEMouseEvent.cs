using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEMouseEvent : ADAGEData
{
    public Vector3 position;
    public string button;

    public ADAGEMouseEvent()
    {
        position = Vector3.zero;
        button = "";
    }

    public ADAGEMouseEvent(Vector3 position, string button)
    {
        this.position = position;
        this.button = button;
    }
}
