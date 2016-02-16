using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEStartSession : ADAGEData
{
    public ADAGEDeviceInfo deviceInfo;

    public ADAGEStartSession()
    {
        deviceInfo = new ADAGEDeviceInfo();
    }
}
