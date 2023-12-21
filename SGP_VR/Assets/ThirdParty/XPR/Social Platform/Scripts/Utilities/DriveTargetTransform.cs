using System.Collections;

using UnityEngine;
using UnityEngine.Rendering;

public enum UpdateType
{
	Update,
	LateUpdate,
	PreRender,
}

[ExecuteAlways]
public class DriveTargetTransform : MonoBehaviour
{
	[SerializeField] private Transform _target;
	public Transform Target
	{
		get => _target;
		set => _target = value;
	}

	[SerializeField] private bool _drivePosition = true;
	public bool DrivePosition
	{
		get => _drivePosition;
		set => _drivePosition = value;
	}
	
	[SerializeField] private bool _driveRotation = true;
	public bool DriveRotation
	{
		get => _driveRotation;
		set => _driveRotation = value;
	}
	
	[SerializeField] private bool _justOnStart;
	public bool JustOnStart
	{
		get => _justOnStart;
		set => _justOnStart = value;
	}
	
	[SerializeField] private bool _justOnAwake;
	public bool JustOnAwake
	{
		get => _justOnAwake;
		set => _justOnAwake = value;
	}
	
	[SerializeField] private bool _waitAFrameAtStartOrAwake;
	public bool WaitAFrameAtStartOrAwake
	{
		get => _waitAFrameAtStartOrAwake;
		set => _waitAFrameAtStartOrAwake = value;
	}
	
	[SerializeField] private bool _maintainInitialOffset;
	public bool MaintainInitialOffset
	{
		get => _maintainInitialOffset;
		set => _maintainInitialOffset = value;
	}

	[SerializeField] private UpdateType _updateStage = UpdateType.Update;
	public UpdateType UpdateStage
	{
		get => _updateStage;
		set
		{
			var prev = _updateStage;
			_updateStage = value;
			
			if(_updateStage == UpdateType.PreRender && prev != UpdateType.PreRender)
			{
				RenderPipelineManager.beginCameraRendering += OnPreRenderHandler;
				Debug.Log($"Started listening to {nameof(RenderPipelineManager.beginCameraRendering)} on {name}");
			}
			else if(prev == UpdateType.PreRender && _updateStage != UpdateType.PreRender)
			{
				RenderPipelineManager.beginCameraRendering -= OnPreRenderHandler;
				Debug.Log($"Stopped listening to {nameof(RenderPipelineManager.beginCameraRendering)} on {name}");
			}
		}
	}
	
	[SerializeField] private Vector3 _positionOffset;
	public Vector3 PositionOffset
	{
		get => _positionOffset;
		set => _positionOffset = value;
	}
	
	[SerializeField] private Quaternion _rotationOffset = Quaternion.identity;
	public Quaternion RotationOffset
	{
		get => _rotationOffset;
		set => _rotationOffset = value;
	}

	private Vector3 _initialPositionOffset = Vector3.zero;
	private Quaternion _initialRotationOffset = Quaternion.identity;

	private void Awake()
	{
		if (_justOnAwake)
			DriveTarget();
	}

	private void OnEnable()
	{
		if(_justOnStart || _justOnAwake)
			return;

		if (_updateStage == UpdateType.PreRender)
			RenderPipelineManager.beginCameraRendering += OnPreRenderHandler;
	}

	private void OnDisable()
	{
		if (_updateStage == UpdateType.PreRender)
			RenderPipelineManager.beginCameraRendering -= OnPreRenderHandler;
		StopAllCoroutines();
	}

	private void OnPreRenderHandler(ScriptableRenderContext scriptableRenderContext, Camera camera1)
	{
		if (_updateStage != UpdateType.PreRender)
			return;
		
		DriveTarget();
	}

	private void Start()
	{
		if (_maintainInitialOffset)
		{
			_initialPositionOffset = _target.position - transform.position;
			_initialRotationOffset = _target.rotation * Quaternion.Inverse(transform.rotation);
			Debug.Log($"Initial offset: {_initialPositionOffset} {_initialRotationOffset}");
		}
		
		if (_justOnStart)
			DriveTarget();
		
		_target.transform.rotation = transform.rotation;
	}

	private void DriveTarget()
	{
		if(_waitAFrameAtStartOrAwake)
			StartCoroutine(WaitAFrameAndDriveTarget());
		else
			DriveTargetInternal();
	}

	private IEnumerator WaitAFrameAndDriveTarget()
	{
		_waitAFrameAtStartOrAwake = false;
		yield return null;
		DriveTargetInternal();
	}

	private void DriveTargetInternal()
	{
		if (!Application.isPlaying) return;

		if (_target == null)
		{
			Debug.Log($"{name} has no target");
			return;
		}
		
		if (_drivePosition)
			_target.transform.position = transform.position + _positionOffset + (_maintainInitialOffset ? _initialPositionOffset : Vector3.zero);

		if (_driveRotation)
			_target.transform.rotation = transform.rotation * _rotationOffset * (_maintainInitialOffset ? _initialRotationOffset : Quaternion.identity);
	}

	public void Update()
	{
		if(_justOnStart || _justOnAwake)
			return;
		
		if(_updateStage == UpdateType.Update)
			DriveTarget();
	}

	public void LateUpdate()
	{
		if (!Application.isPlaying) return;

		if(_justOnStart || _justOnAwake)
			return;
		
		if(_updateStage == UpdateType.LateUpdate)
			DriveTarget();
	}

	public void PreRender()
	{
		if(_justOnStart || _justOnAwake)
			return;
		
		if(_updateStage == UpdateType.PreRender)
			DriveTarget();
	}
}