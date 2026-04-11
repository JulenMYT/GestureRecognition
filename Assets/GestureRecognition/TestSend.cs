using UnityEngine;

public class TestSend : MonoBehaviour
{
    public ApiClient api;

    void Start()
    {
        string json = "{\"label\":\"circle\",\"points\":[[0,0],[1,1]],\"numPoints\":2}";
        api.SendGesture(json);
    }
}