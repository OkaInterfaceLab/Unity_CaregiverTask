using Oculus.Interaction;
using UnityEngine;

public class BowlHandlingController : MonoBehaviour
{
    [Header("Placement Preview Settings")]
    // �X�i�b�v��Ƃ��ẴS�[�X�g�\���p�v���n�u
    [SerializeField] private GameObject ghostModelBowl;
    // �d�˂�Ώۂ̃I�u�W�F�N�g�i��F�x�[�X�ƂȂ�{�E���j
    [SerializeField] private Transform placementTarget;
    // �X�i�b�v�\�ȋ��e����
    [SerializeField] private float distanceThreshold = 0.1f;
    // �X�i�b�v���̏�����I�t�Z�b�g�iplacementTarget �̃��[�J�� up �����j
    [SerializeField] private float snapOffset = 0.03f;

    private GameObject placementPreviewInstance;
    private Grabbable grabbable;
    private Rigidbody rb;
    private Rigidbody placementTargetRb;

    // ���̃{�E���̔c�����
    private bool isGrabbed = false;
    // �X�^�b�N��ԁi�X�i�b�v�����j���ǂ���
    private bool isStacked = false;

    // �X�^�b�N���ɋL�^����AplacementTarget �̃��[�J����Ԃł̂��̃I�u�W�F�N�g�Ƃ̑��΃I�t�Z�b�g
    private Vector3 stackingOffset;
    // placementTarget �ɑ΂��鑊�Ή�]
    private Quaternion stackingRotationOffset;

    private void Awake()
    {
        // �S�[�X�g���f���̃C���X�^���X�𐶐��i�����͔�\���j
        placementPreviewInstance = Instantiate(ghostModelBowl);
        placementPreviewInstance.SetActive(false);

        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();

        if (placementTarget != null)
            placementTargetRb = placementTarget.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // �c�����̏ꍇ
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

                // ����c���ɂȂ����ꍇ�̓X�^�b�N��Ԃ��������Ď��R�ړ�������
                if (isStacked && grabbable.SelectingPointsCount >= 2)
                {
                    // �K�v�ɉ����� Unstack() ���Ăяo��
                    // Unstack();
                }
            }
        }
        // �c��������
        else if (isGrabbed)
        {
            isGrabbed = false;
            OnReleased();
        }
        // �c�����Ă��Ȃ���ԂŃX�^�b�N��
        else if (isStacked)
        {
            transform.position = placementTarget.TransformPoint(stackingOffset);
            transform.rotation = placementTarget.rotation * stackingRotationOffset;
        }
    }

    // �S�[�X�g���f���i�X�i�b�v�ʒu�j�̍X�V����
    private void UpdatePlacementPreview()
    {
        if (placementTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, placementTarget.position);
        // �Ў�c�������e�������A���܂��X�^�b�N���Ă��Ȃ��ꍇ�̂݃S�[�X�g��\��
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

    // �c���J�n���̏����i�K�v�ɉ����������X�V�j
    private void OnGrabbed()
    {
        UpdatePlacementPreview();
        if (isStacked)
        {
            isStacked = false;
        }
    }

    // �c���������̏���
    private void OnReleased()
    {
        if (placementTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, placementTarget.position);

        // ���e�������Ȃ�X�i�b�v���ăX�^�b�N��ԂɈڍs
        if (distance <= distanceThreshold)
        {
            // placementTarget �̃��[�J�� up �����ɃI�t�Z�b�g���������ʒu�ɃX�i�b�v
            Vector3 snapPos = placementTarget.position + placementTarget.up * snapOffset;
            transform.position = snapPos;
            transform.rotation = placementTarget.rotation;

            // placementTarget �̃��[�J����ԂŁAthis�I�u�W�F�N�g�Ƃ̑��΃I�t�Z�b�g���L�^
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

    // �X�^�b�N��Ԃ̉��������i��F����c�����j
    private void Unstack()
    {
        isStacked = false;
        rb.isKinematic = false;
    }
}
