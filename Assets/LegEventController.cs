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

    private Quaternion targetInitialRot; // �^�[�Q�b�g�̏�����]
    private bool isGrab = false; //�͂܂�Ă��邩�ǂ���
    private float weightDecreaseSpeed = 2.0f; // IK weight�̌������x

    private float maxDistance;  // ����������ۑ�

    // Start is called before the first frame update
    void Start()
    {
        if (_multiRotationConstraint != null && _weightAnchor != null)
        {
            // �I�u�W�F�N�g��_weightAnchor�̏����������v�Z
            maxDistance = Vector3.Distance(_leg_target.transform.position, _weightAnchor.transform.position);
            _multiRotationConstraint.weight = 0.3f; // �ŏ��̃E�F�C�g��ݒ�
        }

        // �^�[�Q�b�g�̏�����]��ۑ�
        targetInitialRot = _leg_target.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (_multiRotationConstraint != null && _weightAnchor != null)
        {
            // ���݂̋������v�Z
            float distance = Vector3.Distance(_leg_target.transform.position, _weightAnchor.transform.position);

            // �������߂Â��ɂ�ăE�F�C�g��1�ɁA������0.3�ɋ߂Â���
            float weight = Mathf.Clamp(1f - (distance / maxDistance), 0.3f, 1f);

            // �v�Z�����E�F�C�g��MultiRotationConstraint�ɓK�p
            _multiRotationConstraint.weight = weight;
        }

        // �肪�͂܂�Ă��Ȃ��ꍇ�AIK�̃E�F�C�g�����X�Ɍ���������
        if (!isGrab)
        {
            // weight�����X��0�ɋ߂Â���
            if (_legIKConstraint.weight > 0f)
            {
                _legIKConstraint.weight = Mathf.Lerp(_legIKConstraint.weight, 0f, weightDecreaseSpeed * Time.deltaTime);
            }

            // �^�[�Q�b�g�̈ʒu�Ɖ�]����ɓ���
            _leg_target.transform.position = _foot.transform.position;
            _leg_target.transform.rotation = targetInitialRot;
        }
    }

    // ���͂񂾂Ƃ��̏���
    public void OnSelect()
    {
        _legIKConstraint.weight = 1f;
        isGrab = true;
    }

    // ��𗣂����Ƃ��̏���
    public void OnUnSelect()
    {
        isGrab = false;
    }
}
