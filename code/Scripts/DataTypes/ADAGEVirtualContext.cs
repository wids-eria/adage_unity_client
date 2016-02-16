using UnityEngine;
using System.Collections.Generic;

public class ADAGEVirtualContext
{
    public string level;  //The name of the level(map, scene, stage, etc)
    public List<string> active_units; //Names of all the currently active game units. This can be used as a flat list of tags for processing actions within the scope of the units

    public ADAGEVirtualContext()
    {
        active_units = new List<string>();
        level = "";
    }

    public ADAGEVirtualContext(string curLevel)
    {
        active_units = new List<string>();
        level = curLevel;
    }

    public bool Add(string id)
    {
        if (IsTracking(id))
        {
            return false;
        }

        active_units.Add(id);

        return true;
    }

    public bool Remove(string id)
    {
        if (!IsTracking(id))
        {
            return false;
        }

        active_units.Remove(id);

        return true;
    }

    public bool IsTracking(string id)
    {
        if (active_units == null)
            active_units = new List<string>();

        return active_units.Contains(id);
    }

    public void Clear()
    {
        //This will clear the active units list. Be careful with calling this.
        active_units.Clear();
    }
}
