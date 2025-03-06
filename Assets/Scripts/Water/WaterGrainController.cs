using UnityEngine;

public class WaterGrainController : MonoBehaviour
{
    public float waterTemperature;

    void OnTriggerEnter(Collider other)
    {
        // トリガーに入ったオブジェクトの名前が"sphere"の場合に削除
        if (other.gameObject.name == "WaterSurface")
        {
            // トリガーに入ったオブジェクトを削除
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // トリガーに入ったオブジェクトの名前が"sphere"の場合に削除
        if (other.gameObject.name == "WaterSurface")
        {
            // トリガーに入ったオブジェクトを削除
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // トリガーに入ったオブジェクトの名前が"sphere"の場合に削除
        if (other.gameObject.name == "WaterSurface")
        {
            // トリガーに入ったオブジェクトを削除
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Debug.Log(waterTemperature);
    }
}
