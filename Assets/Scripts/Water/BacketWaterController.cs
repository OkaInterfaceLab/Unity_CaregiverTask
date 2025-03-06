using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class BacketWaterController : MonoBehaviour
{
    /// <summary>
    /// BottleLiquidシェーダ関連のプロパティID定義
    /// </summary>
    private static class ShaderPropertyId
    {
        public static readonly int BottleLiquidWaveCenter = Shader.PropertyToID("_WaveCenter");
        public static readonly int BottleLiquidWaveParams = Shader.PropertyToID("_WaveParams");
        public static readonly int BottleLiquidColorForward = Shader.PropertyToID("_LiquidColorForward");
        public static readonly int BottleLiquidColorBack = Shader.PropertyToID("_LiquidColorBack");
    }

    /// <summary>液面カラーオフセット値</summary>
    private static readonly Color LiquidColorTopOffset = new Color(0.15f, 0.15f, 0.15f, 0.0f);

    /// <summary>液体カラー</summary>
    [SerializeField] private Color liquidColor;

    /// <summary>瓶形状の概要を表すオフセットポイントリスト</summary>
    [SerializeField] private Vector3[] bottleSizeOffsetPoints;

    /// <summary>充填率</summary>
    [Range(0.0f, 1.0f)][SerializeField] public float fillingRate = 0.5f;

    /// <summary>位置差分による動きの影響率</summary>
    [Range(0.0f, 2.0f)][SerializeField] private float positionInfluenceRate = 0.7f;

    /// <summary>回転差分による動きの影響率</summary>
    [Range(0.0f, 2.0f)][SerializeField] private float rotationInfluenceRate = 0.4f;

    /// <summary>波の大きさの減衰率</summary>
    [Range(0.0f, 1.0f)][SerializeField] private float sizeAttenuationRate = 0.92f;

    /// <summary>波の周期の減衰率</summary>
    [Range(0.0f, 1.0f)][SerializeField] private float cycleAttenuationRate = 0.97f;

    /// <summary>時間による位相変化係数</summary>
    [SerializeField] private float cycleOffsetCoef = 12.0f;

    /// <summary>差分による変化量最大（波の大きさ）</summary>
    [SerializeField] private float deltaSizeMax = 0.15f;

    /// <summary>差分による変化量最大（波の周期）</summary>
    [SerializeField] private float deltaCycleMax = 10.0f;

    /// <summary>制御対象となるマテリアル</summary>
    private Material[] targetMaterials;

    /// <summary>前回参照位置</summary>
    private Vector3 prevPosition;

    /// <summary>前回参照オイラー角度</summary>
    private Vector3 prevEulerAngles;

    /// <summary>現在の液体波パラメータ</summary>
    private Vector4 waveCurrentParams;


    /// <summary>
    /// 開始時処理
    /// </summary>
    private void Start()
    {
        Renderer targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            return;
        }

        if (targetMaterials == null || targetMaterials.Length <= 0)
        {
            List<Material> targetMaterialList = new List<Material>();
            for (int index = 0; index < targetRenderer.sharedMaterials.Length; index++)
            {
                Material material = targetRenderer.sharedMaterials[index];
                if (material.shader.name.Contains("BacketLiquid"))
                {
                    targetMaterialList.Add(material);
                }
            }

            targetMaterials = targetMaterialList.ToArray();
        }

        waveCurrentParams = Vector4.zero;

        BackupTransform();
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        if (targetMaterials == null || targetMaterials.Length <= 0)
        {
            return;
        }

        CalculateWaveParams();
        SetupMaterials();

        BackupTransform();
    }

    /// <summary>
    /// 波パラメータ算出
    /// </summary>
    private void CalculateWaveParams()
    {
        // waveParamsにそのままベクトル演算
        // xは振幅、yは周期
        Vector4 attenuationRateVec = new Vector4(sizeAttenuationRate, cycleAttenuationRate, 0.0f, 0.0f);
        Vector4 deltaMaxVec = new Vector4(deltaSizeMax, deltaCycleMax, 0.0f, 0.0f);

        // 減衰処理
        waveCurrentParams = Vector4.Scale(waveCurrentParams, attenuationRateVec);

        // 位置と回転の差分から変化値産出
        Transform thisTransform = transform;
        Vector3 currentRotation = thisTransform.eulerAngles;
        Vector3 diffPos = thisTransform.position - prevPosition;
        Vector3 diffRot = new Vector3(
            Mathf.DeltaAngle(currentRotation.x, prevEulerAngles.x),
            Mathf.DeltaAngle(currentRotation.y, prevEulerAngles.y),
            Mathf.DeltaAngle(currentRotation.z, prevEulerAngles.z));

        waveCurrentParams += deltaMaxVec * (diffPos.magnitude * positionInfluenceRate);
        waveCurrentParams += deltaMaxVec * (diffRot.magnitude * rotationInfluenceRate * 0.01f); // オイラー角差分は元の値が大きいので補正

        waveCurrentParams = Vector4.Min(waveCurrentParams, deltaMaxVec);

        // 時間による位相変化は減衰対象外
        waveCurrentParams.z = cycleOffsetCoef;
    }

    /// <summary>
    /// 波の中心位置算出
    /// </summary>
    private Vector4 CalculateWaveCenter()
    {
        // xzはオブジェクトの中心（ワールド座標系）
        // yは瓶形状と充填率から液面の高さを設定（ワールド座標系）
        (float min, float max) liquidSurfaceHeight = GetLiquidSurfaceHeight();
        return transform.position +
               Vector3.up * Mathf.Lerp(liquidSurfaceHeight.min, liquidSurfaceHeight.max, fillingRate);
    }

    /// <summary>
    /// マテリアル設定
    /// </summary>
    private void SetupMaterials()
    {
        Vector4 waveCenter = CalculateWaveCenter();

        for (int index = 0; index < targetMaterials.Length; index++)
        {
            Material material = targetMaterials[index];
            material.SetVector(ShaderPropertyId.BottleLiquidWaveCenter, waveCenter);
            material.SetVector(ShaderPropertyId.BottleLiquidWaveParams, waveCurrentParams);
            material.SetColor(ShaderPropertyId.BottleLiquidColorForward, liquidColor);
            material.SetColor(ShaderPropertyId.BottleLiquidColorBack, liquidColor + LiquidColorTopOffset);
        }
    }

    /// <summary>
    /// 姿勢情報の保存
    /// </summary>
    private void BackupTransform()
    {
        prevPosition = transform.position;
        prevEulerAngles = transform.eulerAngles;
    }

    /// <summary>
    /// オブジェクトローカルにおける液面の高さ（最小/最大）を取得
    /// </summary>
    private (float min, float max) GetLiquidSurfaceHeight()
    {
        if (bottleSizeOffsetPoints == null || bottleSizeOffsetPoints.Length <= 0)
        {
            return (0.0f, 0.0f);
        }

        Transform thisTransform = transform;
        (float min, float max) ret = (float.MaxValue, float.MinValue);
        for (int index = 0; index < bottleSizeOffsetPoints.Length; index++)
        {
            Vector3 localPoint = thisTransform.TransformPoint(bottleSizeOffsetPoints[index]) - thisTransform.position;
            ret.min = Mathf.Min(ret.min, localPoint.y);
            ret.max = Mathf.Max(ret.max, localPoint.y);
        }

        return ret;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 選択時のギズモ表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (bottleSizeOffsetPoints == null || bottleSizeOffsetPoints.Length <= 0)
        {
            return;
        }

        // 瓶形状オフセットポイントの表示
        Gizmos.color = Color.yellow;
        for (int index = 0; index < bottleSizeOffsetPoints.Length; index++)
        {
            Vector3 point = bottleSizeOffsetPoints[index];
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.05f);
        }
    }
#endif
}
