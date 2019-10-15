using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : Controller
{
    float dummy;
    public MouseControl()
    {
        dummy = 1f;
    }

    public override Vector3 GetRotation()
    {
        Vector3 rotate = Input.mousePosition;
        rotate.x = (rotate.x - Screen.width / 2) / Screen.width;
        rotate.y = (rotate.y - Screen.height / 2) / Screen.height;
        rotate.z = 0f;
        rotate = rotate * 20;
        return rotate;
    }

    public override float GetSpeed()
    {
        return 1f;
    }



}
