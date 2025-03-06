using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine.Events;

public class BathTowelManager : MonoBehaviour
{
    [System.Serializable]
    public class SnapPhase
    {
        [Tooltip("このフェイズで把持状態を判定するための Grabbable コンポーネント（親オブジェクト。Rigidbody付き）")]
        public Grabbable grabbable;

        [Header("Target Settings")]
        [Tooltip("スナップ成立後の表示位置。スナップ表示の座標として利用されます。")]
        public Transform targetObject;

        [Tooltip("閾値判定用の基準位置。未設定の場合は targetObject の位置が使用されます。")]
        public Transform thresholdReference;

        [Tooltip("把持オブジェクト側の閾値判定用基準位置。未設定の場合は grabbable の位置が使用されます。")]
        public Transform grabbableThresholdReference;

        [Tooltip("対象オブジェクトの Renderer（複数可）。インスペクタからアタッチしてください。")]
        public Renderer[] targetRenderers;

        [Tooltip("ゴースト表示を開始する距離の閾値")]
        public float distanceThreshold = 0.1f;

        [Header("Preview Settings")]
        [Tooltip("プレビュー用ゴーストのプレハブ")]
        public GameObject previewGhost;

        [Header("Snap Settings")]
        [Tooltip("スナップ成立時に表示するオブジェクト（シーン上に配置、初期は非アクティブ）")]
        public GameObject snapDisplayPrefab;

        [Header("Phase Events")]
        [Tooltip("把持開始時に実行する処理")]
        public UnityEvent onPhaseGrabbed = new UnityEvent();
        [Tooltip("把持中（更新処理）に実行する処理")]
        public UnityEvent onPhaseUpdated = new UnityEvent();
        [Tooltip("把持解除時に実行する処理")]
        public UnityEvent onPhaseReleased = new UnityEvent();
        [Tooltip("スナップ成立時に実行する処理")]
        public UnityEvent onPhaseSnap = new UnityEvent();

        [Header("Grabbable Object Settings")]
        [Tooltip("把持時に表示するグラバブルオブジェクトの Mesh")]
        public Mesh grabbedMesh;
        [Tooltip("把持していないときに表示するグラバブルオブジェクトの Mesh")]
        public Mesh normalMesh;
        [Tooltip("閾値内で把持解除したときに表示するグラバブルオブジェクトの Mesh")]
        public Mesh thresholdReleaseMesh;
        [Tooltip("グラバブルオブジェクトの MeshFilter への参照")]
        public MeshFilter grabbableMeshFilter;

        [Header("Preconfigured Colliders")]
        [Tooltip("把持時に使用する BoxCollider（GrabbableObjectの子オブジェクト、名前を付けたもの）")]
        public BoxCollider grabCollider;
        [Tooltip("スナップ成立後に使用する BoxCollider（GrabbableObjectの子オブジェクト、名前を付けたもの）")]
        public BoxCollider snapCollider;

        [Tooltip("閾値内で把持解除した場合に、グラバブルオブジェクトを移動させるターゲットの Transform")]
        public Transform grabbableReleaseTarget;
    }

    // 各フェイズの実行状態を保持する内部クラス
    private class PhaseState
    {
        public SnapPhase phase;
        public bool isGrabbed = false;
        public bool isCompleted = false; // このフェイズがスナップ成立したか
        // 把持中に生成されたプレビューゴーストのインスタンス
        public GameObject ghostInstance = null;
        // 把持開始時の初期位置と回転を記録
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        // 対象Rendererの元の色を保持する辞書
        public Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    }

    [Header("Snap Phases Settings")]
    [Tooltip("複数のスナップフェイズ設定を登録してください")]
    public SnapPhase[] snapPhases;

    // 各フェイズの実行状態リスト（フェイズは順次処理します）
    private List<PhaseState> phaseStates = new List<PhaseState>();

    // 現在処理中のフェイズインデックス
    private int currentPhaseIndex = 0;

