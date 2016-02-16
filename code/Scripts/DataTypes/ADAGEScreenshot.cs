using UnityEngine;
using System.Collections;

[ADAGE.BaseClass]
public class ADAGEScreenshot : ADAGEData
{
    public string cameraName;
    public byte[] shot;

    public ADAGEScreenshot()
    {
        cameraName = "";
    }

    public ADAGEScreenshot(string source)
    {
        cameraName = source;
    }
}
