using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WagonMove : MonoBehaviour
{
    private BoxCollider GrabPointCol;

    [SerializeField] private ObjectsOnWagonController _objectsOnWagonControllerOver;
    [SerializeField] private ObjectsOnWagonController_Under _objectsOnWagonControllerUnder;
    [SerializeField] private TextMeshProUGUI _text;

    public int NubmerOfObjectsOnWagon => _objectsOnWagonControllerOver.OnWagonObjectsCount + _objectsOnWagonControllerUnder.OnWagonObjectsUnderCount;

    // Start is called before the first frame update
    void Start()
    {
        GrabPointCol = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(NubmerOfObjectsOnWagon >= 3 )
        {
            GrabPointCol.enabled = true;

            if (_text != null)
            {
                _text.text = "Can move wagon!!";
            }
        }

        else
        {
            GrabPointCol.enabled = false;

            if (_text != null)
            {
                _text.text = "Can't move wagon!!";
            }
        }

    }
}
