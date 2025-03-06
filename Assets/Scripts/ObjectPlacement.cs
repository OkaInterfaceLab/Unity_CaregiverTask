using UnityEngine;

public class ObjectPlacement : MonoBehaviour
{
    // レイの長さ
    [SerializeField]
    private float rayLength = 0.20f;

    // レイキャストで検出するレイヤー
    [SerializeField]
    private LayerMask layerMask = Physics.DefaultRaycastLayers;

    // レイがヒットした位置にワープさせるかどうか
    [SerializeField]
    private bool warpToHitPoint = true;

    // ヒット位置に表示するマーカーのプレハブ
    [SerializeField]
    private GameObject _markerPrefab;

    // マーカーのインスタンス
    private GameObject _markerInstance;

    // オブジェクトのコライダー
    [SerializeField]
    private Collider objectCollider;

    void Awake()
    {
        // オブジェクトのコライダーを取得
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider>();
        }

        // マーカーのインスタンスを作成
        if (_markerPrefab != null)
        {
            _markerInstance = Instantiate(_markerPrefab);
            _markerInstance.SetActive(false); // 最初は非表示
        }
    }

    void Update()
    {
        // レイの発射位置と方向
        Vector3 origin = transform.position;
        Vector3 direction = -transform.up; // オブジェクトの下方向

        // レイキャストの実行
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, rayLength, layerMask))
        {
            // レイがヒットした場合、マーカーを表示
            if (_markerInstance != null)
            {
                _markerInstance.transform.position = hitInfo.point;
                _markerInstance.SetActive(true);
            }

            // ワープが有効であれば、オブジェクトを移動
            if (warpToHitPoint)
            {
                // オブジェクトの底面までの距離を取得
                float distanceToBottom = GetDistanceToBottom();

                // オブジェクトをヒット位置に移動（底面が接するように）
                transform.position = hitInfo.point + hitInfo.normal * distanceToBottom;

                // オブジェクトを平面に直立させる
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hitInfo.normal) * transform.rotation;
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // レイがヒットしなかった場合、マーカーを非表示
            if (_markerInstance != null)
            {
                _markerInstance.SetActive(false);
            }
        }
    }

    // オブジェクトの中心から底面までの距離を計算するメソッド
    private float GetDistanceToBottom()
    {
        // コライダーのバウンディングボックスを取得
        Bounds bounds = objectCollider.bounds;

        // ローカル座標系での中心とエクステントを計算
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localExtents = bounds.extents;

        // ローカルY軸に沿った底面までの距離を計算
        float distance = localCenter.y - localExtents.y;

        // 正の値を得るために符号を反転
        distance = -distance;

        return distance;
    }

    // オブジェクトが選択されたときに呼び出されるメソッド
    public void OnSelect()
    {
        warpToHitPoint = false; // 掴んでいる間はワープを無効化
    }

    // オブジェクトの選択が解除されたときに呼び出されるメソッド
    public void OnUnselect()
    {
        warpToHitPoint = true; // 離したときにワープを再度有効化
    }
}
