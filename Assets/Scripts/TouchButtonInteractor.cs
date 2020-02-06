using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//raycastButtonTrigger v0.0.1
//Button interactor sphere that touches interactables UI devices
//Copyright by Space Animal
[RequireComponent(typeof(SphereCollider))]
public class TouchButtonInteractor : MonoBehaviour
{

    RaycastHit hit;
    
    public bool buttonPressing;
    private ConstantForce lander;
    //private LanderParticles particles;

    public bool objectInteractor;

    float forceX, forceY, forceZ = 0f;

    private void Start()
    {
        if(objectInteractor)
        {
            lander = GameObject.Find("LANDER").GetComponent<ConstantForce>();
            //particles = GameObject.Find("LANDER").GetComponentInChildren<LanderParticles>();
        }
        
    }

 

    public void Button1()
    {
        Debug.Log("button 1 pressed");
    }

    public void Button2()
    {
        Debug.Log("button 2 pressed");
    }

    public void Button3()
    {
        Debug.Log("button 3 pressed");
    }

    public void Button4()
    {
        Debug.Log("button 4 pressed");
    }

    void TouchRaycaster()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.01f))
        {
            if (hit.transform.tag == "Button" && buttonPressing == false)
            {
                hit.transform.gameObject.GetComponent<Button>().onClick.Invoke();
                hit.transform.gameObject.GetComponent<Button>().Select();
                buttonPressing = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {



        if (other.gameObject.tag == "Button")
        {
            TouchRaycaster();
            

            if(other.gameObject.name == "Button_Forward")
            {
                forceY = 270f;
                forceZ = 25f;
                if (objectInteractor)
                {
                    //particles.rocket_front.SetActive(true);
                    //particles.rocket_down.SetActive(true);
                }
            }
            if (other.gameObject.name == "Button_Right")
            {
                forceY = 270f;
                forceX = 25f;
                if (objectInteractor)
                {
                    //particles.rocket_right.SetActive(true);
                    //particles.rocket_down.SetActive(true);
                }

            }
            if (other.gameObject.name == "Button_Left")
            {
                forceY = 270f;
                forceX = -25f;
                if (objectInteractor)
                {
                    //particles.rocket_left.SetActive(true);
                    //particles.rocket_down.SetActive(true);
                }

            }
            if (other.gameObject.name == "Button_Back")
            {
                forceY = 270f;
                forceZ = -25f;
                if (objectInteractor)
                {
                    //particles.rocket_back.SetActive(true);
                    //particles.rocket_down.SetActive(true);
                }

            }
            if (other.gameObject.name == "Button_Up")
            {
                forceY = 270f;
                forceX = 0f;
                forceZ = 0f;
                if (objectInteractor)
                {
                    //particles.rocket_down.SetActive(true);
                }
            }

            if (other.gameObject.name == "Warp")
            {
                if (Time.timeScale == 0.1f)
                {
                    Time.timeScale = 1f;
                }
                else if (Time.timeScale == 1f)
                {
                    Time.timeScale = 0.1f;
                }
            }
            if (objectInteractor)
            {
                lander.force = new Vector3(forceX, forceY, forceZ);
            }

        }
        else
        {
            forceY = 0f;
            forceX = 0f;
            forceZ = 0f;


            
        }
    }



    private void OnTriggerExit(Collider other)
    {
     

        if (other.gameObject.tag == "Button")
        {
            buttonPressing = false;
            lander.force = Vector3.zero;


        }

        if (objectInteractor)
        {
            //particles.rocket_down.SetActive(false);
            //particles.rocket_front.SetActive(false);
            //particles.rocket_back.SetActive(false);
            //particles.rocket_left.SetActive(false);
           // particles.rocket_right.SetActive(false);

            //Time.timeScale = 1;
        }
    }

}