    private void Awake()
    {
        // 各フェイズごとに状態を初期化
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
        // 全フェイズ完了なら何もしない
        if (currentPhaseIndex >= phaseStates.Count)
            return;

        PhaseState state = phaseStates[currentPhaseIndex];
        SnapPhase phase = state.phase;
        // 必要な設定がなければスキップ
        if (phase.grabbable == null || phase.targetObject == null || phase.previewGhost == null)
            return;

        // 現在のフェイズのみ処理（他のフェイズは無視）
        if (phase.grabbable.SelectingPointsCount > 0)
        {
            if (!state.isGrabbed)
            {
                state.isGrabbed = true;
                // 対象Rendererの元の色を保存
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
        else if (state.isGrabbed) // 把持解除時
        {
            state.isGrabbed = false;
            phase.onPhaseReleased.Invoke();
            OnReleased(state);
        }
    }

    /// <summary>
    /// 把持開始時の処理：初期位置・回転の記録とグラバブルオブジェクトの Mesh 更新、ならびに把持用Colliderを適用
    /// </summary>
    private void OnGrabbed(PhaseState state)
    {
        SnapPhase phase = state.phase;
        state.initialPosition = phase.grabbable.transform.position;
        state.initialRotation = phase.grabbable.transform.rotation;
        // 把持中は grabbedMesh を適用
        if (phase.grabbableMeshFilter != null)
        {
            phase.grabbableMeshFilter.mesh = phase.grabbedMesh;
        }
        // 把持時は、targetObjectはFadeモードで透明（α = 0）にする
        SetGrabbableTransparency(phase, false);
        ActivateGrabCollider(phase);
        UpdatePreview(state);
    }

    /// <summary>
    /// 指定したフェイズの grabCollider を有効化し、GrabbableObject の子にある他の Collider を無効化します。
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
    /// スナップ成立時に、指定したフェイズの snapCollider を有効化し、他の Collider を無効化します。
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
    /// シェーダーのレンダリングモードをFadeに切り替えるためのヘルパー関数
    /// </summary>
    private void SetMaterialTransparency(Material mat, bool transparent)
    {
        if (transparent)
        {
            // Fadeモードへ切り替え
            // Unity Standard ShaderにおけるFadeモードは_Modeを2に設定します
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
            // 不透明モードへ切り替え（Opaque）
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
    /// グラバブルオブジェクトの MeshRenderer をFade／不透明に切り替える処理
    /// ※grabbableMeshFilter の GameObject にある MeshRenderer を使用します。
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
    /// 対象オブジェクトの Renderer をFade／不透明に切り替える処理
    /// targetObject（およびインスペクタからアタッチした targetRenderers ）について、
    /// 通常時は不透明（α = 1）、把持中またはスナップ完了時はFadeモードで透明（α = 0）とします。
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
    /// 把持中の処理：
    /// grabbableオブジェクト側の閾値判定も行うため、grabbableThresholdReference が設定されていればその位置を使用します。
    /// thresholdReference（未設定の場合は targetObject）の位置との距離が閾値以下なら、
    /// 対象およびグラバブルオブジェクトをFadeモードで透明化（α = 0）し、プレビューゴーストを生成／更新する
    /// </summary>
    private void UpdatePreview(PhaseState state)
    {
        SnapPhase phase = state.phase;
        Vector3 targetRefPosition = (phase.thresholdReference != null) ? phase.thresholdReference.position : phase.targetObject.position;
        Vector3 grabbableRefPosition = (phase.grabbableThresholdReference != null) ? phase.grabbableThresholdReference.position : phase.grabbable.transform.position;
        float distance = Vector3.Distance(grabbableRefPosition, targetRefPosition);

        if (distance <= phase.distanceThreshold)
        {
            // 閾値内では targetObject をFadeモードで透明（α = 0）に
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
            // 閾値外では targetObject は不透明（α = 1）に戻す
            SetTargetTransparency(state, false);
            SetGrabbableTransparency(phase, false);
        }
    }

    /// <summary>
    /// 把持解除時の処理：
    /// ・thresholdReference（未設定の場合は targetObject）の位置との距離が閾値以下かつプレビューゴーストが存在していればスナップ成立とする
    ///   → 対象オブジェクト（targetObject）はFadeモードの透明（α = 0）のままとする
    ///   → snapDisplayPrefab を所定の位置に表示
    ///   → GrabbableObject は grabbableReleaseTarget の位置へ移動し、snapCollider を適用して再表示する
    ///   → このフェイズを完了状態にし、次のフェイズへ遷移する
    /// ・スナップ条件を満たさなければ、初期位置・normalMesh に戻す
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

            // スナップ成立後も targetObject はFadeモードで透明（α = 0）のままとする
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
            // 閾値外の場合は targetObject は不透明（α = 1）に戻す
            SetTargetTransparency(state, false);
        }
    }
}
