using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

public class MotionControl : ControllerQuang
{

    Thread receiveThread;
    UdpClient client;
    int port;
    Vector3 leftVec;
    Vector3 rightVec;
    int EllapsedTime;
    Stopwatch stopwatch;
    String noVal = "x";
    Vector3 leftCenter;
    Vector3 rightCenter;

    public MotionControl()
    {
        port = 5065;
        InitUDP();
        EllapsedTime = 0;
        stopwatch = new Stopwatch();
        leftCenter = new Vector3(-0.25f, 0f, 0f);
        rightCenter = new Vector3(0.25f, 0f, 0f);
        leftVec = leftCenter;
        rightVec = rightCenter;
    }

    // 3. InitUDP
    private void InitUDP()
    {
        Console.WriteLine("UDP Initialized");
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // 4. Receive Data
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
 
                UpdateLeftRightVec(text);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //print(e.ToString());
            }

            if (InMenu)
            {
                UpdateMouse();
            }
        }

    }

    void UpdateMouse()
    {
        Vector3 vec = (leftVec + rightVec) / 2 * 30f;
        Point cursorPos = new Point();
        GetCursorPos(out cursorPos);
        SetCursorPos(cursorPos.X + (int) Math.Ceiling(vec.x), cursorPos.Y - (int) Math.Ceiling(vec.y));
    }

    void UpdateLeftRightVec(string signal)
    {
        var cords = signal.Split(',');

        var left = cords[0].Split(' ');
        var right = cords[1].Split(' ');
        try
        {
            if (right[0].Equals(noVal))
            {
                if (left[0].Equals(noVal)) // no value at all
                {
                    return;
                }
                else // 1 value
                {
                    rightVec = new Vector3(float.Parse(left[0]), float.Parse(left[1]), 0f);
                    leftVec = leftCenter;
                }
            }
            else // 2 values
            {
                leftVec = new Vector3(float.Parse(left[0]), float.Parse(left[1]), 0f);
                rightVec = new Vector3(float.Parse(right[0]), float.Parse(right[1]), 0f);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            //print(e.ToString());
        }
    }

    public override Vector3 GetRotation()
    {
        Vector3 vec = rightVec - rightCenter;
        vec.z = - (leftVec - leftCenter).x;
        return vec*30f;
    }

    public override float GetSpeed()
    {
        Vector3 vec = leftVec - leftCenter;
        return vec.y + 0.5f;
    }

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);
}
