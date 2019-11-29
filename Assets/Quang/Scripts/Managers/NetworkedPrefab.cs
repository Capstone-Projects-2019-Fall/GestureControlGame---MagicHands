using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkedPrefab
{
    public GameObject Prefab;
    public string Path;

    public NetworkedPrefab(GameObject obj, string path)
    {
        Prefab = obj;
        Path = ReturnPrefabPathModified(path);
    }

    private string ReturnPrefabPathModified(string path)
    {
        int extensionLength = System.IO.Path.GetExtension(path).Length;
        int startIndex = path.ToLower().IndexOf("resources");
        int additionalLength = 10;
        
        if (startIndex == -1)
        {
            return string.Empty;
        }
        else
            return path.Substring(startIndex + additionalLength, path.Length - (additionalLength + startIndex + extensionLength));
    }
}
