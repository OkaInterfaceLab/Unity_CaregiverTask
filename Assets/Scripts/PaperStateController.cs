using Oculus.Interaction;
using UnityEngine;

public class PaperStateController : MonoBehaviour
{
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private MeshRenderer _meshRenderer; // マテリアル変更用
    [SerializeField] private MeshFilter _meshFilter; // メッシュ変更用
    [SerializeField] private MeshCollider _meshCollider; // コライダー変更用

    [SerializeField] private Material openMaterial; // open状態用のマテリアル
    [SerializeField] private Mesh openMesh; // open状態用のメッシュ
    [SerializeField] private Vector3 openScale = Vector3.one; // open状態用のスケール

    [SerializeField] private Material closeMaterial; // close状態用のマテリアル
    [SerializeField] private Mesh closeMesh; // close状態用のメッシュ
    [SerializeField] private Vector3 closeScale = Vector3.one; // close状態用のスケール

    public enum PaperState
    {
        Open,  // 両手で掴んでいる状態
        Close  // 片手で掴んでいる状態
    }

    [HideInInspector] public PaperState _paperState; // 現在のPaperの状態

    void Start()
    {
        _paperState = PaperState.Close; // 初期状態をCloseに設定
        UpdatePaperAppearance(); // 初期状態に応じて外見を更新
    }

    void Update()
    {
        switch (_grabbable.SelectingPointsCount)
        {
            case 1:
                SetPaperStateClose();
                break;
            case 2:
                SetPaperStateOpen();
                break;
        }
    }

    private void SetPaperStateOpen()
    {
        if (_paperState != PaperState.Open)
        {
            _paperState = PaperState.Open; // 状態をOpenに設定
            UpdatePaperAppearance(); // 外見を更新
        }
    }

    private void SetPaperStateClose()
    {
        if (_paperState != PaperState.Close)
        {
            _paperState = PaperState.Close; // 状態をCloseに設定
            UpdatePaperAppearance(); // 外見を更新
        }
    }

    private void UpdatePaperAppearance()
    {
        switch (_paperState)
        {
            case PaperState.Open:
                if (openMaterial != null)
                    _meshRenderer.material = openMaterial;
                if (openMesh != null)
                {
                    _meshFilter.mesh = openMesh;
                    if (_meshCollider != null)
                        _meshCollider.sharedMesh = openMesh; // コライダーのメッシュを変更
                }
                transform.localScale = openScale; // スケールを変更
                break;

            case PaperState.Close:
                if (closeMaterial != null)
                    _meshRenderer.material = closeMaterial;
                if (closeMesh != null)
                {
                    _meshFilter.mesh = closeMesh;
                    if (_meshCollider != null)
                        _meshCollider.sharedMesh = closeMesh; // コライダーのメッシュを変更
                }
                transform.localScale = closeScale; // スケールを変更
                break;
        }
    }
}
