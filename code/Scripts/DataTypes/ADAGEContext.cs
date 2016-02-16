using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEContext : ADAGEData
{
    public string name; //Should be unique
    public string parent_name; //This can be left blank if there is no parent for this unit
    public bool success;

    public ADAGEContext()
    {
        name = "";
        parent_name = "";
        success = false;
    }
}
