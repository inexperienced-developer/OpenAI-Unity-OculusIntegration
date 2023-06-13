using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ChatGPT : MonoBehaviour
{
    private OpenAIAPI m_api;
    private Conversation m_activeConversation;

    [SerializeField] private TMP_InputField m_input;
    [SerializeField] private Transform m_chatBox;
    [SerializeField] private GameObject m_txt;

    private const string M_JEFF_SPLIT = "my name is jeff!";


    private void Awake()
    {
        //Get from env variables
        m_api = new OpenAIAPI(TempOpenAIVars.OPENAI_API_KEY);
    }

    private async void Update()
    {
    }

    public async void AskQuestion()
    {
        var qTxt = m_input.text;
        m_input.text = "";
        var question = Instantiate(m_txt, m_chatBox).GetComponent<TMP_Text>();
        question.SetText($"Me: {qTxt}");
        question.color = Color.green;
        question.gameObject.name = "Txt";
        string res = await Chat(qTxt);
        string[] split = res.Split("\n");
        foreach (var r in split)
        {
            if (r.ToLower().Contains(M_JEFF_SPLIT))
            {
                var regSplit = Regex.Split(r.ToLower(), $"(?<={M_JEFF_SPLIT})|(?={M_JEFF_SPLIT})").Select(s => s.Trim()).ToList();
                bool jeffPlayed = false;
                for (int i = 0; i < regSplit.Count(); i++)
                {
                    if (string.IsNullOrEmpty(regSplit[i])) continue;
                    else if (regSplit[i].Contains(M_JEFF_SPLIT))
                    {
                        if (i > 0)
                        {
                            await Polly.Instance.GetAmazonAudioClip(regSplit[i - 1], true);
                            jeffPlayed = true;
                        }
                        else
                        {
                            Polly.Instance.MyNameIsJeff();
                        }
                    }
                    else if (jeffPlayed)
                    {
                        await Polly.Instance.GetAmazonAudioClip(regSplit[i]);
                    }
                }
            }
            else
            {
                await Polly.Instance.GetAmazonAudioClip(r);
            }
            //await Polly.Instance.GetAmazonAudioClips(regSplit, jeffIndex);
            var answer = Instantiate(m_txt, m_chatBox).GetComponent<TMP_Text>();
            answer.SetText($"GPT: {r}");
            answer.color = Color.blue;
            answer.gameObject.name = "Txt";
        }
    }

    public async Task AskQuestion(string q)
    {
        Polly.Instance.Think();
        var qTxt = q;
        var question = Instantiate(m_txt, m_chatBox).GetComponent<TMP_Text>();
        question.SetText($"Me: {qTxt}");
        question.color = Color.green;
        question.gameObject.name = "Txt";
        string res = await Chat(qTxt);
        string[] split = res.Split("\n");
        foreach (var r in split)
        {
            if (r.ToLower().Contains(M_JEFF_SPLIT))
            {
                var regSplit = Regex.Split(r.ToLower(), $"(?<={M_JEFF_SPLIT})|(?={M_JEFF_SPLIT})").Select(s => s.Trim()).ToList();
                bool jeffPlayed = false;
                for (int i = 0; i < regSplit.Count(); i++)
                {
                    if (string.IsNullOrEmpty(regSplit[i])) continue;
                    else if (regSplit[i].Contains(M_JEFF_SPLIT))
                    {
                        if (i > 0)
                        {
                            await Polly.Instance.GetAmazonAudioClip(regSplit[i - 1], true);
                            jeffPlayed = true;
                        }
                        else
                        {
                            Polly.Instance.MyNameIsJeff();
                        }
                    }
                    else if (jeffPlayed)
                    {
                        await Polly.Instance.GetAmazonAudioClip(regSplit[i]);
                    }
                }
            }
            else
            {
                await Polly.Instance.GetAmazonAudioClip(r);
            }
            //await Polly.Instance.GetAmazonAudioClips(regSplit, jeffIndex);
            var answer = Instantiate(m_txt, m_chatBox).GetComponent<TMP_Text>();
            answer.SetText($"GPT: {r}");
            answer.color = Color.blue;
            answer.gameObject.name = "Txt";
        }
    }

    public bool NewConversation(ChatRequest req)
    {
        m_activeConversation = m_api.Chat.CreateConversation();
        //Append system messages
        foreach (var msg in req.SystemMsgs)
        {
            m_activeConversation.AppendSystemMessage(msg);
        }
        //Get the shorter of the user inputs and chat outputs --loop until the end
        int shorter = req.UserInputs.Count < req.ChatOutputs.Count ? req.UserInputs.Count : req.ChatOutputs.Count;
        for (int i = 0; i < shorter; i++)
        {
            m_activeConversation.AppendUserInput(req.UserInputs[i]);
            m_activeConversation.AppendExampleChatbotOutput(req.ChatOutputs[i]);
        }
        //If there are still chat outputs append those
        if (req.ChatOutputs.Count > shorter)
        {
            m_activeConversation.AppendExampleChatbotOutput(req.ChatOutputs[shorter]);
        }
        //If there are still user inputs append those
        if (req.UserInputs.Count > shorter)
        {
            m_activeConversation.AppendUserInput(req.UserInputs[shorter]);
        }
        return true;
    }

    public async Task<string> Chat(string question, ChatRequest newReqObj = null)
    {
        if (string.IsNullOrEmpty(question)) return null;
        if (newReqObj != null)
        {
            NewConversation(newReqObj);
        }
        else if (m_activeConversation == null)
        {
            NewConversation(NewReq_DEBUG);
        }
        m_activeConversation.AppendUserInput(question);
        string res = await m_activeConversation.GetResponseFromChatbot();
        print(res);
        return res;
    }

    //Debug
    public ChatRequest NewReq_DEBUG;
    public string NewQuestion_DEBUG;
    public bool NewConvo_DEBUG;

    [ContextMenu("Chat")]
    public async Task<bool> SendChat()
    {
        if (NewConvo_DEBUG || m_activeConversation == null)
        {
            NewConversation(NewReq_DEBUG);
            NewConvo_DEBUG = false;
        }
        NewReq_DEBUG.UserInputs.Add(NewQuestion_DEBUG);
        NewQuestion_DEBUG = "";
        NewReq_DEBUG.ChatOutputs.Add("Loading...");
        string res = await Chat(NewQuestion_DEBUG);
        NewReq_DEBUG.ChatOutputs[NewReq_DEBUG.ChatOutputs.Count - 1] = res;
        return true;
    }
}

[Serializable]
public class ChatRequest
{
    public List<string> SystemMsgs;
    public List<string> UserInputs;
    public List<string> ChatOutputs;

    public ChatRequest()
    {
        SystemMsgs = new List<string>();
        UserInputs = new List<string>();
        ChatOutputs = new List<string>();
    }

    public ChatRequest(List<string> systemMsgs, List<string> userInputs, List<string> chatOutputs)
    {
        SystemMsgs = systemMsgs;
        UserInputs = userInputs;
        ChatOutputs = chatOutputs;
    }
}
