using System;
using Oculus.Interaction.Throw;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class TwoHandsGrab : PointableElement, IGrabbable, ITimeConsumer
{
    [Tooltip("A Two Grab...Transformer component, which should be attached to the grabbable object. If you set this property but also want to use one hand for grabs, you must set the One Grab Transformer property.")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _twoGrabTransformer = null;

    [Tooltip("The target transform of the Grabbable. If unassigned, \" +\r\n            \"the transform of this GameObject will be used.")]
    [SerializeField]
    private Transform _targetTransform;

    [Tooltip("The maximum number of grab points. Can be either -1 (unlimited), 1, or 2.")]
    [SerializeField, Min(2)]
    private int _maxGrabPoints = 2;

    [Header("Physics")]
    [SerializeField]
    [Tooltip("Use this rigidbody to control its physics properties while grabbing.")]
    private Rigidbody _rigidbody;
    [SerializeField]
    [Tooltip("Locks the referenced rigidbody to a kinematic while selected.")]
    private bool _kinematicWhileSelected = true;
    [SerializeField]
    [Tooltip("Applies throwing velocities to the rigidbody when fully released.")]
    private bool _throwWhenUnselected = true;

    public int MaxGrabPoints
    {
        get
        {
            return _maxGrabPoints;
        }
        set
        {
            _maxGrabPoints = value;
        }
    }

    public Transform Transform => _targetTransform;
    public List<Pose> GrabPoints => _selectingPoints;

    public new int SelectingPointsCount => _selectingPoints.Count;

    private Func<float> _timeProvider = () => Time.time;
    public void SetTimeProvider(Func<float> timeProvider)
    {
        _timeProvider = timeProvider;
        if (_throw != null)
        {
            _throw.SetTimeProvider(timeProvider);
        }
    }

    private ITransformer _activeTransformer = null;
    private ITransformer TwoGrabTransformer;

    private ThrowWhenUnselected _throw;

    private bool _isKinematicLocked = false;

    #region エディタ
    protected virtual void Reset()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        TwoGrabTransformer = _twoGrabTransformer as ITransformer;
    }

    protected override void Start()
    {
        this.BeginStart(ref _started, () => base.Start());

        if (_targetTransform == null)
        {
            _targetTransform = transform;
        }

        if (_twoGrabTransformer != null)
        {
            this.AssertField(TwoGrabTransformer, nameof(TwoGrabTransformer));
            TwoGrabTransformer.Initialize(this);
        }

        if (TwoGrabTransformer == null)
        {
            Debug.LogError("TwoGrabTransformerが設定されていません。Two Grab Transformerコンポーネントをアタッチしてください。");
        }

        if (_rigidbody != null && _throwWhenUnselected)
        {
            _throw = new ThrowWhenUnselected(_rigidbody, this);
            _throw.SetTimeProvider(this._timeProvider);
        }

        this.EndStart(ref _started);
    }

    protected override void OnDisable()
    {
        if (_started)
        {
            EndTransform();
        }

        base.OnDisable();
    }

    protected virtual void OnDestroy()
    {
        if (_throw != null)
        {
            _throw.Dispose();
            _throw = null;
        }
    }

    public override void ProcessPointerEvent(PointerEvent evt)
    {
        base.ProcessPointerEvent(evt);

        switch (evt.Type)
        {
            case PointerEventType.Select:
                if (_selectingPoints.Count == 2)
                {
                    BeginTransform();
                }
                break;
            case PointerEventType.Unselect:
                if (_selectingPoints.Count < 2)
                {
                    EndTransform();
                }
                break;
            case PointerEventType.Move:
                if (_selectingPoints.Count == 2)
                {
                    UpdateTransform();
                }
                break;
            case PointerEventType.Cancel:
                EndTransform();
                break;
        }
    }

    protected override void PointableElementUpdated(PointerEvent evt)
    {
        base.PointableElementUpdated(evt);

        UpdateKinematicLock(SelectingPointsCount == 2);
    }

    private void UpdateKinematicLock(bool isGrabbing)
    {
        if (_rigidbody == null || !_kinematicWhileSelected)
        {
            return;
        }

        if (!_isKinematicLocked && isGrabbing)
        {
            _isKinematicLocked = true;
            _rigidbody.LockKinematic();
        }
        else if (_isKinematicLocked && !isGrabbing)
        {
            _isKinematicLocked = false;
            _rigidbody.UnlockKinematic();
        }
    }

    private void BeginTransform()
    {
        if (_activeTransformer != null)
        {
            return;
        }

        if (_selectingPoints.Count != 2)
        {
            return;
        }

        _activeTransformer = TwoGrabTransformer;

        if (_activeTransformer == null)
        {
            return;
        }

        _activeTransformer.BeginTransform();
    }

    private void UpdateTransform()
    {
        if (_activeTransformer == null)
        {
            return;
        }

        _activeTransformer.UpdateTransform();
    }

    private void EndTransform()
    {
        if (_activeTransformer == null)
        {
            return;
        }

        _activeTransformer.EndTransform();
        _activeTransformer = null;
    }

    #region 注入

    public void InjectOptionalTwoGrabTransformer(ITransformer transformer)
    {
        _twoGrabTransformer = transformer as UnityEngine.Object;
        TwoGrabTransformer = transformer;
    }

    public void InjectOptionalTargetTransform(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }

    public void InjectOptionalRigidbody(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public void InjectOptionalThrowWhenUnselected(bool throwWhenUnselected)
    {
        _throwWhenUnselected = throwWhenUnselected;
    }

    public void InjectOptionalKinematicWhileSelected(bool kinematicWhileSelected)
    {
        _kinematicWhileSelected = kinematicWhileSelected;
    }

    #endregion

    /// <summary>
    /// IPointableによって選択されている間、Rigidbodyの動きを追跡し、完全に選択解除されたときに投擲速度を適用します。
    /// </summary>
    private class ThrowWhenUnselected : ITimeConsumer, IDisposable
    {
        private Rigidbody _rigidbody;
        private Func<float> _timeProvider = () => Time.time;
        public void SetTimeProvider(Func<float> timeProvider)
        {
            _timeProvider = timeProvider;
        }

        private static IObjectPool<RANSACVelocity> _ransacVelocityPool = new ObjectPool<RANSACVelocity>(
            createFunc: () => new RANSACVelocity(8, 2, 2),
            collectionCheck: false,
            defaultCapacity: 2);

        private RANSACVelocity _ransacVelocity = null;

        private Pose _lastPose = Pose.identity;
        private float _lastTime = 0f;
        private bool _isHighConfidence = true;

        private int _selectorsCount = 0;

        private TwoHandsGrab _grabbable;

        public ThrowWhenUnselected(Rigidbody rigidbody, TwoHandsGrab grabbable)
        {
            _rigidbody = rigidbody;
            _grabbable = grabbable;

            _grabbable.WhenPointerEventRaised += HandlePointerEventRaised;
        }

        public void Dispose()
        {
            _grabbable.WhenPointerEventRaised -= HandlePointerEventRaised;
        }

        private void AddSelection()
        {
            if (_selectorsCount++ == 0)
            {
                Initialize();
            }
        }

        private void RemoveSelection(bool canThrow)
        {
            if (--_selectorsCount == 0)
            {
                if (canThrow && _grabbable.SelectingPointsCount == 2)
                {
                    Process(true);
                    LoadThrowVelocities();
                }
                Teardown();
            }
            _selectorsCount = Mathf.Max(0, _selectorsCount);
        }

        private void HandlePointerEventRaised(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Select:
                    AddSelection();
                    break;
                case PointerEventType.Move:
                    if (_selectorsCount > 0)
                    {
                        Process(false);
                        MarkFrameConfidence(evt.Identifier);
                    }
                    break;
                case PointerEventType.Cancel:
                    RemoveSelection(false);
                    break;
                case PointerEventType.Unselect:
                    MarkFrameConfidence(evt.Identifier);
                    RemoveSelection(true);
                    break;
            }
        }

        private void Initialize()
        {
            Pose rootPose = _rigidbody.transform.GetPose();
            float time = _timeProvider.Invoke();

            _ransacVelocity = _ransacVelocityPool.Get();
            _ransacVelocity.Initialize(rootPose, time);
        }

        private void Teardown()
        {
            _ransacVelocityPool.Release(_ransacVelocity);
            _ransacVelocity = null;
        }

        private void MarkFrameConfidence(int emitterKey)
        {
            if (!_isHighConfidence)
            {
                return;
            }

            if (HandTrackingConfidenceProvider.TryGetTrackingConfidence(emitterKey, out bool isHighConfidence))
            {
                if (!isHighConfidence)
                {
                    _isHighConfidence = false;
                }
            }
        }

        private void Process(bool forceSubmit)
        {
            float time = _timeProvider.Invoke();
            Pose pose = _rigidbody.transform.GetPose();

            if (time > _lastTime || forceSubmit)
            {
                _isHighConfidence &= pose.position != _lastPose.position;
                _ransacVelocity.Process(pose,
                    forceSubmit ? time : _lastTime,
                    _isHighConfidence);
                _isHighConfidence = true;
            }

            _lastTime = time;
            _lastPose = pose;
        }

        private void LoadThrowVelocities()
        {
            _ransacVelocity.GetVelocities(out Vector3 velocity, out Vector3 torque);
            _rigidbody.velocity = velocity;
            _rigidbody.angularVelocity = torque;
        }

    }
}
