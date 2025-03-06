using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine.Events;

public class BathTowelManager : MonoBehaviour
{
    [System.Serializable]
    public class SnapPhase
    {
        [Tooltip("���̃t�F�C�Y�Ŕc����Ԃ𔻒肷�邽�߂� Grabbable �R���|�[�l���g�i�e�I�u�W�F�N�g�BRigidbody�t���j")]
        public Grabbable grabbable;

        [Header("Target Settings")]
        [Tooltip("�X�i�b�v������̕\���ʒu�B�X�i�b�v�\���̍��W�Ƃ��ė��p����܂��B")]
        public Transform targetObject;

        [Tooltip("臒l����p�̊�ʒu�B���ݒ�̏ꍇ�� targetObject �̈ʒu���g�p����܂��B")]
        public Transform thresholdReference;

        [Tooltip("�c���I�u�W�F�N�g����臒l����p��ʒu�B���ݒ�̏ꍇ�� grabbable �̈ʒu���g�p����܂��B")]
        public Transform grabbableThresholdReference;

        [Tooltip("�ΏۃI�u�W�F�N�g�� Renderer�i�����j�B�C���X�y�N�^����A�^�b�`���Ă��������B")]
        public Renderer[] targetRenderers;

        [Tooltip("�S�[�X�g�\�����J�n���鋗����臒l")]
        public float distanceThreshold = 0.1f;

        [Header("Preview Settings")]
        [Tooltip("�v���r���[�p�S�[�X�g�̃v���n�u")]
        public GameObject previewGhost;

        [Header("Snap Settings")]
        [Tooltip("�X�i�b�v�������ɕ\������I�u�W�F�N�g�i�V�[����ɔz�u�A�����͔�A�N�e�B�u�j")]
        public GameObject snapDisplayPrefab;

        [Header("Phase Events")]
        [Tooltip("�c���J�n���Ɏ��s���鏈��")]
        public UnityEvent onPhaseGrabbed = new UnityEvent();
        [Tooltip("�c�����i�X�V�����j�Ɏ��s���鏈��")]
        public UnityEvent onPhaseUpdated = new UnityEvent();
        [Tooltip("�c���������Ɏ��s���鏈��")]
        public UnityEvent onPhaseReleased = new UnityEvent();
        [Tooltip("�X�i�b�v�������Ɏ��s���鏈��")]
        public UnityEvent onPhaseSnap = new UnityEvent();

        [Header("Grabbable Object Settings")]
        [Tooltip("�c�����ɕ\������O���o�u���I�u�W�F�N�g�� Mesh")]
        public Mesh grabbedMesh;
        [Tooltip("�c�����Ă��Ȃ��Ƃ��ɕ\������O���o�u���I�u�W�F�N�g�� Mesh")]
        public Mesh normalMesh;
        [Tooltip("臒l���Ŕc�����������Ƃ��ɕ\������O���o�u���I�u�W�F�N�g�� Mesh")]
        public Mesh thresholdReleaseMesh;
        [Tooltip("�O���o�u���I�u�W�F�N�g�� MeshFilter �ւ̎Q��")]
        public MeshFilter grabbableMeshFilter;

        [Header("Preconfigured Colliders")]
        [Tooltip("�c�����Ɏg�p���� BoxCollider�iGrabbableObject�̎q�I�u�W�F�N�g�A���O��t�������́j")]
        public BoxCollider grabCollider;
        [Tooltip("�X�i�b�v������Ɏg�p���� BoxCollider�iGrabbableObject�̎q�I�u�W�F�N�g�A���O��t�������́j")]
        public BoxCollider snapCollider;

        [Tooltip("臒l���Ŕc�����������ꍇ�ɁA�O���o�u���I�u�W�F�N�g���ړ�������^�[�Q�b�g�� Transform")]
        public Transform grabbableReleaseTarget;
    }

    // �e�t�F�C�Y�̎��s��Ԃ�ێ���������N���X
    private class PhaseState
    {
        public SnapPhase phase;
        public bool isGrabbed = false;
        public bool isCompleted = false; // ���̃t�F�C�Y���X�i�b�v����������
        // �c�����ɐ������ꂽ�v���r���[�S�[�X�g�̃C���X�^���X
        public GameObject ghostInstance = null;
        // �c���J�n���̏����ʒu�Ɖ�]���L�^
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        // �Ώ�Renderer�̌��̐F��ێ����鎫��
        public Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    }

