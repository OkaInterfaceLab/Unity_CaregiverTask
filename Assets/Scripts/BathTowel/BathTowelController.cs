using UnityEngine;
using Oculus.Interaction;

public class BathTowelController : MonoBehaviour
{
    [Header("Grabbable Settings")]
    [SerializeField] private Grabbable grabbable;

    [Header("Display Settings")]
    [Tooltip("���̃��b�V����\�����邽�߂� MeshRenderer�B�c�����͔�\���ɂ��܂�")]
    [SerializeField] private MeshRenderer originalMeshRenderer;
    [Tooltip("�c�����ꂽ�Ƃ��ɕ\������I�u�W�F�N�g�B���I�u�W�F�N�g�̈ʒu�E��]�ɒǏ]���܂�")]
    [SerializeField] private GameObject grabbedDisplayObject;

    private bool isGrabbed = false;

    private void Awake()
    {
        // originalMeshRenderer �����ݒ�Ȃ�A���̃I�u�W�F�N�g�� MeshRenderer ���擾
        if (originalMeshRenderer == null)
        {
            originalMeshRenderer = GetComponent<MeshRenderer>();
        }
        // ������ԁF���̃��b�V���͕\���A�c�����\���p�I�u�W�F�N�g�͔�\��
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

        // �c�����̏ꍇ�AgrabbedDisplayObject �̈ʒu�Ɖ�]�����I�u�W�F�N�g�ɒǏ]������
        if (isGrabbed && grabbedDisplayObject != null)
        {
            grabbedDisplayObject.transform.position = transform.position;
            grabbedDisplayObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// �c���J�n���̏����F���̃��b�V�����\���ɂ��A�c���\���p�I�u�W�F�N�g��L����
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
    /// �c���������̏����F���̃��b�V�����ĕ\�����A�c���\���p�I�u�W�F�N�g���\���ɂ���
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
