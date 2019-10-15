using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MotionControl : Controller
{

    Thread receiveThread;
    UdpClient client;
    int port;
    Vector3 leftVec;
    Vector3 rightVec;

    public MotionControl()
    {
        port = 5065;
        InitUDP();
        leftVec = new Vector3(0f, 0f, 0f);
        rightVec = new Vector3(0f, 0f, 0f);
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
        }
    }

    void UpdateLeftRightVec(string signal)
    {
        var cords = signal.Split(',');

        var left = cords[0].Split(' ');
        var right = cords[1].Split(' ');
        try
        {
            leftVec = new Vector3(float.Parse(left[0]), float.Parse(left[1]), 0f);
            rightVec = new Vector3(float.Parse(right[0]), float.Parse(right[1]), 0f);

            float mag = (leftVec - rightVec).magnitude;
            Vector3 vec = (leftVec + rightVec) / 2;
            vec.z = mag;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            //print(e.ToString());
        }
    }

    public override Vector3 GetRotation()
    {
        Vector3 vec = (leftVec + rightVec) / 2;
        Vector3 leftRight = rightVec - leftVec;
        float angle;
        if (leftRight.magnitude == 0f)
        {
            angle = 0f;
        }
        else
        {
            angle = Vector3.SignedAngle(new Vector3(1f, 0f, 0f), leftRight, new Vector3(0f, 0f, 1f));
        }
        vec.z = angle / 360;
        return vec;
    }

    public override float GetSpeed()
    {
        return (leftVec - rightVec).magnitude;
    }
}
