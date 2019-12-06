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
using Debug = UnityEngine.Debug;

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
    Queue<double> delays = new Queue<double>(30);
    bool destroyed = false;
    bool portDestroyed = false;


    public MotionControl()
    {
        port = GameManager.port;
        client = GameManager.client;
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
        //client = new UdpClient(port);
        while (!destroyed)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
 
                UpdateLeftRightVec(text);
                var delay = GetDelay(text);
                delays.Enqueue(delay);
                Debug.Log("delay in ms: " + 1000 * QueueAverage(delays));
            }

            catch (Exception e)
            {
                Debug.Log(e.ToString());
                //print(e.ToString());
            }

            if (InMenu)
            {
                UpdateMouse();
            }
        }
        client.Close();
        portDestroyed = true;
        Debug.Log("successfully destroyed the old predefined motion controller");
    }

    double QueueAverage(Queue<double> q)
    {
        double sum = 0;
        int count = 0;
        IEnumerator<double> enumerator = q.GetEnumerator();
        while (enumerator.MoveNext())
        {
            sum += enumerator.Current;
            count += 1;
        }
        return sum / count;
    }

    double GetDelay(String signal)
    {
        var startTime = double.Parse(signal.Split('_')[1]);
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        double secondsSinceEpoch = t.TotalSeconds;
        var delay = secondsSinceEpoch - startTime;
        return delay;
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
        var pieces = signal.Split('_');
        var cords = pieces[0].Split(',');

        var left = cords[0].Split(' ');
        var right = cords[1].Split(' ');
        try
        {
            if (right[0].Equals(noVal))
            {
                if (left[0].Equals(noVal)) // no value at all
                {
                    rightVec = rightCenter;
                    leftVec = leftCenter;
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

    public override void Destroy()
    {
        destroyed = true;   
    }

    public override bool GetPortDestroyed()
    {
        return portDestroyed;
    }
}
