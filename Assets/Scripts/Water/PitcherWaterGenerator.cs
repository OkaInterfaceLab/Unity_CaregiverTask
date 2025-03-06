using UnityEngine;

public class PitcherWaterGenerator : MonoBehaviour
{
    public GameObject waterGrainPrefab;
    public GameObject targetObject;
    public float tiltThreshold = 30f;
    public float initialSpeed = 2f;
    public float destroyDelay = 3f;
    public float totalDuration = 3f;
    public float maxWidthSpawnRange = 0;
    [SerializeField] private PitcherWaterController _pitcherWaterController;
    [SerializeField] private BowlWaterSurfaceGenerator _bowlWaterSurfaceGenerator;
    [SerializeField] private MeshRenderer _waterSurfaceMeshRenderer;

    public float maxFillingRate = 0.7f;
    [HideInInspector] public float fillingRate;
    private float elapsedTime;
    private bool isFilled;
    private int leadingWaterLevel;

    private float currentWaterTemperature;

    void Start()
    {
        fillingRate = _pitcherWaterController.fillingRate;
        isFilled = _pitcherWaterController.fillingRate > 0;
    }

    void Update()
    {
        if (isFilled)
        {
            ControllWater();
        }
        else
        {
            _waterSurfaceMeshRenderer.enabled = false;
        }
    }

    private void ControllWater()
    {
        // fillingRateを0〜0.7の範囲にクランプし、線形補間用の係数を算出
        float clampedFillingRate = Mathf.Clamp(fillingRate, 0f, 0.7f);
        float t = 1f - (clampedFillingRate / 0.7f);  // fillingRate=0.7のときt=0、fillingRate=0のときt=1
        float tiltThreshold = Mathf.Lerp(30f, 100f, t);

        float tiltAngle = Vector3.Angle(Vector3.up, targetObject.transform.up);

        if (fillingRate > 0 && tiltAngle >= tiltThreshold)
        {
            elapsedTime += Time.deltaTime;
            fillingRate = Mathf.Clamp01(1f - (elapsedTime / totalDuration));
            _pitcherWaterController.fillingRate = fillingRate;

            // 法線方向（横方向）のオフセットを算出
            Vector3 normalDirection = Vector3.Cross(Vector3.up, targetObject.transform.up).normalized;
            float widthOffset = Random.Range(-maxWidthSpawnRange, maxWidthSpawnRange) * (fillingRate / maxFillingRate);

            // 生成位置の設定
            Vector3 spawnPosition = transform.position + normalDirection * widthOffset;
            GameObject sphere = Instantiate(waterGrainPrefab, spawnPosition, Quaternion.identity);

            WaterGrainController waterGrainController = sphere.GetComponent<WaterGrainController>();
            if (waterGrainController != null)
            {
                waterGrainController.waterTemperature = currentWaterTemperature;
            }

            // Rigidbodyを取得し、傾いている方向に初速を与える
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = targetObject.transform.up * initialSpeed;

            Destroy(sphere, destroyDelay);
        }
        else if (fillingRate <= 0.1)
        {
            _waterSurfaceMeshRenderer.enabled = false;
            isFilled = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterSurface")
        {
            if (other.gameObject.tag == "Bowl")
            {
                fillingRate = _pitcherWaterController.fillingRate;
                leadingWaterLevel = (int)((_bowlWaterSurfaceGenerator.maxPouredWaterLevel / 3) * ((maxFillingRate - fillingRate) / maxFillingRate));
                leadingWaterLevel = Mathf.Clamp(_bowlWaterSurfaceGenerator.pouredWaterLevel, 0, 90);
                Debug.Log(leadingWaterLevel);
            }

            _pitcherWaterController.fillingRate = maxFillingRate;
            fillingRate = _pitcherWaterController.fillingRate;
            _waterSurfaceMeshRenderer.enabled = true;

            // WaterSurface にアタッチされた WaterTemperature スクリプトを取得
            WaterTemperature _waterTemperature = other.GetComponent<WaterTemperature>();
            currentWaterTemperature = _waterTemperature.waterTemperature;

        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "WaterSurface")
        {
            if (other.gameObject.tag == "Bowl")
            {
                _bowlWaterSurfaceGenerator.pouredWaterLevel -= leadingWaterLevel;
                Debug.Log(_bowlWaterSurfaceGenerator.pouredWaterLevel);
            }

            isFilled = true;
            elapsedTime = 0;
        }
    }

}
