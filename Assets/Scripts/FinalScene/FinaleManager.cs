using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum FinaleOrder
{
    DoorShut,
    NatalieOnPhone,
    JacobPullsPhone,
    NatalieResponds,
    JeffResponds
}

public class FinaleManager : MonoBehaviour
{
    public FinaleOrder Order;

    public GameObject Door;
    public Vector3 DoorShutRotation;

    public List<AudioClip> NatalieClips = new List<AudioClip>();
    public AudioSource NatalieSource;
    public Animator NatalieAnim;
    public List<AudioClip> JeffClips = new List<AudioClip>();
    public AudioSource JeffSource;

    public AudioClip DoorClose;

    public GameObject Phone;
    public Vector3 PhonePos;
    public Vector3 PhoneRot;
    public Transform Me;
    public GameObject RightHand;

    public Rig NatalieRig;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))  
        {
            ContinueScene();
        }
    }

    private IEnumerator ShutDoor()
    {
        Quaternion doorRot = Quaternion.Euler(DoorShutRotation);
        Quaternion originalRot = Door.transform.rotation;
        float lerp = 0;
        while (Door.transform.rotation != doorRot)
        {
            Door.transform.rotation = Quaternion.Lerp(originalRot, doorRot, lerp);
            lerp += Time.deltaTime;
            yield return null;
        }
        JeffSource.PlayOneShot(DoorClose);
        NatalieRig.weight = 1;
        yield return new WaitForSeconds(DoorClose.length);

        Order++;
        ContinueScene();
    }

    private IEnumerator Caught()
    {
        NatalieSource.clip = (NatalieClips[0]);
        NatalieSource.Play();
        yield return new WaitForSeconds(NatalieClips[0].length);
        NatalieClips.RemoveAt(0);
        NatalieAnim.transform.LookAt(Me);
        NatalieAnim.SetTrigger("StealPhone");
        NatalieAnim.applyRootMotion = false;
        Order++;
    }

    private IEnumerator CatchPhone()
    {
        Phone.transform.SetParent(RightHand.transform);
        Vector3 orig = Phone.transform.localPosition;
        Quaternion origRot = Phone.transform.localRotation;
        Quaternion endRot = Quaternion.Euler(PhoneRot);
        float lerp = 0;
        while (Phone.transform.localPosition != PhonePos && Phone.transform.localRotation != endRot)
        {
            Phone.transform.localPosition = Vector3.Lerp(orig, PhonePos, lerp);
            Phone.transform.localRotation = Quaternion.Lerp(origRot, endRot, lerp);
            lerp += Time.deltaTime;
            yield return null;
        }
        NatalieSource.clip = (NatalieClips[0]);
        NatalieSource.Play();
        NatalieAnim.SetBool("isTalking", true);
        yield return new WaitForSeconds(NatalieClips[0].length);
        NatalieAnim.SetBool("isTalking", false);
        NatalieClips.RemoveAt(0);
        Order++;
        Order++;
    }

    private IEnumerator NatalieResponds()
    {
        yield return null;
    }

    private void ContinueScene()
    {
        switch (Order)
        {
            case FinaleOrder.DoorShut:
                StartCoroutine(ShutDoor());
                break;
            case FinaleOrder.NatalieOnPhone:
                StartCoroutine(Caught());
                break;
            case FinaleOrder.JacobPullsPhone:
                StartCoroutine(CatchPhone());
                break;
            case FinaleOrder.NatalieResponds:
                StartCoroutine(NatalieResponds());
                break;
            case FinaleOrder.JeffResponds:
                JeffSource.PlayOneShot(JeffClips[0]);
                JeffSource.clip = JeffClips[1];
                JeffSource.PlayDelayed(JeffClips[0].length);
                JeffClips.RemoveAt(0);
                break;
        }
    }
}
