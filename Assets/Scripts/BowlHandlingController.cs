using Oculus.Interaction;
using UnityEngine;

public class BowlHandlingController : MonoBehaviour
{
    [Header("Placement Preview Settings")]
    // スナップ先としてのゴースト表示用プレハブ
    [SerializeField] private GameObject ghostModelBowl;
    // 重ねる対象のオブジェクト（例：ベースとなるボウル）
    [SerializeField] private Transform placementTarget;
    // スナップ可能な許容距離
    [SerializeField] private float distanceThreshold = 0.1f;
    // スナップ時の上方向オフセット（placementTarget のローカル up 方向）
    [SerializeField] private float snapOffset = 0.03f;

    private GameObject placementPreviewInstance;
    private Grabbable grabbable;
    private Rigidbody rb;
    private Rigidbody placementTargetRb;

    // このボウルの把持状態
    private bool isGrabbed = false;
    // スタック状態（スナップ成立）かどうか
    private bool isStacked = false;

    // スタック時に記録する、placementTarget のローカル空間でのこのオブジェクトとの相対オフセット
    private Vector3 stackingOffset;
    // placementTarget に対する相対回転
    private Quaternion stackingRotationOffset;

    private void Awake()
    {
        // ゴーストモデルのインスタンスを生成（初期は非表示）
        placementPreviewInstance = Instantiate(ghostModelBowl);
        placementPreviewInstance.SetActive(false);

        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();

        if (placementTarget != null)
            placementTargetRb = placementTarget.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 把持中の場合
        if (grabbable.SelectingPointsCount > 0)
        {
            if (!isGrabbed)
            {
                isGrabbed = true;
                OnGrabbed();
            }
            else
            {
                UpdatePlacementPreview();

                // 両手把持になった場合はスタック状態を解除して自由移動させる
                if (isStacked && grabbable.SelectingPointsCount >= 2)
                {
                    // 必要に応じて Unstack() を呼び出す
                    // Unstack();
                }
            }
        }
        // 把持解除時
        else if (isGrabbed)
        {
            isGrabbed = false;
            OnReleased();
        }
        // 把持していない状態でスタック中
        else if (isStacked)
        {
            transform.position = placementTarget.TransformPoint(stackingOffset);
            transform.rotation = placementTarget.rotation * stackingRotationOffset;
        }
    }

    // ゴーストモデル（スナップ位置）の更新処理
    private void UpdatePlacementPreview()
    {
        if (placementTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, placementTarget.position);
        // 片手把持かつ許容距離内、かつまだスタックしていない場合のみゴーストを表示
        if (distance <= distanceThreshold && grabbable.SelectingPointsCount == 1 && !isStacked)
        {
            placementPreviewInstance.SetActive(true);
            Vector3 previewPos = placementTarget.position + placementTarget.up * snapOffset;
            placementPreviewInstance.transform.position = previewPos;
            placementPreviewInstance.transform.rotation = placementTarget.rotation;
        }
        else
        {
            placementPreviewInstance.SetActive(false);
        }
    }

    // 把持開始時の処理（必要に応じた初期更新）
    private void OnGrabbed()
    {
        UpdatePlacementPreview();
        if (isStacked)
        {
            isStacked = false;
        }
    }

    // 把持解除時の処理
    private void OnReleased()
    {
        if (placementTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, placementTarget.position);

        // 許容距離内ならスナップしてスタック状態に移行
        if (distance <= distanceThreshold)
        {
            // placementTarget のローカル up 方向にオフセットを加えた位置にスナップ
            Vector3 snapPos = placementTarget.position + placementTarget.up * snapOffset;
            transform.position = snapPos;
            transform.rotation = placementTarget.rotation;

            // placementTarget のローカル空間で、thisオブジェクトとの相対オフセットを記録
            stackingOffset = placementTarget.InverseTransformPoint(transform.position);
            stackingRotationOffset = Quaternion.Inverse(placementTarget.rotation) * transform.rotation;

            rb.isKinematic = true;
            isStacked = true;
        }
        else
        {
            Unstack();
        }

        placementPreviewInstance.SetActive(false);
    }

    // スタック状態の解除処理（例：両手把持時）
    private void Unstack()
    {
        isStacked = false;
        rb.isKinematic = false;
    }
}
