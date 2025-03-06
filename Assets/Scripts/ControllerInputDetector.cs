using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInputDetector : MonoBehaviour
{
    [SerializeField] private OVRInput.Button leftHandTrigger;
    [SerializeField] private OVRInput.Button rightHandTrigger;

    [HideInInspector] public bool isLeftTriggerPushing;
    [HideInInspector] public bool isRightTriggerPushing;
    [HideInInspector] public bool isLeftTriggerPressed;
    [HideInInspector] public bool isRightTriggerPressed;


    // Start is called before the first frame update
    void Start()
    {
        isLeftTriggerPushing = false;
        isRightTriggerPushing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(leftHandTrigger))
        {
            isLeftTriggerPushing = true;
        }
        else if (OVRInput.GetUp(leftHandTrigger))
        {
            isLeftTriggerPushing = false;
        }
        else if (OVRInput.Get(rightHandTrigger))
        {
            isRightTriggerPushing = true;
        }
        else if (OVRInput.GetUp(rightHandTrigger))
        {
            isRightTriggerPushing = false;
        }

        if (OVRInput.GetDown(leftHandTrigger))
        {
            isLeftTriggerPressed = true;
        }
        else if (OVRInput.GetUp(leftHandTrigger))
        {
            isLeftTriggerPressed = false;
        }
        else if (OVRInput.GetDown(rightHandTrigger))
        {
            isRightTriggerPressed = true;
        }
        else if (OVRInput.GetUp(rightHandTrigger))
        {
            isRightTriggerPressed = false;
        }


    }
}
