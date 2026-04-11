using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ApiClient : MonoBehaviour
{
    public void SendGesture(string json)
    {
        StartCoroutine(Post(json));
    }

    IEnumerator Post(string json)
    {
        var request = new UnityWebRequest("http://10.196.222.59:3000/gestures", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log(request.result);
    }
}