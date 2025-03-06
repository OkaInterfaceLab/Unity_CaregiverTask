using UnityEngine;

public class ObjectPlacement : MonoBehaviour
{
    // ���C�̒���
    [SerializeField]
    private float rayLength = 0.20f;

    // ���C�L���X�g�Ō��o���郌�C���[
    [SerializeField]
    private LayerMask layerMask = Physics.DefaultRaycastLayers;

    // ���C���q�b�g�����ʒu�Ƀ��[�v�����邩�ǂ���
    [SerializeField]
    private bool warpToHitPoint = true;

    // �q�b�g�ʒu�ɕ\������}�[�J�[�̃v���n�u
    [SerializeField]
    private GameObject _markerPrefab;

    // �}�[�J�[�̃C���X�^���X
    private GameObject _markerInstance;

    // �I�u�W�F�N�g�̃R���C�_�[
    [SerializeField]
    private Collider objectCollider;

    void Awake()
    {
        // �I�u�W�F�N�g�̃R���C�_�[���擾
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider>();
        }

        // �}�[�J�[�̃C���X�^���X���쐬
        if (_markerPrefab != null)
        {
            _markerInstance = Instantiate(_markerPrefab);
            _markerInstance.SetActive(false); // �ŏ��͔�\��
        }
    }

    void Update()
    {
        // ���C�̔��ˈʒu�ƕ���
        Vector3 origin = transform.position;
        Vector3 direction = -transform.up; // �I�u�W�F�N�g�̉�����

        // ���C�L���X�g�̎��s
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, rayLength, layerMask))
        {
            // ���C���q�b�g�����ꍇ�A�}�[�J�[��\��
            if (_markerInstance != null)
            {
                _markerInstance.transform.position = hitInfo.point;
                _markerInstance.SetActive(true);
            }

            // ���[�v���L���ł���΁A�I�u�W�F�N�g���ړ�
            if (warpToHitPoint)
            {
                // �I�u�W�F�N�g�̒�ʂ܂ł̋������擾
                float distanceToBottom = GetDistanceToBottom();

                // �I�u�W�F�N�g���q�b�g�ʒu�Ɉړ��i��ʂ��ڂ���悤�Ɂj
                transform.position = hitInfo.point + hitInfo.normal * distanceToBottom;

                // �I�u�W�F�N�g�𕽖ʂɒ���������
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hitInfo.normal) * transform.rotation;
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // ���C���q�b�g���Ȃ������ꍇ�A�}�[�J�[���\��
            if (_markerInstance != null)
            {
                _markerInstance.SetActive(false);
            }
        }
    }

    // �I�u�W�F�N�g�̒��S�����ʂ܂ł̋������v�Z���郁�\�b�h
    private float GetDistanceToBottom()
    {
        // �R���C�_�[�̃o�E���f�B���O�{�b�N�X���擾
        Bounds bounds = objectCollider.bounds;

        // ���[�J�����W�n�ł̒��S�ƃG�N�X�e���g���v�Z
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localExtents = bounds.extents;

        // ���[�J��Y���ɉ�������ʂ܂ł̋������v�Z
        float distance = localCenter.y - localExtents.y;

        // ���̒l�𓾂邽�߂ɕ����𔽓]
        distance = -distance;

        return distance;
    }

    // �I�u�W�F�N�g���I�����ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void OnSelect()
    {
        warpToHitPoint = false; // �͂�ł���Ԃ̓��[�v�𖳌���
    }

    // �I�u�W�F�N�g�̑I�����������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void OnUnselect()
    {
        warpToHitPoint = true; // �������Ƃ��Ƀ��[�v���ēx�L����
    }
}
