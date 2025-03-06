using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectsOnWagonController_Under : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI childCountText; // 結果を表示するUIテキスト

    List<GameObject> onWagonObjectsListUnder = new List<GameObject>();

    public int OnWagonObjectsUnderCount => onWagonObjectsListUnder.Count; //追記（山田）

    private void Start()
    {
        UpdateChildCountText();
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "ObjectsOnWagon")
        {
            onWagonObjectsListUnder.Add(other.gameObject);
            Debug.Log("rifberuferfu");
            UpdateChildCountText();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if(other.gameObject.tag == "ObjectsOnWagon")
        {
            onWagonObjectsListUnder.Remove(other.gameObject);
            UpdateChildCountText();
        }
    }

    public void OnSelect()
    {
        foreach(GameObject obj in onWagonObjectsListUnder)
        {
            obj.transform.parent = this.transform;
        }
    }

    public void OnUnselect()
    {
        foreach(GameObject obj in onWagonObjectsListUnder)
        {
            obj.transform.parent = null;
        }
    }

    private void UpdateChildCountText()
    {
        if(childCountText != null)
        {
            childCountText.text = "Number of Objects on Wagon: " + OnWagonObjectsUnderCount;
        }
    }

}
