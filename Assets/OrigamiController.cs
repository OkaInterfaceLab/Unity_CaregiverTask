using UnityEngine;

public class OrigamiController : MonoBehaviour
{
    public GameObject origamiObject;    // Origamiをアタッチしたオブジェクト
    public GameObject anchor;           // Anchorが配置された親オブジェクト

    public float maxScale = 2.0f;        // スケールの最大値
    public float minScale = 0.5f;         // スケールの最小値

    private Origami origamiScript;

    private void Start()
    {
        if (origamiObject == null || anchor == null)
        {
            Debug.LogError("Origami Object or Anchor is not assigned.");
            return;
        }

        // Origamiスクリプトを取得
        origamiScript = origamiObject.GetComponent<Origami>();
        if (origamiScript == null)
        {
            Debug.LogError("Origami script is not attached to the assigned object.");
        }

        // origamiObjectをanchorの子として設定
        origamiObject.transform.SetParent(anchor.transform);
    }

    private void Update()
    {
        if (origamiObject == null || anchor == null || origamiScript == null) return;

        // スケールに基づいてFoldingの値を更新
        UpdateFoldingBasedOnScale();

        // Anchorの座標を変更
        UpdateAnchorPosition();
    }

    /// <summary>
    /// スケールに基づいてFoldingの値を更新
    /// </summary>
    private void UpdateFoldingBasedOnScale()
    {
        float currentScale = transform.localScale.x; // X軸スケールを基準にする
        // スケールに基づくFoldingの計算（値を反転）
        float foldingValue = 1.0f - Mathf.InverseLerp(minScale, maxScale, currentScale);
        origamiScript.Folding = foldingValue;
    }

    /// <summary>
    /// Anchorの位置を変更（親子関係を利用してorigamiObjectが移動）
    /// </summary>
    private void UpdateAnchorPosition()
    {
        // 例えば、Anchorの位置を動かす
        anchor.transform.position = transform.position;  // このオブジェクトと同じ位置にAnchorを移動させる
        anchor.transform.rotation = transform.rotation;  // 回転も合わせる場合
    }

    private void OnValidate()
    {
        // エディターでパラメータが変更された場合の更新処理
        if (origamiScript != null)
        {
            UpdateFoldingBasedOnScale();
        }

        if (origamiObject != null && anchor != null)
        {
            UpdateAnchorPosition();
        }
    }
}
