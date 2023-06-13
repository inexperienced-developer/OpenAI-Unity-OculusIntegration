using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterAnimator : MonoBehaviour
{
    private Animator m_animator;
    [SerializeField] private bool m_isJeff = true;
    private float m_switchTimer, m_nextSwitchTime;
    [SerializeField] private float[] m_idleVals, m_talkVals;
    private int m_idleIndex, m_talkIndex, m_nextIdleIndex, m_nextTalkIndex;
    private float m_lerp;
    private bool m_cycle;
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        Polly.s_StartTalking += Talk;
        Polly.s_EndTalking += StopTalk;
    }

    private void OnDestroy()
    {
        Polly.s_StartTalking -= Talk;
        Polly.s_EndTalking -= StopTalk;
    }

    private void StopTalk(bool forJeff)
    {
        if(m_isJeff == forJeff)
        {
            IsTalking(false);
        }
        else
        {
            IsTalking(false);
        }
    }

    private void Talk(bool forJeff)
    {
        if (m_isJeff == forJeff)
        {
            IsTalking(true);
        }
        else
        {
            IsTalking(true);
        }
    }

    private void Update()
    {
        m_switchTimer += Time.deltaTime;
        if(m_switchTimer > m_nextSwitchTime)
        {
            m_nextSwitchTime = Random.Range(10f, 20f);
            m_nextIdleIndex = m_idleIndex + 1;
            m_nextIdleIndex %= m_idleVals.Length;
            m_nextTalkIndex = m_talkIndex + 1;
            m_nextTalkIndex %= m_talkVals.Length;
            m_cycle = true;
        }
        if (m_cycle)
        {
            var idle = Mathf.Lerp(m_idleIndex, m_nextIdleIndex, m_lerp);
            var talk = Mathf.Lerp(m_talkIndex, m_nextTalkIndex, m_lerp);
            m_animator.SetFloat("Idle", idle);
            m_animator.SetFloat("Talking", talk);
            m_lerp += Time.deltaTime;
            if(m_lerp >= 1)
            {
                m_cycle = false;
                m_lerp = 0;
                m_switchTimer = 0;
                m_idleIndex = m_nextIdleIndex;
                m_talkIndex = m_nextTalkIndex;
            }
        }
    }

    public void IsTalking(bool val)
    {
        m_animator.SetBool("isTalking", val);
    }

}
