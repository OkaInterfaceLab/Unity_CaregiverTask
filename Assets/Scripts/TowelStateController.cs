using Oculus.Interaction;
using UnityEngine;
using System.Collections; //コルーチンのために追記（山田）

public class TowelStateController : MonoBehaviour
{
    [SerializeField] private Grabbable _grabbable; 
    [SerializeField] private MeshRenderer _meshRenderer; // マテリアル変更用
    [SerializeField] private MeshFilter _meshFilter; // メッシュ変更用
    [SerializeField] private Material wetMaterial; // 濡れた状態用の新しいマテリアル
    [SerializeField] private Mesh wetMesh; // 濡れた状態用の新しいメッシュ
    [SerializeField] private Material grabbedMaterial; // grab状態用の新しいマテリアル
    [SerializeField] private Mesh grabbedMesh; // grab状態用の新しいメッシュ
    [SerializeField] private Material onTableMaterial; // テーブル上にある状態用のマテリアル
    [SerializeField] private Mesh onTableMesh; // テーブル上にある状態用のメッシュ
    [SerializeField] private Material squeezingMaterial; // squeezing状態用のマテリアル
    [SerializeField] private Mesh squeezingMesh; // squeezing状態用のメッシュ

    [SerializeField] private Material squeezedMaterial; //squeezed状態用のマテリアル（山田）
    [SerializeField] private Mesh squeezedMesh; //squeezed状態用のメッシュ（山田）
    [SerializeField] private float squeezeTime = 3.0f; //絞る秒数の変数指定（山田）

    private bool isOnTable = false;

    public enum TowelState
    {
        OnTable, // テーブルの上にある状態
        Grabbed, // 掴まれている状態
        Squeezing, // 絞られている状態
        Wet, // 濡れている状態
        Squeezed //絞られた後の状態
    }

    [HideInInspector] public TowelState _towelState; // 現在のタオルの状態

    void Start()
    {
        _towelState = TowelState.OnTable; // 初期状態をテーブルの上に設定
        UpdateTowelAppearance(); // 初期状態に応じて外見を更新
    }

    void Update()
    {
        if(_towelState == TowelState.Wet)
        {
            switch (_grabbable.SelectingPointsCount)
            {
                case 0:
                    if (isOnTable)
                    {
                        SetTowelStateOnTable();
                    }
                    break;
                case 1:
                    SetTowelStateWet();
                    break;
                case 2:
                    SetTowelStateSqueezing();
                    StartCoroutine(ToSqueezed(squeezeTime)); //付け足し（山田）
                    break;
            }
        }
        else if(_towelState == TowelState.Grabbed || _towelState == TowelState.OnTable)  //.Wetから.Grabbedまたは.OnTableへ（山田）
        {
            switch (_grabbable.SelectingPointsCount)
            {
                case 0:
                    if (isOnTable)
                    {
                        SetTowelStateOnTable();
                    }
                    break;
                case 1:
                    SetTowelStateGrabbed();
                    break;
                case 2:
                    SetTowelStateGrabbed();
                    break;
            }
        }
        else if(_towelState == TowelState.Squeezing) //付け足し（山田）
        {
            switch (_grabbable.SelectingPointsCount)
            {
                case 0:
                    if (isOnTable)
                    {
                        SetTowelStateOnTable();
                    }
                    break;
                case 1:
                    SetTowelStateGrabbed();
                    break;
                case 2:
                    SetTowelStateSqueezing();
                    break;
            }
        }
        else if(_towelState == TowelState.Squeezed)   //付け足し（山田）
        {
            switch (_grabbable.SelectingPointsCount)
            {
                case 0:
                    if (isOnTable)
                    {
                        SetTowelStateOnTable();
                    }
                    break;
                case 1:
                    SetTowelStateSqueezed();
                    break;
                case 2:
                    SetTowelStateSqueezed();
                    break;
            }
        }
    }

    //一定時間後にSqueezed状態に移行するコルーチン（山田）
    private IEnumerator ToSqueezed(float delaySeconds)
    {
        // Squeezing状態でなければ終了
        if(_towelState != TowelState.Squeezing)
            yield break;

        // 指定秒数待機
        yield return new WaitForSeconds(delaySeconds);

        // 状態がまだSqueezingであればSqueezedに変更
        if(_towelState == TowelState.Squeezing)
        {
            SetTowelStateSqueezed();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterSurface")
        {
            SetTowelStateWet(); // BowlWaterSurfaceに触れたら濡れた状態
        }
    }

    private void OnCollisionEnter(Collision other)  //下の奴から書き換えた（山田）
    {
        if(other.gameObject.name == "DeskSurface" || other.gameObject.name == "Body1_Surface" || other.gameObject.name == "Body3:4_Surface")
        {
            isOnTable = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.name == "DeskSurface" || other.gameObject.name == "Body1_Surface" || other.gameObject.name == "Body3:4_Surface")
        {
            isOnTable = false;
        }
    }

   /* private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Surface"))
        {
            isOnTable = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Surface"))
        {
            isOnTable = false;
        }
    }*/

    private void SetTowelStateWet()
    {
        _towelState = TowelState.Wet; // 状態をWetに設定
        UpdateTowelAppearance(); // 外見を更新
    }

    private void SetTowelStateOnTable()
    {
        _towelState = TowelState.OnTable; // 状態をOnTableに設定
        UpdateTowelAppearance(); // 外見を更新
    }

    private void SetTowelStateGrabbed()
    {
        _towelState = TowelState.Grabbed; // 状態をGrabbedに設定
        UpdateTowelAppearance(); // 外見を更新
    }

    private void SetTowelStateSqueezing()
    {
        _towelState = TowelState.Squeezing; // 状態をSqueezingに設定
        UpdateTowelAppearance(); // 外見を更新
    }

    private void SetTowelStateSqueezed()
    {
        _towelState = TowelState.Squeezed; //状態をSqueezedに設定
        UpdateTowelAppearance(); //外見を更新
    }

    private void UpdateTowelAppearance()
    {
        switch (_towelState)
        {
            case TowelState.Wet:
                if (wetMaterial != null)
                    _meshRenderer.material = wetMaterial;
                if (wetMesh != null && _meshFilter != null)
                    _meshFilter.mesh = wetMesh;
                break;

            case TowelState.OnTable:
                if (onTableMaterial != null)
                    _meshRenderer.material = onTableMaterial;
                if (onTableMesh != null && _meshFilter != null)
                    _meshFilter.mesh = onTableMesh;
                break;
            
            case TowelState.Grabbed:
                if (grabbedMaterial != null)
                    _meshRenderer.material = grabbedMaterial;
                if (grabbedMesh != null && _meshFilter != null)
                    _meshFilter.mesh = grabbedMesh;
                break;

            case TowelState.Squeezing:
                if (squeezingMaterial != null)
                    _meshRenderer.material = squeezingMaterial;
                if (squeezingMesh != null && _meshFilter != null)
                    _meshFilter.mesh = squeezingMesh;
                break;

            case TowelState.Squeezed:
                if (squeezedMaterial != null)
                    _meshRenderer.material = squeezedMaterial;
                if (squeezedMesh != null && _meshFilter != null)
                    _meshFilter.mesh = squeezedMesh;
                break;

                // 他の状態に応じた処理も追加可能（GrabbedやSqueezedなど）
        }
    }
}
