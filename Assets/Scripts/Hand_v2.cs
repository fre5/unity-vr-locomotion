using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// THE INTERACTABLE HAND(S) physics only, no animation

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Hand_v2 : MonoBehaviour
{
    ///--------------------------------------------------------------
    /// INTERACTABLES


    private SteamVR_Behaviour_Pose controller = null;
    public SteamVR_Action_Boolean trigger = null;
    public SteamVR_Action_Boolean grip = null;
    public SteamVR_Action_Vibration hapticAction;
    
    private CharacterControllerVR characterControllerVR;

    private Interactable interactableObject = null;
    public List<Interactable> interactableObjectList = new List<Interactable>();

    [Tooltip("Default is 2f")]
    [SerializeField]
    private float throwSpeed = 2f;

    Vector3 lastPos;
    Vector3 movement;

    private TouchButtonInteractor touch;
    private Animator handAnimatorLeft,handAnimatorRight;

    ///------------------------------------------------------------
    /// WEAPONRY
 
    public bool primaryHand, secondaryHand = false; //Transform of hands for two handed mode

    static Transform secondaryHandTransform;


    private void Awake()
    {
        controller = GetComponent<SteamVR_Behaviour_Pose>();
        characterControllerVR = GetComponentInParent<CharacterControllerVR>();
        touch = GetComponentInChildren<TouchButtonInteractor>();
        handAnimatorLeft = GameObject.Find("HAND_LEFT").GetComponentInChildren<Animator>();
        handAnimatorRight = GameObject.Find("HAND_RIGHT").GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        lastPos = transform.localPosition; //initialize last position so it won't be empty    

        GetComponent<SphereCollider>().isTrigger = true;
        GetComponent<SphereCollider>().center = new Vector3(-0.03f, -0.03f, -0.05f);
        GetComponent<SphereCollider>().radius = 0.05f;

        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Update()
    {
        //Debug.Log(secondaryHandTransform);

        if(touch.buttonPressing)
        {
            Pulse(0.1f, 0.1f, 0.1f, controller.inputSource);

        }

        //TRIGGERS OR GRIP
        // Grab an object on trigger down or grip down
        if (trigger.GetStateDown(controller.inputSource))
        {           
            Pickup();         
        }   
        // Drop an object on trigger up
        if (trigger.GetStateUp(controller.inputSource))
        {
            Drop();           
        }

        if (grip.GetStateDown(controller.inputSource))
        {
            Pickup();
        }

        if (grip.GetStateUp(controller.inputSource) )
        {
            Drop();
        }



        if (characterControllerVR.gripRight && characterControllerVR.triggerRight > 0.5 && characterControllerVR.thumbTouchRight)
        {
            handAnimatorRight.Play("Fist");
        }
        else if (characterControllerVR.gripRight && characterControllerVR.triggerRight <= 0.5)
        {
            handAnimatorRight.Play("Index");
        }
        else if (!characterControllerVR.gripRight && characterControllerVR.triggerRight > 0.5)
        {
            handAnimatorRight.Play("Okay");
        }
        else if (characterControllerVR.gripRight && characterControllerVR.triggerRight > 0.5 && !characterControllerVR.thumbTouchRight)
        {
            handAnimatorRight.Play("Thumb");
        }
        else
        {
            handAnimatorRight.Play("Rest");
        }

        if (characterControllerVR.gripLeft && characterControllerVR.triggerLeft > 0.5 && characterControllerVR.thumbTouchLeft)
        {
            handAnimatorLeft.Play("Fist");
        }
        else if (characterControllerVR.gripLeft && characterControllerVR.triggerLeft <= 0.5)
        {
            handAnimatorLeft.Play("Index");
        }
        else if (!characterControllerVR.gripLeft && characterControllerVR.triggerLeft > 0.5)
        {
            handAnimatorLeft.Play("Okay");
        }
        else if (characterControllerVR.gripLeft && characterControllerVR.triggerLeft > 0.5 && !characterControllerVR.thumbTouchLeft)
        {
            handAnimatorLeft.Play("Thumb");
        }
        else
        {
            handAnimatorLeft.Play("Rest");
        }

    }

    private void OnTriggerEnter(Collider other) // touching
    {      
        if (other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("weapon"))
        {
            
            interactableObjectList.Add(other.gameObject.GetComponent<Interactable>());
            Pulse(0.1f, 0.1f, 0.1f, controller.inputSource);
        }

        if (other.gameObject.CompareTag("Climbable"))
        {
            Pulse(0.1f, 0.1f, 0.1f, controller.inputSource);
        }

    }

    private void OnTriggerStay(Collider other) // stay climbing
    {
        if (other.gameObject.CompareTag("Climbable") && grip.GetState(controller.inputSource) && trigger.GetState(controller.inputSource))
        {

            Climbing();
        }
        if (other.gameObject.CompareTag("Climbable") && grip.GetStateUp(controller.inputSource))
        {

            characterControllerVR.gravity = -0.1f;
        }
    }

    private void OnTriggerExit(Collider other) // letting go
    {
        if (other.gameObject.CompareTag("Interactable") || other.gameObject.CompareTag("weapon"))
        {
            interactableObjectList.Remove(other.gameObject.GetComponent<Interactable>());        
        }
        if (other.gameObject.CompareTag("Climbable"))
        {
            characterControllerVR.gravity = -0.1f;

        }

        if(other.gameObject.CompareTag("weapon"))
        {
            if (primaryHand) { primaryHand = !primaryHand;  }
            else if (secondaryHand) { secondaryHand = !secondaryHand; secondaryHandTransform = null; }
        }
        
    }

    public void Pickup()
    {
       
            //Get nearest
            interactableObject = GetNearestInteractable();

            //null check
            if (!interactableObject)
            { return; }

            //already held, check
            if (interactableObject.activeHand && interactableObject.tag == "Interactable")
            {
                interactableObject.activeHand.Drop();
            }

            if(interactableObject.tag == "weapon") // if it's the only hand holding the object
            {
                if (interactableObject.handList.Count < 2)
                {
                    secondaryHandTransform = null;
                    secondaryHand = false;
                    primaryHand = true;
                }
                else if (interactableObject.handList.Count > 1)
                {
                    primaryHand = false;
                    secondaryHand = true;
                    secondaryHandTransform = transform;
                }
            }

            //attach
            Rigidbody targetBody = interactableObject.GetComponent<Rigidbody>();
            interactableObject.transform.parent = transform;
            targetBody.isKinematic = true;
            targetBody.useGravity = false;

            //set active hand
            interactableObject.activeHand = this;
 
    }

    public void Drop()
    {
        //null check
        if(!interactableObject) { return; }

        if (this == interactableObject.activeHand) // if this is the active hand then drop/throw
        {
            Rigidbody targetBody = interactableObject.GetComponent<Rigidbody>();
            targetBody.isKinematic = false;
            targetBody.useGravity = true;

            interactableObject.activeHand = null;
            interactableObject.transform.parent = null;

            Transform origin = controller.origin;

            targetBody.velocity = origin.TransformVector(controller.GetVelocity() * throwSpeed);

        }

        else // if this is not the hand holding the object then switch parent to the other hand
        {
            if(interactableObject.activeHand)
            interactableObject.transform.parent = interactableObject.activeHand.transform;
        }
    }
   
    //Scans closest object
    private Interactable GetNearestInteractable() // closest interactable
    {
        Interactable nearest  = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach(Interactable interactable in interactableObjectList)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;

            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }
        return nearest;    
    }

    //Climbing
    public void Climbing()
    {
        
        //Climbing movement
        movement = (transform.localPosition - lastPos);
        lastPos = transform.localPosition;
        characterControllerVR.gravity = -movement.y;

        //haptic
        //Pulse(0.1f, 0.1f, 0.1f, controller.inputSource);
    }
       
    //Haptic
    private void Pulse (float duration, float frequency, float amplitude, SteamVR_Input_Sources source)
    {
        hapticAction.Execute(0,duration, frequency, amplitude, source);

        //Debug.Log(source.ToString());
    }


    
}