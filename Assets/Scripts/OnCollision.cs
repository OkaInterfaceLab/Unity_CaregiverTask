using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollision : MonoBehaviour
{
    public GameObject ResetText;

    //OnCollisionEnter()
    private void OnCollisionEnter(Collision collision)
{
    //アタッチしたオブジェクトが衝突したオブジェクトがFloorだった場合
    if (collision.gameObject.name == "Floor v1")
    {
        ResetText.SetActive(true);
    }
}
}