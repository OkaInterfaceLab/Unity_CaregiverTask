using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BowlSmallWaterSurfaceGenerator : MonoBehaviour
{
    [SerializeField] private BowlSmallWaterController _bowlSmallWaterController;
    [SerializeField] private TextMeshProUGUI _waterTemp;

    private MeshRenderer _meshRenderer;
    private float fillingRate;
    public int pouredWaterLevel = 0;
    public int maxPouredWaterLevel; // 水の粒の最大レベル
    public float maxFillingRate = 0.7f; // fillingRateの最大値

    public float currentBowlWaterTemperature = 20.0f; // 初期水温 (摂氏)
    public float ambientTemperature = 20.0f; // 環境温度 (摂氏)
    public float coolingRate = 0.1f; // 自然冷却率 (1秒当たりの温度減少)

    private void Start()
    {
        pouredWaterLevel = 0;

        // MeshRenderer を取得
        _meshRenderer = _bowlSmallWaterController.GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogError("MeshRenderer が BowlWaterController にアタッチされていません。");
        }
    }

    private void Update()
    {
        // BowlWaterController の fillingRate を常に取得
        fillingRate = _bowlSmallWaterController.fillingRate;

        // MeshRenderer の状態を更新
        UpdateMeshRenderer();

        // fillingRate を更新
        UpdateFillingRateFromPouredWaterLevel();

        // 自然冷却を適用
        ApplyNaturalCooling();

        if (fillingRate <= 0)
        {
            _waterTemp.text = "Bowl Water Temp : Empty";
        }
        else
        {
            _waterTemp.text = "Water Temp : " + currentBowlWaterTemperature + "C";
        }
    }

    private void UpdateMeshRenderer()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = fillingRate > 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterGrain(Clone)")
        {
            pouredWaterLevel++;
            pouredWaterLevel = Mathf.Clamp(pouredWaterLevel, 0, maxPouredWaterLevel);

            WaterGrainController _waterGrainController = other.GetComponent<WaterGrainController>();

            if (_waterGrainController != null)
            {
                // 水粒子の温度を取得してボウルの水温を更新
                float waterGrainTemperature = _waterGrainController.waterTemperature;
                UpdateBowlWaterTemperature(waterGrainTemperature);
            }

            Destroy(other.gameObject); // 水粒子を削除
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PitcherWaterGenerator")
        {
        }
    }

    private void UpdateFillingRateFromPouredWaterLevel()
    {
        fillingRate = Mathf.Lerp(0, maxFillingRate, (float)pouredWaterLevel / maxPouredWaterLevel);
        fillingRate = Mathf.Clamp(fillingRate, 0, maxFillingRate);

        _bowlSmallWaterController.fillingRate = fillingRate;

        // Debug.Log(fillingRate);
    }

    private void UpdateBowlWaterTemperature(float waterGrainTemperature)
    {
        // ボウル内の水の総量 (pouredWaterLevel)
        float bowlWaterMass = pouredWaterLevel;

        // 水粒子の量 (1粒分)
        float grainMass = 1.0f;

        // ボウル内の水量がゼロの場合、単純に粒子の温度を採用
        if (bowlWaterMass <= 0)
        {
            currentBowlWaterTemperature = waterGrainTemperature;
            return;
        }

        // 新しい総水量を計算
        float newTotalMass = pouredWaterLevel + grainMass;

        // 質量加重平均で新しい温度を計算
        currentBowlWaterTemperature = (pouredWaterLevel * currentBowlWaterTemperature + grainMass * waterGrainTemperature) / newTotalMass;

    }

    private void ApplyNaturalCooling()
    {
        // 時間経過に基づく自然冷却
        if (currentBowlWaterTemperature > ambientTemperature)
        {
            currentBowlWaterTemperature -= coolingRate * Time.deltaTime;
            currentBowlWaterTemperature = Mathf.Max(currentBowlWaterTemperature, ambientTemperature);
        }
    }
}
