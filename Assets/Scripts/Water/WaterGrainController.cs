using UnityEngine;

public class WaterGrainController : MonoBehaviour
{
    public float waterTemperature;

    void OnTriggerEnter(Collider other)
    {
        // �g���K�[�ɓ������I�u�W�F�N�g�̖��O��"sphere"�̏ꍇ�ɍ폜
        if (other.gameObject.name == "WaterSurface")
        {
            // �g���K�[�ɓ������I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // �g���K�[�ɓ������I�u�W�F�N�g�̖��O��"sphere"�̏ꍇ�ɍ폜
        if (other.gameObject.name == "WaterSurface")
        {
            // �g���K�[�ɓ������I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // �g���K�[�ɓ������I�u�W�F�N�g�̖��O��"sphere"�̏ꍇ�ɍ폜
        if (other.gameObject.name == "WaterSurface")
        {
            // �g���K�[�ɓ������I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Debug.Log(waterTemperature);
    }
}
