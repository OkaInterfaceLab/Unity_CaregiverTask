using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollision : MonoBehaviour
{
    public GameObject ResetText;

    //OnCollisionEnter()
    private void OnCollisionEnter(Collision collision)
{
    //�A�^�b�`�����I�u�W�F�N�g���Փ˂����I�u�W�F�N�g��Floor�������ꍇ
    if (collision.gameObject.name == "Floor v1")
    {
        ResetText.SetActive(true);
    }
}
}