using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class PlayerController : MonoBehaviour, ICanTakeDamage
{
	private const float checkGroundedRadius = .2f;

	public static PlayerController Instance { get; private set; }

	public delegate void PlayerEventHandler();

	public event PlayerEventHandler OnShoot;

	[Header("Movements")]
	[SerializeField] private float moveSpeed = 10f;
	[SerializeField] private float jumpForce = 50f;
	[SerializeField] private float pushingGravityModifier = 1.2f;
	[SerializeField] private float releaseGravityModifier = 0.5f;
	[SerializeField] private float releaseGravityDuration = 0.5f;
	[Space]
	[SerializeField] private float resetPushingDuration = 0.2f;
	[SerializeField] private float checkGroundTiming = 0.05f;

	[Header("Breathing Animations")]
	[SerializeField] private float stretchingXFactor = 0.8f;
	[Space]
	[SerializeField] private float stretchingYFactor = 1.2f;
	[Space]
	[SerializeField] private float stretchingDuration = 1f;
	[SerializeField] private Ease stretchingEase = Ease.InOutSine;

	[Header("Pushing Animations")]
	[SerializeField] private float stretchingPushingXFactor = 0.8f;
	[Space]
	[SerializeField] private float stretchingPushingYFactor = 1.2f;
	[Space]
	[SerializeField] private float stretchingPushingDuration = 1f;
	[SerializeField] private Ease stretchingPushingEase = Ease.OutCubic;

	[Header("Sprite Animations")]
	[SerializeField] private Sprite normal;
	[SerializeField] private Sprite holdingBullet;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;

	[Header("Shooting")]
	[SerializeField] private float shootingReload = 0.2f;
	[SerializeField] private Transform bulletSpawn;

	[Header("References")]
	[SerializeField] private Dependency<Rigidbody2D> _body;
	[SerializeField] private LayerMask goundLayer;
	[SerializeField] private Transform[] groundChecks;

	public Rigidbody2D Body => _body.Resolve(this);
	public int BulletUsed { get; set; }

	private Vector2 move;
	private bool isPushingGround;
	private bool isGrounded;
	private bool canCheckGround;
	private bool canShoot;
	private Coroutine shootingCore;
	private Coroutine timerForEndCore;
	private Coroutine releasingGravityCore;
	private int bulletCount;
	private float startGravity;
	private bool canPush;
	private Vector3 startLocalScale;

	private void Awake()
	{
		Instance = this;

		GameController.OnPreparationPhase += DeactivateBody;

		GameController.OnBuildingPhase += ActiveBody;

		GameController.OnTransitionPhase += DeactivateBody;
		GameController.OnTransitionPhase += StartTransition;

		GameController.OnShootingPhase += ActiveBody;
		GameController.OnShootingPhase += ShowHoldingBullet;
		GameController.OnShootingPhase += ConfigureShooting;

		GameController.OnEndPhase += ShowNormalBody;
		GameController.OnEndPhase += StopTimerForEnd;
	}

	private void Start()
	{
		canCheckGround = true;
		canPush = true;
		startGravity = Body.gravityScale;

		// Breathing Animation
		startLocalScale = transform.localScale;
		SetBreathingAnimation();
	}

	public void SetBreathingAnimation()
	{
		transform?.DOKill();
		transform.localScale = startLocalScale;

		transform.DOScaleX(stretchingXFactor * startLocalScale.x, stretchingDuration).SetEase(stretchingEase).SetLoops(-1, LoopType.Yoyo);
		transform.DOScaleY(stretchingYFactor * startLocalScale.y, stretchingDuration).SetEase(stretchingEase).SetLoops(-1, LoopType.Yoyo);
	}

	private void SetPushingAnimation()
	{
		transform?.DOKill();
		transform.localScale = startLocalScale;

		transform.DOScaleX(stretchingPushingXFactor * startLocalScale.x, stretchingPushingDuration).SetEase(stretchingPushingEase);
		transform.DOScaleY(stretchingPushingYFactor * startLocalScale.y, stretchingPushingDuration).SetEase(stretchingPushingEase);
	}

	private void ShowNormalBody()
	{
		spriteRenderer.sprite = normal;
	}

	private void ShowHoldingBullet()
	{
		spriteRenderer.sprite = holdingBullet;
	}

	private void Update()
	{
		if (Input.GetButtonDown("Action"))
		{
			if (GameController.LevelState == LevelState.Building && !isPushingGround && canPush)
			{
				StartCoroutine(ResetPushing());

				SetPushingAnimation();
				isPushingGround = true;
				StopReleaseGravity();
				Body.gravityScale *= pushingGravityModifier;
			}
			else if (GameController.LevelState == LevelState.Shooting && canShoot && bulletCount < GameController.BulletCount)
			{
				if (shootingCore != null)
				{
					StopCoroutine(shootingCore);
				}
				shootingCore = StartCoroutine(ShootingCore());
			}
		}
	}

	private void FixedUpdate()
	{
		// Inputs
		if (GameController.LevelState != LevelState.End)
		{
			move.x = Input.GetAxisRaw("Horizontal");
			Body.AddForce(move.normalized * moveSpeed);
		}

		if (GameController.LevelState == LevelState.Building)
		{
			// Ground Check
			if (canCheckGround)
			{
				isGrounded = false;
				foreach (var check in groundChecks)
				{
					if (isGrounded)
						break;

					Collider2D[] colliders = Physics2D.OverlapCircleAll(check.position, checkGroundedRadius, goundLayer);
					for (int i = 0; i < colliders.Length; i++)
					{
						if (colliders[i].gameObject == gameObject)
							return;

						isGrounded = true;
						OnLanding();
						if (isPushingGround)
						{
							isPushingGround = false;
							colliders[i].GetComponent<BasicPlatform>().Push(transform.position);
						}

						var p = Instantiate(Prefabs.smallImpactEffect);
						p.transform.position = check.position;

						GameController.GenerateImpulse();

						StartCoroutine(ResetGroupCheck());
						break;
					}
				}
			}
		}
	}

	private void ReleaseGravity()
	{
		if (releasingGravityCore != null)
		{
			StopCoroutine(releasingGravityCore);
		}
		releasingGravityCore = StartCoroutine(ReleaseGravityCore());
	}

	private void StopReleaseGravity()
	{
		if (releasingGravityCore != null)
		{
			StopCoroutine(releasingGravityCore);
		}
		Body.gravityScale = startGravity;
	}

	private IEnumerator ReleaseGravityCore()
	{
		Body.gravityScale = startGravity;
		Body.gravityScale *= releaseGravityModifier;
		yield return new WaitForSeconds(0.02f);
		Body.AddForce(Vector2.up * jumpForce);
		yield return new WaitForSeconds(releaseGravityDuration);
		Body.gravityScale = startGravity;
	}

	private void Freeze()
	{
		Time.timeScale = 0f;
		DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.125f).SetEase(Ease.OutSine).SetUpdate(true);
	}

	private void DeactivateBody()
	{
		Body.bodyType = RigidbodyType2D.Kinematic;
		Body.velocity = Vector2.zero;
	}

	private void ActiveBody()
	{
		Body.bodyType = RigidbodyType2D.Dynamic;
	}

	private void ConfigureShooting()
	{
		bulletCount = 0;
		Body.gravityScale = 0f;
		canShoot = true;
	}

	private IEnumerator ResetGroupCheck()
	{
		canCheckGround = false;
		yield return new WaitForSeconds(checkGroundTiming);
		canCheckGround = true;
	}

	private IEnumerator ResetPushing()
	{
		canPush = false;
		yield return new WaitForSeconds(resetPushingDuration);
		canPush = true;
	}

	public void OnLanding()
	{
		Freeze();
		SetBreathingAnimation();
		ReleaseGravity();
	}

	public void Kill()
	{
		SetBreathingAnimation();
		GameController.StartShootingPhase();
	}

	private void StartTransition()
	{
		StartCoroutine(TransitionCore());
	}

	private IEnumerator TransitionCore()
	{
		GameController.GenerateImpulse();
		yield return new WaitForSeconds(1f);
		transform.DOMove(GameController.Spawn.position, 1f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(1f);
		GameController.LevelState = LevelState.Shooting;
	}

	private IEnumerator ShootingCore()
	{
		canShoot = false;

		OnShoot?.Invoke();

		GameController.GenerateImpulse();

		var bullet = Instantiate(Prefabs.bulletPrefab);
		bullet.transform.position = bulletSpawn.transform.position;
		bulletCount++;

		if (bulletCount >= GameController.BulletCount)
			StartTimerForEnd();

		ShowNormalBody();
		yield return new WaitForSeconds(shootingReload);

		if (bulletCount < GameController.BulletCount)
			ShowHoldingBullet();

		canShoot = true;
	}

	private void StartTimerForEnd()
	{
		timerForEndCore = StartCoroutine(StartTimerForEndCore());
	}

	private void StopTimerForEnd()
	{
		if (timerForEndCore != null)
		{
			StopCoroutine(timerForEndCore);
		}
	}

	private IEnumerator StartTimerForEndCore()
	{
		yield return new WaitForSeconds(5f);
		FindObjectsOfType<Bullet>().ForEach(x => x.Kill());
		GameController.EndLevel();
	}

	public void CheckAllBulletUsed()
	{
		BulletUsed++;
		if (GameController.BulletCount <= BulletUsed)
		{
			GameController.EndLevel();
		}
	}
}
