using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerQuang
{
    public abstract Vector3 GetRotation();
    public abstract float GetSpeed();
    public abstract void Destroy();
    public abstract bool GetPortDestroyed();
    public bool InMenu;

}
