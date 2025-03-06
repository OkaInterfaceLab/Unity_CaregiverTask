using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ArmEventController : MonoBehaviour
{
    [SerializeField] TwoBoneIKConstraint _leftIKConstraint;
    [SerializeField] GameObject _leftArm_target;
    [SerializeField] GameObject _leftHand;

    private Quaternion targetInitialRot;
    private bool isGrab = false;
    private float weightDecreaseSpeed = 2.0f; // weight‚ğŒ¸­‚³‚¹‚é‘¬“x

    // Start is called before the first frame update
    void Start()
    {
        targetInitialRot = _leftArm_target.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGrab)
        {
            // weight‚ğ™X‚É0‚É‹ß‚Ã‚¯‚é
            if (_leftIKConstraint.weight > 0f)
            {
                _leftIKConstraint.weight = Mathf.Lerp(_leftIKConstraint.weight, 0f, weightDecreaseSpeed * Time.deltaTime);
            }

            _leftArm_target.transform.position = _leftHand.transform.position;
            _leftArm_target.transform.rotation = targetInitialRot;
        }
    }

    // è‚ğ’Í‚ñ‚¾‚Æ‚«‚Ìˆ—
    public void OnSelect()
    {
        _leftIKConstraint.weight = 1f;
        isGrab = true;
    }

    // è‚ğ—£‚µ‚½‚Æ‚«‚Ìˆ—
    public void OnUnSelect()
    {
        isGrab = false;
    }
}
