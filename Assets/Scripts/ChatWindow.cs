using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour
{
    private ScrollRect m_scrollRect;
    [Range(0f, 1f)]
    public float value;
    public bool blah;
    // Start is called before the first frame update
    void Start()
    {
        m_scrollRect = GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (blah)
            m_scrollRect.normalizedPosition = new Vector2(m_scrollRect.normalizedPosition.x, value);
    }
}
