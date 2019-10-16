using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

public class MouseMover : MonoBehaviour
{


    Stopwatch stopwatch;
    // Start is called before the first frame update
    void Start()
    {
        stopwatch = new Stopwatch();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Point cursorPos = new Point();
            GetCursorPos(out cursorPos);
            SetCursorPos(cursorPos.X, cursorPos.Y - 1);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            stopwatch.Start();
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            stopwatch.Stop();
            print(stopwatch.ElapsedMilliseconds);
        }
    }

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);
}
