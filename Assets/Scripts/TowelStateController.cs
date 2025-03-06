using Oculus.Interaction;
using UnityEngine;
using System.Collections; //�R���[�`���̂��߂ɒǋL�i�R�c�j

public class TowelStateController : MonoBehaviour
{
    [SerializeField] private Grabbable _grabbable; 
    [SerializeField] private MeshRenderer _meshRenderer; // �}�e���A���ύX�p
    [SerializeField] private MeshFilter _meshFilter; // ���b�V���ύX�p
    [SerializeField] private Material wetMaterial; // �G�ꂽ��ԗp�̐V�����}�e���A��
    [SerializeField] private Mesh wetMesh; // �G�ꂽ��ԗp�̐V�������b�V��
    [SerializeField] private Material grabbedMaterial; // grab��ԗp�̐V�����}�e���A��
    [SerializeField] private Mesh grabbedMesh; // grab��ԗp�̐V�������b�V��
    [SerializeField] private Material onTableMaterial; // �e�[�u����ɂ����ԗp�̃}�e���A��
    [SerializeField] private Mesh onTableMesh; // �e�[�u����ɂ����ԗp�̃��b�V��
    [SerializeField] private Material squeezingMaterial; // squeezing��ԗp�̃}�e���A��
    [SerializeField] private Mesh squeezingMesh; // squeezing��ԗp�̃��b�V��

    [SerializeField] private Material squeezedMaterial; //squeezed��ԗp�̃}�e���A���i�R�c�j
    [SerializeField] private Mesh squeezedMesh; //squeezed��ԗp�̃��b�V���i�R�c�j
    [SerializeField] private float squeezeTime = 3.0f; //�i��b���̕ϐ��w��i�R�c�j

    private bool isOnTable = false;

    public enum TowelState
    {
        OnTable, // �e�[�u���̏�ɂ�����
        Grabbed, // �͂܂�Ă�����
        Squeezing, // �i���Ă�����
        Wet, // �G��Ă�����
        Squeezed //�i��ꂽ��̏��
    }

    [HideInInspector] public TowelState _towelState; // ���݂̃^�I���̏��

    void Start()
    {
        _towelState = TowelState.OnTable; // ������Ԃ��e�[�u���̏�ɐݒ�
        UpdateTowelAppearance(); // ������Ԃɉ����ĊO�����X�V
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
                    StartCoroutine(ToSqueezed(squeezeTime)); //�t�������i�R�c�j
                    break;
            }
        }
        else if(_towelState == TowelState.Grabbed || _towelState == TowelState.OnTable)  //.Wet����.Grabbed�܂���.OnTable�ցi�R�c�j
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
        else if(_towelState == TowelState.Squeezing) //�t�������i�R�c�j
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
        else if(_towelState == TowelState.Squeezed)   //�t�������i�R�c�j
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

    //��莞�Ԍ��Squeezed��ԂɈڍs����R���[�`���i�R�c�j
    private IEnumerator ToSqueezed(float delaySeconds)
    {
        // Squeezing��ԂłȂ���ΏI��
        if(_towelState != TowelState.Squeezing)
            yield break;

        // �w��b���ҋ@
        yield return new WaitForSeconds(delaySeconds);

        // ��Ԃ��܂�Squeezing�ł����Squeezed�ɕύX
        if(_towelState == TowelState.Squeezing)
        {
            SetTowelStateSqueezed();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterSurface")
        {
            SetTowelStateWet(); // BowlWaterSurface�ɐG�ꂽ��G�ꂽ���
        }
    }

    private void OnCollisionEnter(Collision other)  //���̓z���珑���������i�R�c�j
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
        _towelState = TowelState.Wet; // ��Ԃ�Wet�ɐݒ�
        UpdateTowelAppearance(); // �O�����X�V
    }

    private void SetTowelStateOnTable()
    {
        _towelState = TowelState.OnTable; // ��Ԃ�OnTable�ɐݒ�
        UpdateTowelAppearance(); // �O�����X�V
    }

    private void SetTowelStateGrabbed()
    {
        _towelState = TowelState.Grabbed; // ��Ԃ�Grabbed�ɐݒ�
        UpdateTowelAppearance(); // �O�����X�V
    }

    private void SetTowelStateSqueezing()
    {
        _towelState = TowelState.Squeezing; // ��Ԃ�Squeezing�ɐݒ�
        UpdateTowelAppearance(); // �O�����X�V
    }

    private void SetTowelStateSqueezed()
    {
        _towelState = TowelState.Squeezed; //��Ԃ�Squeezed�ɐݒ�
        UpdateTowelAppearance(); //�O�����X�V
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

                // ���̏�Ԃɉ������������ǉ��\�iGrabbed��Squeezed�Ȃǁj
        }
    }
}
