using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegEventController : MonoBehaviour
{
    [SerializeField] MultiRotationConstraint _multiRotationConstraint;
    [SerializeField] GameObject _weightAnchor;

    [SerializeField] TwoBoneIKConstraint _legIKConstraint;
    [SerializeField] GameObject _leg_target;
    [SerializeField] GameObject _foot;

    private Quaternion targetInitialRot; // ターゲットの初期回転
    private bool isGrab = false; //掴まれているかどうか
    private float weightDecreaseSpeed = 2.0f; // IK weightの減少速度

    private float maxDistance;  // 初期距離を保存

    // Start is called before the first frame update
    void Start()
    {
        if (_multiRotationConstraint != null && _weightAnchor != null)
        {
            // オブジェクトと_weightAnchorの初期距離を計算
            maxDistance = Vector3.Distance(_leg_target.transform.position, _weightAnchor.transform.position);
            _multiRotationConstraint.weight = 0.3f; // 最初のウェイトを設定
        }

        // ターゲットの初期回転を保存
        targetInitialRot = _leg_target.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (_multiRotationConstraint != null && _weightAnchor != null)
        {
            // 現在の距離を計算
            float distance = Vector3.Distance(_leg_target.transform.position, _weightAnchor.transform.position);

            // 距離が近づくにつれてウェイトを1に、離れると0.3に近づける
            float weight = Mathf.Clamp(1f - (distance / maxDistance), 0.3f, 1f);

            // 計算したウェイトをMultiRotationConstraintに適用
            _multiRotationConstraint.weight = weight;
        }

        // 手が掴まれていない場合、IKのウェイトを徐々に減少させる
        if (!isGrab)
        {
            // weightを徐々に0に近づける
            if (_legIKConstraint.weight > 0f)
            {
                _legIKConstraint.weight = Mathf.Lerp(_legIKConstraint.weight, 0f, weightDecreaseSpeed * Time.deltaTime);
            }

            // ターゲットの位置と回転を手に同期
            _leg_target.transform.position = _foot.transform.position;
            _leg_target.transform.rotation = targetInitialRot;
        }
    }

    // 手を掴んだときの処理
    public void OnSelect()
    {
        _legIKConstraint.weight = 1f;
        isGrab = true;
    }

    // 手を離したときの処理
    public void OnUnSelect()
    {
        isGrab = false;
    }
}
