using UnityEngine;

public class BowlWaterGenerator : MonoBehaviour
{
    [Header("Water Grain Settings")]
    public GameObject _waterGrainPrefab; // 水粒子のPrefab
    public float initialSpeed = 2f;      // 粒子の初速度
    public float destroyDelay = 3f;      // 粒子の消滅までの時間
    public float maxWidthSpawnRange = 0; // 生成位置の最大幅

    [Header("Bowl Settings")]
    public GameObject _targetObject;      // 対象オブジェクト（ボウル）
    public float tiltThreshold = 30f;     // 水が流れる傾きの閾値
    [SerializeField] private BowlWaterController _bowlWaterController; // ボウルの水管理スクリプト
    [SerializeField] private MeshRenderer _waterSurfaceMeshRenderer;  // 水面のMeshRenderer
    [SerializeField] private BowlWaterSurfaceGenerator _bowlWaterSurfaceGenerator;

    [Header("Filling Rate Settings")]
    public float maxFillingRate = 0.8f;   // 最大充填率
    public float totalDrainTime = 3f;     // 全ての水が流れるまでの時間

    private float fillingRate;            // 現在の充填率
    private int pouredWaterLevel;
    private float elapsedTime;            // 傾いた時間を計測
    private bool isFilled;                // ボウルに水があるかの状態
    private float drainSpeed;             // 一定の水減少スピード

    void Start()
    {
        // 初期化
        fillingRate = _bowlWaterController.fillingRate;
        isFilled = fillingRate > 0.05f;
        drainSpeed = maxFillingRate / totalDrainTime; // 水の減少速度を計算

        pouredWaterLevel = _bowlWaterSurfaceGenerator.pouredWaterLevel;
    }

    void Update()
    {
        // 現在の充填率を取得
        fillingRate = _bowlWaterController.fillingRate;

        pouredWaterLevel = _bowlWaterSurfaceGenerator.pouredWaterLevel;

        ControllBowlWater();

    }

    private void ControllBowlWater()
    {
        float tiltAngle = Vector3.Angle(Vector3.up, _targetObject.transform.up);
        // fillingRateを0〜0.7の範囲にクランプし、線形補間用の係数を算出
        float clampedFillingRate = Mathf.Clamp(fillingRate, 0f, 0.7f);
        float t = 1f - (clampedFillingRate / 0.7f);  // fillingRate=0.7のときt=0、fillingRate=0のときt=1
        float tiltThreshold = Mathf.Lerp(45f, 75f, t);

        if (pouredWaterLevel > 0)
        {
            if (tiltAngle >= tiltThreshold)
            {
                // 水を減少させる
                elapsedTime += Time.deltaTime;

                _bowlWaterSurfaceGenerator.pouredWaterLevel--;

                if (pouredWaterLevel > 0)
                {
                    SpawnWaterGrain();
                }
            }
        }
    }

    private void SpawnWaterGrain()
    {
        Vector3 normalDirection = Vector3.Cross(Vector3.up, _targetObject.transform.up).normalized;
        float widthOffset = Random.Range(-maxWidthSpawnRange, maxWidthSpawnRange) * (fillingRate / maxFillingRate);
        Vector3 spawnPosition = transform.position + normalDirection * widthOffset;

        GameObject waterGrain = Instantiate(_waterGrainPrefab, spawnPosition, Quaternion.identity);

        if (waterGrain.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = _targetObject.transform.up * initialSpeed;
        }

        Destroy(waterGrain, destroyDelay);
    }
}
