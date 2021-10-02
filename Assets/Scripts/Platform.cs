using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public abstract class Platform : MonoBehaviour
{
	private const float rotationMax = 30f;
	private const float rotationStep = 5f;

	[SerializeField] private bool initAtStart;

	[Header("Movements")]
	[SerializeField] protected bool activeMovementX;
	[SerializeField] protected float movementXDuration = 1f;
	[SerializeField] protected float movementXValue = 1f;
	[Space]
	[SerializeField] protected bool activeMovementY;
	[SerializeField] protected float movementYDuration = 1f;
	[SerializeField] protected float movementYValue = 1f;
	[Space]
	[SerializeField] protected Ease movementEase = Ease.InOutSine;

	[Header("References")]
	[SerializeField] protected SpriteRenderer pivot;

	private bool hasBeenInitialized;

	private void Awake()
	{
		GameController.OnBuildingPhase += OnStart;
		GameController.OnEndPhase += Disapear;
	}

	protected virtual void OnStart()
	{
		HidePivot();
		if (initAtStart)
			Init();
	}

	public void Init()
	{
		hasBeenInitialized = true;
		CheckMovements();
		ShowPivot();
	}

	public void CheckMovements()
	{
		if (activeMovementX)
			transform.DOLocalMoveX(movementXValue, movementXDuration).SetRelative().SetEase(movementEase).SetLoops(-1, LoopType.Yoyo);

		if (activeMovementY)
			transform.DOLocalMoveY(movementYValue, movementYDuration).SetRelative().SetEase(movementEase).SetLoops(-1, LoopType.Yoyo);
	}

	public void ShowPivot()
	{
		if (pivot == null)
			return;

		pivot.color = pivot.color.WithAlpha(0f);
		pivot.DOKill();
		pivot.DOColor(pivot.color.WithAlpha(1f), 0.2f).SetEase(Ease.OutSine);
	}

	public void HidePivot()
	{
		if (pivot == null)
			return;

		pivot.color = pivot.color.WithAlpha(0f);
	}

	public virtual void Push(Vector2 forcePosition)
	{
		var divisible = GetComponentInParent<DivisiblePlatform>();
		if (divisible != null && divisible.gameObject != gameObject)
			divisible.Divide(gameObject, forcePosition);

		if (!hasBeenInitialized)
			return;

		int multiplier = forcePosition.x - transform.position.x > 0 ? -1 : 1;

		float angle = transform.eulerAngles.z;
		float targetAngle = 0;
		if (angle > 180f)
		{
			targetAngle = Mathf.Clamp(transform.eulerAngles.z + rotationStep * multiplier, 360f - rotationMax, 360f + rotationStep);
		}
		else
		{
			targetAngle = Mathf.Clamp(transform.eulerAngles.z + rotationStep * multiplier, -rotationStep, rotationMax);
		}
		transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle));
	}

	private void Disapear()
	{
		transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
	}

	private void OnDestroy()
	{
		GameController.OnBuildingPhase -= OnStart;
		GameController.OnEndPhase -= Disapear;
	}
}
