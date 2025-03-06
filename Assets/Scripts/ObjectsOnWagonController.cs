using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectsOnWagonController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI childCountText; // ���ʂ�\������UI�e�L�X�g

    List<GameObject> onWagonObjectsList = new List<GameObject>();

    public int OnWagonObjectsCount => onWagonObjectsList.Count; //�ǋL�i�R�c�j

    private void Start()
    {
        UpdateChildCountText();
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "ObjectsOnWagon")
        {
            onWagonObjectsList.Add(other.gameObject);
            Debug.Log("rifberuferfu");
            UpdateChildCountText();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if(other.gameObject.tag == "ObjectsOnWagon")
        {
            onWagonObjectsList.Remove(other.gameObject);
            UpdateChildCountText();
        }
    }

    public void OnSelect()
    {
        foreach(GameObject obj in onWagonObjectsList)
        {
            obj.transform.parent = this.transform;
        }
    }

    public void OnUnselect()
    {
        foreach(GameObject obj in onWagonObjectsList)
        {
            obj.transform.parent = null;
        }
    }

    private void UpdateChildCountText()
    {
        if(childCountText != null)
        {
            childCountText.text = "Number of Objects on Wagon: " + OnWagonObjectsCount;
        }
    }

}
