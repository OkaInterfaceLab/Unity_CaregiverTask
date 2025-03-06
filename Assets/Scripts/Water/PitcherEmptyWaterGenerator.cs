using UnityEngine;

public class PitcherEmptyWaterGenerator : MonoBehaviour
{
    public GameObject waterGrainPrefab;
    public GameObject targetObject;
    public float tiltThreshold = 30f;
    public float initialSpeed = 2f;
    public float destroyDelay = 3f;
    public float totalDuration = 3f;
    public float maxWidthSpawnRange = 0;
    [SerializeField] private PitcherEmptyWaterController _pitcherEmptyWaterController;
    [SerializeField] private MeshRenderer _waterSurfaceMeshRenderer;
    [SerializeField] private WaterTemperature _waterTemperature;

    public float maxFillingRate = 0.7f;
    private float fillingRate;
    private float elapsedTime;
    private bool isFilled;

    private float currentWaterTemperature;

    void Start()
    {
        fillingRate = _pitcherEmptyWaterController.fillingRate;
        isFilled = _pitcherEmptyWaterController.fillingRate > 0;
        currentWaterTemperature = _waterTemperature.waterTemperature;
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
            // fillingRate = Mathf.Clamp01(1f - (elapsedTime / totalDuration));
            // 水量の変化をさせないため
            // _pitcherEmptyWaterController.fillingRate = fillingRate;

            // 法線方向の横幅を計算
            Vector3 normalDirection = Vector3.Cross(Vector3.up, targetObject.transform.up).normalized;
            float widthOffset = Random.Range(-maxWidthSpawnRange, maxWidthSpawnRange) * (fillingRate / maxFillingRate);

            // 生成位置を設定
            Vector3 spawnPosition = transform.position + normalDirection * widthOffset;
            GameObject sphere = Instantiate(waterGrainPrefab, spawnPosition, Quaternion.identity);

            WaterGrainController waterGrainController = sphere.GetComponent<WaterGrainController>();
            if (waterGrainController != null)
            {
                waterGrainController.waterTemperature = currentWaterTemperature;
            }

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
        if (other.gameObject.name == "WaterSurface")
        {
            _pitcherEmptyWaterController.fillingRate = maxFillingRate;
            fillingRate = _pitcherEmptyWaterController.fillingRate;
            _waterSurfaceMeshRenderer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "WaterSurface")
        {
            isFilled = true;
            elapsedTime = 0;
        }
    }
}
