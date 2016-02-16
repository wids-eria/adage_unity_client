using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEMenuButtonClick : ADAGEData
{
    public string buttonName;

    public ADAGEMenuButtonClick(string name)
        : base()
    {
        buttonName = name;
    }
}
