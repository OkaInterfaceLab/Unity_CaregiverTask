using UnityEngine;
using Oculus.Interaction;

public class BathTowelController : MonoBehaviour
{
    [Header("Grabbable Settings")]
    [SerializeField] private Grabbable grabbable;

    [Header("Display Settings")]
    [Tooltip("元のメッシュを表示するための MeshRenderer。把持時は非表示にします")]
    [SerializeField] private MeshRenderer originalMeshRenderer;
    [Tooltip("把持されたときに表示するオブジェクト。元オブジェクトの位置・回転に追従します")]
    [SerializeField] private GameObject grabbedDisplayObject;

    private bool isGrabbed = false;

    private void Awake()
    {
        // originalMeshRenderer が未設定なら、このオブジェクトの MeshRenderer を取得
        if (originalMeshRenderer == null)
        {
            originalMeshRenderer = GetComponent<MeshRenderer>();
        }
        // 初期状態：元のメッシュは表示、把持時表示用オブジェクトは非表示
        if (originalMeshRenderer != null)
        {
            originalMeshRenderer.enabled = true;
        }
        if (grabbedDisplayObject != null)
        {
            grabbedDisplayObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (grabbable == null)
            return;

        bool currentlyGrabbed = grabbable.SelectingPointsCount > 0;

        if (currentlyGrabbed && !isGrabbed)
        {
            isGrabbed = true;
            OnGrabbed();
        }
        else if (!currentlyGrabbed && isGrabbed)
        {
            isGrabbed = false;
            OnReleased();
        }

        // 把持中の場合、grabbedDisplayObject の位置と回転を元オブジェクトに追従させる
        if (isGrabbed && grabbedDisplayObject != null)
        {
            grabbedDisplayObject.transform.position = transform.position;
            grabbedDisplayObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// 把持開始時の処理：元のメッシュを非表示にし、把持表示用オブジェクトを有効化
    /// </summary>
    private void OnGrabbed()
    {
        if (originalMeshRenderer != null)
        {
            originalMeshRenderer.enabled = false;
        }
        if (grabbedDisplayObject != null)
        {
            grabbedDisplayObject.SetActive(true);
            grabbedDisplayObject.transform.position = transform.position;
            grabbedDisplayObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// 把持解除時の処理：元のメッシュを再表示し、把持表示用オブジェクトを非表示にする
    /// </summary>
    private void OnReleased()
    {
        if (originalMeshRenderer != null)
        {
            originalMeshRenderer.enabled = true;
        }
        if (grabbedDisplayObject != null)
        {
            grabbedDisplayObject.SetActive(false);
        }
    }
}
