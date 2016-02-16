using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEMenuClick : ADAGEData
{
    public Vector2 screenPosition;
    public string mouseButton;

    public ADAGEMenuClick(string button, Vector2 position)
        : base()
    {
        mouseButton = button;
        screenPosition = position;
    }
}