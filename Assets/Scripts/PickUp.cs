using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    //OnTriggerEnter that detects OVRHand component in child, if true, picks up the object by parenting it to the hand 
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInChildren<OVRHand>())
        {
            transform.parent = other.transform;
        }
    }
}