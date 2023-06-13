using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public enum AmazonVoice
{
    Joanna,
    Aditi,
    East2,
}

public class Polly : MonoBehaviour
{
    public static Polly Instance { get; private set; }

    private AmazonPollyClient m_client;
    private string m_output;
    private RegionEndpoint m_endPoint = RegionEndpoint.USEast1;
    [SerializeField] private SerializedVoice m_Voice;

    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_jeffClip;
    [SerializeField] private AudioClip[] m_thinkClips;

    private Coroutine m_addClipRoutine;
    //private Dictionary<uint, Coroutine> m_addClipDict = new Dictionary<uint, Coroutine>();

    private List<AudioClip> m_queue = new List<AudioClip>();

    private bool m_isPlaying;
    public static event Action<bool> s_StartTalking;
    public static event Action<bool> s_EndTalking;

    private void Awake()
    {
        Instance = this;
        StartCoroutine(PlayClipsFromQueue());
        m_client = new AmazonPollyClient("AKIA3GMSSTBRNDXPOQP4", "4T13Ha0lesgT+BNCO5GAcGknQTi3h7AHfdmkceDN", m_endPoint);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            MyNameIsJeff();
        }
    }

    private IEnumerator PlayClipsFromQueue()
    {
        while (true)
        {
            if(m_queue.Count <= 0)
            {
                if (m_isPlaying)
                {
                    m_isPlaying = false;
                    s_EndTalking?.Invoke(true);
                }
                yield return null;
                continue;
            }
            if (!m_audioSource.isPlaying)
            {
                m_audioSource.clip = m_queue[0];
                m_queue.RemoveAt(0);
                m_audioSource.Play();
                m_isPlaying = true;
                s_StartTalking?.Invoke(true);
            }
            yield return null;
        }
    }

    public string TestAudio = "Testing";
    [ContextMenu("Test with text")]
    public async void TestAudioClip()
    {
        await GetAmazonAudioClip(TestAudio);
    }

    public async Task GetAmazonAudioClip(string text, bool jeff = false)
    {
        Debug.Log("Building request");
        SynthesizeSpeechRequest synthesizeSpeechPresignRequest = new SynthesizeSpeechRequest();
        synthesizeSpeechPresignRequest.Engine = "neural";
        synthesizeSpeechPresignRequest.Text = text;
        synthesizeSpeechPresignRequest.VoiceId = VoiceId.Salli;
        synthesizeSpeechPresignRequest.SampleRate = "16000"; //OPTIONAL,REDUCES BANDWIDTH
        synthesizeSpeechPresignRequest.OutputFormat = OutputFormat.Mp3;
        m_output = Application.dataPath + "/Audio/Generated/" + (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + ".mp3";

        Debug.Log("Submitting request");
        var res = await m_client.SynthesizeSpeechAsync(synthesizeSpeechPresignRequest);
        Debug.Log("Received response, saving file");
        using (FileStream fs = File.Create(m_output))
        {
            res.AudioStream.CopyTo(fs);
            m_addClipRoutine = StartCoroutine(AddAudioClip(m_output, jeff));
        }
    }

    IEnumerator PlayAudioClip(string audio_path)
    {
        yield return null;
        Debug.Log("Playing file");
        Debug.Log("file://" + audio_path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audio_path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError) Debug.Log("error");
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                m_audioSource.Stop();
                m_audioSource.clip = clip;
                m_audioSource.Play();
            }
        }
    }

    private IEnumerator AddAudioClip(string path, bool jeff)
    {
        yield return null;
        Debug.Log("file://" + path);
        Debug.Log("Adding file");
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError) Debug.Log("error");
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                m_queue.Add(clip);
                if (jeff)
                    m_queue.Add(m_jeffClip);
                //m_audioSource.Stop();
                //m_audioSource.clip = clip;
                //m_audioSource.Play();
            }
        }
        m_addClipRoutine = null;
    }

    public void Think()
    {
        int maxCount = m_thinkClips.Length < 4 ? m_thinkClips.Length : 4;
        int count = UnityEngine.Random.Range(1, maxCount);
        for(int i = 0; i < count; i++) 
        {
            //int selected = UnityEngine.Random.Range(0, m_thinkClips.Length);
            m_queue.Add(m_thinkClips[i]);
        }
    }

    public void MyNameIsJeff()
    {
        m_queue.Add(m_jeffClip);
    }

    private static async Task<SynthesizeSpeechResponse> PollySynthesizeSpeech(IAmazonPolly client, string text)
    {
        var synthesizeSpeechRequest = new SynthesizeSpeechRequest()
        {
            OutputFormat = OutputFormat.Mp3,
            VoiceId = VoiceId.Joanna,
            Text = text,
        };

        var synthesizeSpeechResponse =
            await client.SynthesizeSpeechAsync(synthesizeSpeechRequest);

        return synthesizeSpeechResponse;
    }

    private static void WriteSpeechToStream(Stream audioStream, string outputFileName)
    {
        var outputStream = new FileStream(
            outputFileName,
            FileMode.Create,
            FileAccess.Write);
        byte[] buffer = new byte[2 * 1024];
        int readBytes;

        while ((readBytes = audioStream.Read(buffer, 0, 2 * 1024)) > 0)
        {
            outputStream.Write(buffer, 0, readBytes);
        }

        // Flushes the buffer to avoid losing the last second or so of
        // the synthesized text.
        outputStream.Flush();
        Console.WriteLine($"Saved {outputFileName} to disk.");
        Debug.Log($"Saved {outputFileName} to disk.");
    }
}
