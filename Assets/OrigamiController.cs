using UnityEngine;

public class OrigamiController : MonoBehaviour
{
    public GameObject origamiObject;    // Origami���A�^�b�`�����I�u�W�F�N�g
    public GameObject anchor;           // Anchor���z�u���ꂽ�e�I�u�W�F�N�g

    public float maxScale = 2.0f;        // �X�P�[���̍ő�l
    public float minScale = 0.5f;         // �X�P�[���̍ŏ��l

    private Origami origamiScript;

    private void Start()
    {
        if (origamiObject == null || anchor == null)
        {
            Debug.LogError("Origami Object or Anchor is not assigned.");
            return;
        }

        // Origami�X�N���v�g���擾
        origamiScript = origamiObject.GetComponent<Origami>();
        if (origamiScript == null)
        {
            Debug.LogError("Origami script is not attached to the assigned object.");
        }

        // origamiObject��anchor�̎q�Ƃ��Đݒ�
        origamiObject.transform.SetParent(anchor.transform);
    }

    private void Update()
    {
        if (origamiObject == null || anchor == null || origamiScript == null) return;

        // �X�P�[���Ɋ�Â���Folding�̒l���X�V
        UpdateFoldingBasedOnScale();

        // Anchor�̍��W��ύX
        UpdateAnchorPosition();
    }

    /// <summary>
    /// �X�P�[���Ɋ�Â���Folding�̒l���X�V
    /// </summary>
    private void UpdateFoldingBasedOnScale()
    {
        float currentScale = transform.localScale.x; // X���X�P�[������ɂ���
        // �X�P�[���Ɋ�Â�Folding�̌v�Z�i�l�𔽓]�j
        float foldingValue = 1.0f - Mathf.InverseLerp(minScale, maxScale, currentScale);
        origamiScript.Folding = foldingValue;
    }

    /// <summary>
    /// Anchor�̈ʒu��ύX�i�e�q�֌W�𗘗p����origamiObject���ړ��j
    /// </summary>
    private void UpdateAnchorPosition()
    {
        // �Ⴆ�΁AAnchor�̈ʒu�𓮂���
        anchor.transform.position = transform.position;  // ���̃I�u�W�F�N�g�Ɠ����ʒu��Anchor���ړ�������
        anchor.transform.rotation = transform.rotation;  // ��]�����킹��ꍇ
    }

    private void OnValidate()
    {
        // �G�f�B�^�[�Ńp�����[�^���ύX���ꂽ�ꍇ�̍X�V����
        if (origamiScript != null)
        {
            UpdateFoldingBasedOnScale();
        }

        if (origamiObject != null && anchor != null)
        {
            UpdateAnchorPosition();
        }
    }
}
