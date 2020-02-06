using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    
    public Hand_v2 activeHand = null;
    public List<Hand_v2> handList = new List<Hand_v2>();


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "hand")
        {
            handList.Add(other.gameObject.GetComponent<Hand_v2>());
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "hand")
        {
            handList.Remove(other.gameObject.GetComponent<Hand_v2>());
        }
    }
}
