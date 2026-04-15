using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    private const int MaxAudioSources = 10;

    private AudioSource[] sources;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        CreateAudioSources();
    }

    private void CreateAudioSources()
    {
        sources = new AudioSource[MaxAudioSources];

        for (int i = 0; i < MaxAudioSources; i++)
        {
            GameObject child = new GameObject("AudioSource_" + i);
            child.transform.SetParent(transform);

            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sources[i] = source;
        }
    }

    private AudioSource GetAvailableSource()
    {
        for (int i = 0; i < MaxAudioSources; i++)
        {
            if (!sources[i].isPlaying)
                return sources[i];
        }

        return null;
    }

    public AudioSource PlaySound(AudioClip clip, float volume = 1f, bool loop = false)
    {
        var source = GetAvailableSource();

        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.pitch = 1f;

        source.Play();

        return source;
    }
}