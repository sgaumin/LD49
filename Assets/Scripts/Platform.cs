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
	[SerializeField] private bool canBeDestroyed;
	[SerializeField] protected float breakingForce = 500f;

	[Header("Movements")]
	[SerializeField] protected bool activeMovementX;
	[SerializeField] protected float movementXDuration = 1f;
	[SerializeField] protected float movementXValue = 1f;
	[SerializeField] private bool movementXReverse;
	[Space]
	[SerializeField] protected bool activeMovementY;
	[SerializeField] protected float movementYDuration = 1f;
	[SerializeField] protected float movementYValue = 1f;
	[SerializeField] private bool movementYReverse;
	[Space]
	[SerializeField] protected Ease movementEase = Ease.InOutSine;

	[Header("Audio")]
	[SerializeField] protected AudioExpress destructionSound;

	[Header("References")]
	[SerializeField] protected SpriteRenderer body;
	[SerializeField] protected SpriteRenderer pivot;

	private bool hasBeenInitialized;

	private void Awake()
	{
		GameController.OnBuildingPhase += OnStart;
	}

	protected virtual void OnStart()
	{
		if (initAtStart)
			Init();
	}

	public void Init()
	{
		hasBeenInitialized = true;
		CheckMovements();
		Show();
	}

	public void CheckMovements()
	{
		if (activeMovementX)
			transform.DOLocalMoveX(movementXValue * (movementXReverse ? -1 : 1), movementXDuration).SetRelative().SetEase(movementEase).SetLoops(-1, LoopType.Yoyo);

		if (activeMovementY)
			transform.DOLocalMoveY(movementYValue * (movementYReverse ? -1 : 1), movementYDuration).SetRelative().SetEase(movementEase).SetLoops(-1, LoopType.Yoyo);
	}

	public void PlayDestruction()
	{
		destructionSound.Play();
	}

	public void Show()
	{
		if (body != null)
		{
			body.enabled = true;
			body.color = body.color.WithAlpha(0f);
			body.DOKill();
			body.DOColor(body.color.WithAlpha(1f), 0.2f).SetEase(Ease.OutSine);
		}

		pivot.color = pivot.color.WithAlpha(0f);
		pivot.DOKill();
		pivot.DOColor(pivot.color.WithAlpha(1f), 0.2f).SetEase(Ease.OutSine);
	}

	public virtual void Push(Vector2 forcePosition)
	{
		var divisible = GetComponentInParent<DivisiblePlatform>();
		if (divisible != null && divisible.gameObject != gameObject)
			divisible.Divide(gameObject, forcePosition);

		if (!hasBeenInitialized)
			return;

		if (canBeDestroyed)
		{
			DestroyAfterImpact();
			return;
		}

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

	protected virtual void DestroyAfterImpact()
	{
		GameController.GenerateImpulse();
		GameController.SetChromaticAberation(0.8f, 0.125f, Ease.OutSine);
		StartCoroutine(ApplyForceCore());
	}

	private IEnumerator ApplyForceCore()
	{
		yield return new WaitForSeconds(0.02f);
		Player.Body.AddForce(breakingForce * Vector2.up);
		PlayDestruction();
		Destroy(gameObject);
	}

	public void Disapear()
	{
		transform.DOScale(0f, 0.15f).SetEase(Ease.InSine).OnComplete(() => Destroy(gameObject));
	}

	private void OnDestroy()
	{
		GameController.OnBuildingPhase -= OnStart;
	}
}