    [Header("Snap Phases Settings")]
    [Tooltip("�����̃X�i�b�v�t�F�C�Y�ݒ��o�^���Ă�������")]
    public SnapPhase[] snapPhases;

    // �e�t�F�C�Y�̎��s��ԃ��X�g�i�t�F�C�Y�͏����������܂��j
    private List<PhaseState> phaseStates = new List<PhaseState>();

    // ���ݏ������̃t�F�C�Y�C���f�b�N�X
    private int currentPhaseIndex = 0;

    private void Awake()
    {
        // �e�t�F�C�Y���Ƃɏ�Ԃ�������
        foreach (var phase in snapPhases)
        {
            PhaseState state = new PhaseState
            {
                phase = phase,
                isGrabbed = false,
                ghostInstance = null,
                isCompleted = false
            };
            phaseStates.Add(state);
        }
    }

    private void Update()
    {
        // �S�t�F�C�Y�����Ȃ牽�����Ȃ�
        if (currentPhaseIndex >= phaseStates.Count)
            return;

        PhaseState state = phaseStates[currentPhaseIndex];
        SnapPhase phase = state.phase;
        // �K�v�Ȑݒ肪�Ȃ���΃X�L�b�v
        if (phase.grabbable == null || phase.targetObject == null || phase.previewGhost == null)
            return;

        // ���݂̃t�F�C�Y�̂ݏ����i���̃t�F�C�Y�͖����j
        if (phase.grabbable.SelectingPointsCount > 0)
        {
            if (!state.isGrabbed)
            {
                state.isGrabbed = true;
                // �Ώ�Renderer�̌��̐F��ۑ�
                if (phase.targetRenderers != null)
                {
                    state.originalColors.Clear();
                    foreach (Renderer r in phase.targetRenderers)
                    {
                        if (r != null)
                        {
                            state.originalColors[r] = r.material.color;
                        }
                    }
                }
                phase.onPhaseGrabbed.Invoke();
                OnGrabbed(state);
            }
            else
            {
                phase.onPhaseUpdated.Invoke();
                UpdatePreview(state);
            }
        }
        else if (state.isGrabbed) // �c��������
        {
            state.isGrabbed = false;
            phase.onPhaseReleased.Invoke();
            OnReleased(state);
        }
    }

    /// <summary>
    /// �c���J�n���̏����F�����ʒu�E��]�̋L�^�ƃO���o�u���I�u�W�F�N�g�� Mesh �X�V�A�Ȃ�тɔc���pCollider��K�p
    /// </summary>
    private void OnGrabbed(PhaseState state)
    {
        SnapPhase phase = state.phase;
        state.initialPosition = phase.grabbable.transform.position;
        state.initialRotation = phase.grabbable.transform.rotation;
        // �c������ grabbedMesh ��K�p
        if (phase.grabbableMeshFilter != null)
        {
            phase.grabbableMeshFilter.mesh = phase.grabbedMesh;
        }
        // �c�����́AtargetObject��Fade���[�h�œ����i�� = 0�j�ɂ���
        SetGrabbableTransparency(phase, false);
        ActivateGrabCollider(phase);
        UpdatePreview(state);
    }

