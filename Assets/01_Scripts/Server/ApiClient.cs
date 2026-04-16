using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[Serializable]
public class GestureData
{
    public string label;
    public Vector2[] points;
    public int numPoints;

    public string userId;
    public string createdAt;
    public string version;
    public string mode;
}

[Serializable]
public class GestureDataList
{
    public GestureData[] items;
}

public class ApiClient : MonoBehaviour
{
    public Action<GestureData[]> OnGesturesReceived;

    public void SendGesture(string json)
    {
        StartCoroutine(Post(json));
    }

    public void GetGestures(int page)
    {
        StartCoroutine(Get(page));
    }

    IEnumerator Post(string json)
    {
        var request = new UnityWebRequest("https://gesture-api.onrender.com/gestures", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-api-key", "dragonball123");

        yield return request.SendWebRequest();

        Debug.Log(request.result);
        Debug.Log(request.downloadHandler.text);
    }

    IEnumerator Get(int page)
    {
        string url = "https://gesture-api.onrender.com/gestures?page=" + page;

        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("x-api-key", "dragonball123");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            GestureDataList wrapper = JsonUtility.FromJson<GestureDataList>("{\"items\":" + json + "}");

            OnGesturesReceived?.Invoke(wrapper.items);
        }
    }
}