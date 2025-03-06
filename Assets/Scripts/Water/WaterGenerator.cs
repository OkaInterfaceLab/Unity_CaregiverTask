using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject targetObject;
    public float tiltThreshold = 30f;
    public float initialSpeed = 2f;
    public float destroyDelay = 3f;
    public float totalDuration = 3f;

    public float maxWidthSpawnRange = 0;
    [SerializeField] private PitcherWaterController _pitcherWaterController;
    [SerializeField] private MeshRenderer _waterSurfaceMeshRenderer;

    private float initialFillingRate;
    private float fillingRate;
    private float elapsedTime;
    private bool isFilled;

    void Start()
    {
        initialFillingRate = _pitcherWaterController.fillingRate;
        fillingRate = initialFillingRate;
        isFilled = _pitcherWaterController.fillingRate > 0;
    }

    void Update()
    {
        if (isFilled)
        {
            ControllWater();
        }
    }

    private void ControllWater()
    {
        float tiltAngle = Vector3.Angle(Vector3.up, targetObject.transform.up);

        if (fillingRate > 0 && tiltAngle >= tiltThreshold)
        {
            elapsedTime += Time.deltaTime;
            fillingRate = Mathf.Clamp01(1f - (elapsedTime / totalDuration));
            _pitcherWaterController.fillingRate = fillingRate;

            // 法線方向の横幅を計算
            Vector3 normalDirection = Vector3.Cross(Vector3.up, targetObject.transform.up).normalized;
            float widthOffset = Random.Range(-maxWidthSpawnRange, maxWidthSpawnRange) * (fillingRate / initialFillingRate);

            // 生成位置を設定
            Vector3 spawnPosition = transform.position + normalDirection * widthOffset;
            GameObject sphere = Instantiate(spherePrefab, spawnPosition, Quaternion.identity);

            // Rigidbodyを取得し、傾き方向に速度を設定
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = targetObject.transform.up * initialSpeed;

            Destroy(sphere, destroyDelay);
        }
        else if (fillingRate <= 0)
        {
            _waterSurfaceMeshRenderer.enabled = false;
            isFilled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "BowlWaterSurface")
        {
            _pitcherWaterController.fillingRate = initialFillingRate;
            fillingRate = _pitcherWaterController.fillingRate;
            _waterSurfaceMeshRenderer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "BowlWaterSurface")
        {
            isFilled = true;
            elapsedTime = 0;
        }
    }
}