    /// <summary>
    /// �w�肵���t�F�C�Y�� grabCollider ��L�������AGrabbableObject �̎q�ɂ��鑼�� Collider �𖳌������܂��B
    /// </summary>
    private void ActivateGrabCollider(SnapPhase phase)
    {
        foreach (BoxCollider bc in phase.grabbable.GetComponentsInChildren<BoxCollider>())
        {
            bc.gameObject.SetActive(false);
        }
        if (phase.grabCollider != null)
        {
            phase.grabCollider.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �X�i�b�v�������ɁA�w�肵���t�F�C�Y�� snapCollider ��L�������A���� Collider �𖳌������܂��B
    /// </summary>
    private void ActivateSnapCollider(SnapPhase phase)
    {
        foreach (BoxCollider bc in phase.grabbable.GetComponentsInChildren<BoxCollider>())
        {
            bc.gameObject.SetActive(false);
        }
        if (phase.snapCollider != null)
        {
            phase.snapCollider.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �V�F�[�_�[�̃����_�����O���[�h��Fade�ɐ؂�ւ��邽�߂̃w���p�[�֐�
    /// </summary>
    private void SetMaterialTransparency(Material mat, bool transparent)
    {
        if (transparent)
        {
            // Fade���[�h�֐؂�ւ�
            // Unity Standard Shader�ɂ�����Fade���[�h��_Mode��2�ɐݒ肵�܂�
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            Color col = mat.color;
            col.a = 0f;
            mat.color = col;
        }
        else
        {
            // �s�������[�h�֐؂�ւ��iOpaque�j
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
            Color col = mat.color;
            col.a = 1f;
            mat.color = col;
        }
    }

    /// <summary>
    /// �O���o�u���I�u�W�F�N�g�� MeshRenderer ��Fade�^�s�����ɐ؂�ւ��鏈��
    /// ��grabbableMeshFilter �� GameObject �ɂ��� MeshRenderer ���g�p���܂��B
    /// </summary>
    private void SetGrabbableTransparency(SnapPhase phase, bool transparent)
    {
        if (phase.grabbableMeshFilter != null)
        {
            MeshRenderer mr = phase.grabbableMeshFilter.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                foreach (var mat in mr.materials)
                {
                    SetMaterialTransparency(mat, transparent);
                }
            }
        }
    }

    /// <summary>
    /// �ΏۃI�u�W�F�N�g�� Renderer ��Fade�^�s�����ɐ؂�ւ��鏈��
    /// targetObject�i����уC���X�y�N�^����A�^�b�`���� targetRenderers �j�ɂ��āA
    /// �ʏ펞�͕s�����i�� = 1�j�A�c�����܂��̓X�i�b�v��������Fade���[�h�œ����i�� = 0�j�Ƃ��܂��B
    /// </summary>
    private void SetTargetTransparency(PhaseState state, bool transparent)
    {
        SnapPhase phase = state.phase;
        if (phase.targetRenderers != null)
        {
            foreach (Renderer r in phase.targetRenderers)
            {
                if (r != null)
                {
                    foreach (var mat in r.materials)
                    {
                        SetMaterialTransparency(mat, transparent);
                    }
                }
            }
        }
    }

    /// <summary>
    /// �c�����̏����F
    /// grabbable�I�u�W�F�N�g����臒l������s�����߁AgrabbableThresholdReference ���ݒ肳��Ă���΂��̈ʒu���g�p���܂��B
    /// thresholdReference�i���ݒ�̏ꍇ�� targetObject�j�̈ʒu�Ƃ̋�����臒l�ȉ��Ȃ�A
    /// �Ώۂ���уO���o�u���I�u�W�F�N�g��Fade���[�h�œ������i�� = 0�j���A�v���r���[�S�[�X�g�𐶐��^�X�V����
    /// </summary>
    private void UpdatePreview(PhaseState state)
    {
        SnapPhase phase = state.phase;
        Vector3 targetRefPosition = (phase.thresholdReference != null) ? phase.thresholdReference.position : phase.targetObject.position;
        Vector3 grabbableRefPosition = (phase.grabbableThresholdReference != null) ? phase.grabbableThresholdReference.position : phase.grabbable.transform.position;
        float distance = Vector3.Distance(grabbableRefPosition, targetRefPosition);

        if (distance <= phase.distanceThreshold)
        {
            // 臒l���ł� targetObject ��Fade���[�h�œ����i�� = 0�j��
            SetTargetTransparency(state, true);
            SetGrabbableTransparency(phase, true);
            if (state.ghostInstance == null)
            {
                state.ghostInstance = Instantiate(phase.previewGhost, phase.targetObject.position, phase.targetObject.rotation);
            }
            else
            {
                state.ghostInstance.transform.position = phase.targetObject.position;
                state.ghostInstance.transform.rotation = phase.targetObject.rotation;
            }
        }
        else
        {
            if (state.ghostInstance != null)
            {
                Destroy(state.ghostInstance);
                state.ghostInstance = null;
            }
            // 臒l�O�ł� targetObject �͕s�����i�� = 1�j�ɖ߂�
            SetTargetTransparency(state, false);
            SetGrabbableTransparency(phase, false);
        }
    }

    /// <summary>
    /// �c���������̏����F
    /// �EthresholdReference�i���ݒ�̏ꍇ�� targetObject�j�̈ʒu�Ƃ̋�����臒l�ȉ����v���r���[�S�[�X�g�����݂��Ă���΃X�i�b�v�����Ƃ���
    ///   �� �ΏۃI�u�W�F�N�g�itargetObject�j��Fade���[�h�̓����i�� = 0�j�̂܂܂Ƃ���
    ///   �� snapDisplayPrefab ������̈ʒu�ɕ\��
    ///   �� GrabbableObject �� grabbableReleaseTarget �̈ʒu�ֈړ����AsnapCollider ��K�p���čĕ\������
    ///   �� ���̃t�F�C�Y��������Ԃɂ��A���̃t�F�C�Y�֑J�ڂ���
    /// �E�X�i�b�v�����𖞂����Ȃ���΁A�����ʒu�EnormalMesh �ɖ߂�
    /// </summary>
    private void OnReleased(PhaseState state)
    {
        SnapPhase phase = state.phase;
        Vector3 targetRefPosition = (phase.thresholdReference != null) ? phase.thresholdReference.position : phase.targetObject.position;
        Vector3 grabbableRefPosition = (phase.grabbableThresholdReference != null) ? phase.grabbableThresholdReference.position : phase.grabbable.transform.position;
        float distance = Vector3.Distance(grabbableRefPosition, targetRefPosition);

        if (distance <= phase.distanceThreshold && state.ghostInstance != null)
        {
            Destroy(state.ghostInstance);
            state.ghostInstance = null;

            // �X�i�b�v������� targetObject ��Fade���[�h�œ����i�� = 0�j�̂܂܂Ƃ���
            SetTargetTransparency(state, true);

            if (phase.snapDisplayPrefab != null)
            {
                phase.snapDisplayPrefab.transform.position = phase.targetObject.position;
                phase.snapDisplayPrefab.transform.rotation = phase.targetObject.rotation;
                phase.snapDisplayPrefab.SetActive(true);
            }
            phase.onPhaseSnap.Invoke();

            if (phase.grabbable != null && phase.grabbableReleaseTarget != null)
            {
                phase.grabbable.transform.position = phase.grabbableReleaseTarget.position;
                phase.grabbable.transform.rotation = phase.grabbableReleaseTarget.rotation;
                phase.grabbable.gameObject.SetActive(true);
            }
            if (phase.snapCollider != null)
            {
                ActivateSnapCollider(phase);
            }
            SetGrabbableTransparency(phase, false);
            if (phase.grabbableMeshFilter != null && phase.thresholdReleaseMesh != null)
            {
                phase.grabbableMeshFilter.mesh = phase.thresholdReleaseMesh;
            }
            if (phase.grabbableMeshFilter != null)
            {
                GameObject gm = phase.grabbableMeshFilter.gameObject;
                if (!gm.activeSelf)
                {
                    gm.SetActive(true);
                }
                MeshRenderer mr = gm.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    foreach (var mat in mr.materials)
                    {
                        Color col = mat.color;
                        col.a = 1f;
                        mat.color = col;
                    }
                }
            }
            state.isCompleted = true;
            currentPhaseIndex++;
        }
        else
        {
            if (state.ghostInstance != null)
            {
                Destroy(state.ghostInstance);
                state.ghostInstance = null;
            }
            if (phase.grabbable != null)
            {
                phase.grabbable.transform.position = state.initialPosition;
                phase.grabbable.transform.rotation = state.initialRotation;
            }
            if (phase.grabbableMeshFilter != null)
            {
                phase.grabbableMeshFilter.mesh = phase.normalMesh;
            }
            SetGrabbableTransparency(phase, false);
            // 臒l�O�̏ꍇ�� targetObject �͕s�����i�� = 1�j�ɖ߂�
            SetTargetTransparency(state, false);
        }
    }
}
