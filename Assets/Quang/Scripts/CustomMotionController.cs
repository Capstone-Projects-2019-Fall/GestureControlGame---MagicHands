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
using UnityEngine;
using Debug = UnityEngine.Debug;


public class CustomMotionController : ControllerQuang
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
    Vector3 rotation;
    float speed;
    Queue<double> delays = new Queue<double>(30);
    bool destroyed = false;
    bool portDestroyed = false;

    public CustomMotionController()
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
        rotation = new Vector3(0f, 0f, 0f);
        speed = 0.5f;
        Debug.Log("init custom motion controller");
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
                Debug.Log("signal: " + text);
                UpdateControl(text);
                var delay = GetDelay(text);
                delays.Enqueue(delay);
                Debug.Log("delay in ms: " + 1000 * QueueAverage(delays));
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
        client.Close();
        portDestroyed = true;
        Debug.Log("successfully destroyed the old custom motion controller");
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
        SetCursorPos(cursorPos.X + (int)Math.Ceiling(vec.x), cursorPos.Y - (int)Math.Ceiling(vec.y));
    }

    void UpdateControl(string signal)
    {
        var pieces = signal.Split('_');
        Debug.Log(signal);
        // the signal is never empty
        var controlVec = pieces[0].Split(' ');
        rotation.x = float.Parse(controlVec[0]);
        rotation.y = float.Parse(controlVec[1]);
        rotation.z = -float.Parse(controlVec[2]);
        speed = float.Parse(controlVec[3]);
    }

    public override Vector3 GetRotation()
    {
        return rotation * 30f;
    }

    public override float GetSpeed()
    {
        float realSpeed = speed + 0.5f;
        return realSpeed;
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