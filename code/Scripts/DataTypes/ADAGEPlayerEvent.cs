using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEPlayerEvent : ADAGEGameEvent
{
    public ADAGEPositionalContext positional_context;

    public ADAGEPlayerEvent()
        : base()
    {
        positional_context = new ADAGEPositionalContext();
    }

    public override void Update(int id)
    {
        base.Update(id);
        //positional_context = ADAGE.PositionalContext;
    }
}
