using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//CharacterControllerVR v0.0.3
//VR Character Controller that will take OpenVR controller Inputs and also enables player locomotion and rotation based on head position as an anchor
//Copyright by Space Animal


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SteamVR_ActivateActionSetOnLoad))]
public class CharacterControllerVR : MonoBehaviour

{
    [Header("SteamVR Properties")]
    public SteamVR_Action_Vector2 touchpadPosition;
    public SteamVR_Action_Boolean grip; // Grip for Left and Right Controllers
    public SteamVR_Action_Single trigger; // Trigger for Left and Right Controllers
    public SteamVR_Action_Boolean topButton; // Y or B button
    public SteamVR_Action_Boolean bottomButton; // X or A button
    public SteamVR_Action_Boolean touchpadDown; // Down on Left and Right Thumbstick
    public SteamVR_Action_Boolean thumbTouch; // Controllers thumbstick touch
   
    private CharacterController characterController; // CharacterController Component *required
    [Header("Character Controller")]

    [SerializeField]
    private Transform pivot; // Create a gameobject Transform to use as an anchor for player rotation (RotatorAnchor)
    [SerializeField]
    private Transform bodyRotationHeadAnchor;   // Locomotion rotation anchor 
    [SerializeField]
    private Transform handDirection; // Locomotion moving direction anchor
    [SerializeField]
    private Transform hipPosition; // Emulated hip position
    [SerializeField]
    private Transform feetPosition; // Emulated feet position

    [Tooltip("In Meters")]
    [SerializeField]
    public float gravity; //Player gravity setting
    [Tooltip("Default is 0.6f")]
    [SerializeField]
    private float playerAverageScale; // Player height adjustment

    [Tooltip("[1 - 10] Default:7")]
    [SerializeField]
    private float fastSpeed = 7f; // Player walk and speed  
    [Tooltip("[1 - 3] Default:2")]
    [SerializeField]
    private float slowSpeed = 2f; // Player crouch walk speed

    private bool yaw = false; // State of rotation to prevent constant rotation
    private float distance, realSpeed; // Distance from head to feet and adjusted motion speed 

    [HideInInspector]
    public Vector2 touchpadValueRight, touchpadValueLeft;
    
    [HideInInspector]
    public float triggerRight, triggerLeft;

    [HideInInspector]
    public bool Xbutton, Ybutton, Abutton, Bbutton, gripRight, gripLeft, touchpadDownLeft, touchpadDownRight, thumbTouchLeft, thumbTouchRight;

    private void VRInput()
    {
        //Touchpad Value of both controllers in Vector2
        touchpadValueRight = touchpadPosition.GetAxis(SteamVR_Input_Sources.RightHand);
        touchpadValueLeft = touchpadPosition.GetAxis(SteamVR_Input_Sources.LeftHand);

        //Grip value in booleans
        gripRight = grip.GetState(SteamVR_Input_Sources.RightHand);
        gripLeft = grip.GetState(SteamVR_Input_Sources.LeftHand);

        //Trigger value in floats
        triggerRight = trigger.GetAxis(SteamVR_Input_Sources.RightHand);
        triggerLeft = trigger.GetAxis(SteamVR_Input_Sources.LeftHand);

        //Button value in Booleans
        Abutton = bottomButton.GetState(SteamVR_Input_Sources.RightHand);
        Xbutton = bottomButton.GetState(SteamVR_Input_Sources.LeftHand);
        Bbutton = topButton.GetState(SteamVR_Input_Sources.RightHand);
        Ybutton = topButton.GetState(SteamVR_Input_Sources.LeftHand);

        //Touchpad downpress in booleans
        touchpadDownRight = touchpadDown.GetState(SteamVR_Input_Sources.RightHand);
        touchpadDownLeft = touchpadDown.GetState(SteamVR_Input_Sources.LeftHand);

        //Touchpad thumb touching
        thumbTouchRight = thumbTouch.GetState(SteamVR_Input_Sources.RightHand);
        thumbTouchLeft = thumbTouch.GetState(SteamVR_Input_Sources.LeftHand);

    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();        
    }

    void Start()
    {
        characterController.slopeLimit = 45;
        characterController.stepOffset = 0.3f;
        characterController.skinWidth = 0.1f;
        characterController.minMoveDistance = 0;
        characterController.center = new Vector3(0f, 5f, 0f);
        characterController.radius = 0.12f;
        characterController.height = 1;

        //Set up rotation anchor for rotation pivot point
        
        pivot.parent = bodyRotationHeadAnchor;
        pivot.localRotation = Quaternion.identity;
        pivot.localPosition = Vector3.zero;
        pivot.localScale = Vector3.one;

        //Character controller properties setup
        characterController.skinWidth = 0.08f;
        characterController.radius = 0.1f;
    }

    private void ResetPosition()
    {
        transform.position = new Vector3(transform.position.x - feetPosition.transform.position.x, 2.55f, transform.position.z - feetPosition.transform.position.z);
        

    }

    private void Update()
    {

        //MOTION OF CHARACTER
        characterController.Move(new Vector3(0f, gravity, 0f));

        


        VRInput();

        if (Input.GetKeyUp(KeyCode.H))
        {
            ResetPosition();
        }

        //Move CharacterController collision to the center of character
        characterController.center = new Vector3(bodyRotationHeadAnchor.transform.localPosition.x, playerAverageScale, bodyRotationHeadAnchor.transform.localPosition.z);


        //Body rotation constraint
        pivot.transform.eulerAngles = new Vector3(0, pivot.transform.eulerAngles.y * Time.deltaTime, 0);
     
        //Running speed adjuster
        distance = Vector3.Distance(bodyRotationHeadAnchor.position, feetPosition.position);
        if(distance > 1.5f) { realSpeed = fastSpeed; }
        else if (distance <= 1.5f) { realSpeed = distance * slowSpeed;  }

        //Hip position
        hipPosition.position = new Vector3(feetPosition.position.x, feetPosition.position.y + distance * 0.5f, feetPosition.position.z);
        

        //Feet position
        feetPosition.position = new Vector3(bodyRotationHeadAnchor.position.x, 0f + transform.position.y, bodyRotationHeadAnchor.position.z);

        //Check movement only when player is grounded
        if (characterController.isGrounded)
        {
            //Body movement in X and Y axis

            //Forward and back
            characterController.Move(new Vector3(handDirection.transform.forward.x * touchpadValueLeft.y, gravity, handDirection.transform.forward.z * touchpadValueLeft.y) * realSpeed * Time.deltaTime);

            //Left and right
            characterController.Move(new Vector3(handDirection.transform.right.x * touchpadValueLeft.x, gravity, handDirection.transform.right.z * touchpadValueLeft.x) * realSpeed * Time.deltaTime);
        }

        //Rotate player       
        if(yaw == true)
        {
            if (touchpadValueRight.x < 0.5f && touchpadValueRight.x > -0.5f)
            { yaw = false; }
            else { return; }
        }
        else 
        {
            if (touchpadValueRight.x >= 0.5f)
            {
                transform.RotateAround(pivot.transform.position, pivot.transform.up, 45);
                yaw = true;
            }
            else if (touchpadValueRight.x <= -0.5f)
            {
                transform.RotateAround(pivot.transform.position, pivot.transform.up, -45);
                yaw = true;
            }
            
        }
        

    }



}
