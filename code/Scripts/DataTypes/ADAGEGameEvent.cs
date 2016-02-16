using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEGameEvent : ADAGEEventData
{
    public ADAGEVirtualContext virtual_context;

    public ADAGEGameEvent()
    {
        virtual_context = new ADAGEVirtualContext();
    }

    public override void Update(int id)
    {
        if (ADAGE.users != null && ADAGE.users.ContainsKey(id))
            virtual_context = ADAGE.users[id].vContext;
    }
}
