using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacketEmptySurfaceGenerator : MonoBehaviour
{
    [SerializeField] private PitcherWaterController _pitcherWaterController;
    [SerializeField] private BacketEmptyWaterController _backetEmptyWaterController;

    private MeshRenderer _meshRenderer;
    private float fillingRate;
    private int pouredWaterLevel = 0;
    private float reducedWaterLevel = 0;
    public float maxPitcherFillingRate = 0.7f;
    public int maxPouredWaterLevel;  // 水の粒の最大レベル
    public float maxFillingRate = 0.7f;  // fillingRateの最大値

    // Start is called before the first frame update
    void Start()
    {
        fillingRate = _backetEmptyWaterController.fillingRate;  // 初期値を取得
        pouredWaterLevel = 0;

        // MeshRenderer を取得
        _meshRenderer = _backetEmptyWaterController.GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogError("MeshRenderer が BacketEmptyWaterController にアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 現在の pouredWaterLevel に応じて fillingRate を更新
        UpdateFillingRate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterGrain(Clone)")
        {
            pouredWaterLevel++;
            pouredWaterLevel = Mathf.Clamp(pouredWaterLevel, 0, maxPouredWaterLevel);  // 最大値を超えないように制御
            Debug.Log(pouredWaterLevel);
        }
        else if (other.gameObject.name == "PitcherWaterGenerator")
        {
            reducedWaterLevel = ((maxPitcherFillingRate - _pitcherWaterController.fillingRate) / maxPitcherFillingRate) * 60; //Pitcherのfillingrateに応じて水を減らす
        }
    }

    private void UpdateFillingRate()
    {
        // pouredWaterLevel に基づいて fillingRate を更新
        fillingRate = Mathf.Lerp(0, maxFillingRate, (float)pouredWaterLevel / maxPouredWaterLevel);
        fillingRate = Mathf.Clamp(fillingRate, 0, maxFillingRate);  // fillingRate の上限を制御

        // BowlWaterController に値を反映
        _backetEmptyWaterController.fillingRate = fillingRate;

        // fillingRate が 0 以下なら MeshRenderer をオフ、そうでなければオン
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = fillingRate > 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PitcherWaterGenerator")
        {
            pouredWaterLevel -= (int)reducedWaterLevel;
            UpdateFillingRate();
        }
    }
}
