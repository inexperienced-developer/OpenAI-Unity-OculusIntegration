using Meta.WitAi.Json;
using Meta.WitAi;
using Oculus.Voice;
using UnityEngine;
using TMPro;
using Oculus.Voice.Demo.LightTraitsDemo;

public class VoiceToText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField textArea;
    [SerializeField] private bool showJson;

    [Header("Voice")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;

    [SerializeField] private ChatGPT m_chat;

    [SerializeField] private GameObject[] m_lights;

    // Whether voice is activated
    public bool IsActive => _active;
    private bool _active = false;

    // Add delegates
    private void OnEnable()
    {
        appVoiceExperience.events.OnRequestCreated.AddListener(OnRequestStarted);
        appVoiceExperience.events.OnPartialTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.events.OnFullTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.events.OnStartListening.AddListener(OnListenStart);
        appVoiceExperience.events.OnStoppedListening.AddListener(OnListenStop);
        appVoiceExperience.events.OnStoppedListeningDueToDeactivation.AddListener(OnListenForcedStop);
        appVoiceExperience.events.OnStoppedListeningDueToInactivity.AddListener(OnListenForcedStop);
        appVoiceExperience.events.OnResponse.AddListener(OnRequestResponse);
        appVoiceExperience.events.OnError.AddListener(OnRequestError);
    }
    // Remove delegates
    private void OnDisable()
    {
        appVoiceExperience.events.OnRequestCreated.RemoveListener(OnRequestStarted);
        appVoiceExperience.events.OnPartialTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.events.OnFullTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.events.OnStartListening.RemoveListener(OnListenStart);
        appVoiceExperience.events.OnStoppedListening.RemoveListener(OnListenStop);
        appVoiceExperience.events.OnStoppedListeningDueToDeactivation.RemoveListener(OnListenForcedStop);
        appVoiceExperience.events.OnStoppedListeningDueToInactivity.RemoveListener(OnListenForcedStop);
        appVoiceExperience.events.OnResponse.RemoveListener(OnRequestResponse);
        appVoiceExperience.events.OnError.RemoveListener(OnRequestError);
    }

    private void Start()
    {
        SetActivation(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ToggleActivation();
    }

    // Request began
    private void OnRequestStarted(WitRequest r)
    {
        // Store json on completion
        if (showJson) r.onRawResponse = (response) => textArea.text = response;
        // Begin
        _active = true;
    }
    // Request transcript
    private void OnRequestTranscript(string transcript)
    {
        textArea.text = transcript;
    }
    // Listen start
    private void OnListenStart()
    {
        textArea.text = "Listening...";
    }
    // Listen stop
    private void OnListenStop()
    {
        textArea.text = "Processing...";
    }
    // Listen stop
    private void OnListenForcedStop()
    {
        if (!showJson)
        {
            textArea.text = "";
        }
        OnRequestComplete();
    }

    private void LightToggle()
    {
        foreach(var light in m_lights)
        {
            light.SetActive(!light.activeSelf);
        }
    }

    // Request response
    private async void OnRequestResponse(WitResponseNode response)
    {
        if (!showJson)
        {
            if (!string.IsNullOrEmpty(response["text"]))
            {
                //textArea.text = "I heard: " + response["text"];
                textArea.text = "";
                //                await m_chat.AskQuestion(response["text"]); //debug
                LightToggle();
            }
            else
            {
                textArea.text = "";
            }
        }
        OnRequestComplete();
        ToggleActivation();
    }
    // Request error
    private void OnRequestError(string error, string message)
    {
        if (!showJson)
        {
            textArea.text = $"<color=\"red\">Error: {error}\n\n{message}</color>";
        }
        OnRequestComplete();
    }
    // Deactivate
    private void OnRequestComplete()
    {
        _active = false;
    }

    // Toggle activation
    public void ToggleActivation()
    {
        SetActivation(!_active);
    }
    // Set activation
    public void SetActivation(bool toActivated)
    {
        if (_active != toActivated)
        {
            _active = toActivated;
            if (_active)
            {
                appVoiceExperience.Activate();
            }
            else
            {
                appVoiceExperience.Deactivate();
            }
        }
    }
}