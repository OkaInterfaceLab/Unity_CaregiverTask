using Oculus.Interaction;
using UnityEngine;

public class PaperStateController : MonoBehaviour
{
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private MeshRenderer _meshRenderer; // �}�e���A���ύX�p
    [SerializeField] private MeshFilter _meshFilter; // ���b�V���ύX�p
    [SerializeField] private MeshCollider _meshCollider; // �R���C�_�[�ύX�p

    [SerializeField] private Material openMaterial; // open��ԗp�̃}�e���A��
    [SerializeField] private Mesh openMesh; // open��ԗp�̃��b�V��
    [SerializeField] private Vector3 openScale = Vector3.one; // open��ԗp�̃X�P�[��

    [SerializeField] private Material closeMaterial; // close��ԗp�̃}�e���A��
    [SerializeField] private Mesh closeMesh; // close��ԗp�̃��b�V��
    [SerializeField] private Vector3 closeScale = Vector3.one; // close��ԗp�̃X�P�[��

    public enum PaperState
    {
        Open,  // ����Œ͂�ł�����
        Close  // �Ў�Œ͂�ł�����
    }

    [HideInInspector] public PaperState _paperState; // ���݂�Paper�̏��

    void Start()
    {
        _paperState = PaperState.Close; // ������Ԃ�Close�ɐݒ�
        UpdatePaperAppearance(); // ������Ԃɉ����ĊO�����X�V
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
            _paperState = PaperState.Open; // ��Ԃ�Open�ɐݒ�
            UpdatePaperAppearance(); // �O�����X�V
        }
    }

    private void SetPaperStateClose()
    {
        if (_paperState != PaperState.Close)
        {
            _paperState = PaperState.Close; // ��Ԃ�Close�ɐݒ�
            UpdatePaperAppearance(); // �O�����X�V
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
                        _meshCollider.sharedMesh = openMesh; // �R���C�_�[�̃��b�V����ύX
                }
                transform.localScale = openScale; // �X�P�[����ύX
                break;

            case PaperState.Close:
                if (closeMaterial != null)
                    _meshRenderer.material = closeMaterial;
                if (closeMesh != null)
                {
                    _meshFilter.mesh = closeMesh;
                    if (_meshCollider != null)
                        _meshCollider.sharedMesh = closeMesh; // �R���C�_�[�̃��b�V����ύX
                }
                transform.localScale = closeScale; // �X�P�[����ύX
                break;
        }
    }
}
