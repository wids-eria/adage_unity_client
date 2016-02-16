using UnityEngine;
using System.Collections;

public class ADAGEPositionalContext
{
    public float x;
    public float y;
    public float z;
    //Euler angles for rotation
    public float rotx;
    public float roty;
    public float rotz;

    public ADAGEPositionalContext()
    {
        x = 0f;
        y = 0f;
        z = 0f;
        rotx = 0f;
        roty = 0f;
        rotz = 0f;
    }

    public void setPosition(float iX, float iY, float iZ)
    {
        x = iX;
        y = iY;
        z = iZ;
    }

    public void setRotation(float iX, float iY, float iZ)
    {
        rotx = iX;
        roty = iY;
        rotz = iZ;
    }
}
