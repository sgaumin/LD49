using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class PlayerController : MonoBehaviour
{
	private const float checkGroundedRadius = .2f;

	public static PlayerController Instance { get; private set; }

	public delegate void PlayerEventHandler();

	public event PlayerEventHandler OnShoot;

	[Header("Movements")]
	[SerializeField] private float moveSpeed = 10f;
	[SerializeField] private float jumpForce = 50f;
	[Space]
	[SerializeField] private float checkGroundTiming = 0.05f;

	[Header("Shooting")]
	[SerializeField] private float shootingReload = 0.2f;
	[SerializeField] private Transform bulletSpawn;

	[Header("References")]
	[SerializeField] private Dependency<Rigidbody2D> _body;
	[SerializeField] private LayerMask goundLayer;
	[SerializeField] private Transform[] groundChecks;

	private Rigidbody2D body => _body.Resolve(this);

	private Vector2 move;
	private bool isPushingGround;
	private bool isGrounded;
	private bool canCheckGround;
	private bool canShoot;
	private Coroutine shootingCore;
	private int bulletCount;

	private void Awake()
	{
		Instance = this;

		GameController.OnPreparationPhase += DeactivateBody;

		GameController.OnBuildingPhase += ActiveBody;

		GameController.OnTransitionPhase += DeactivateBody;
		GameController.OnTransitionPhase += StartTransition;

		GameController.OnShootingPhase += ActiveBody;
		GameController.OnShootingPhase += ConfigureShooting;
	}

	private void Start()
	{
		canCheckGround = true;
	}

	private void Update()
	{
		if (Input.GetButtonDown("Action"))
		{
			if (GameController.LevelState == LevelState.Building)
			{
				isPushingGround = true;
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
		move.x = Input.GetAxisRaw("Horizontal");
		body.AddForce(move.normalized * moveSpeed);

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

						StartCoroutine(ResetGroupCheck());
						break;
					}
				}
			}
		}
	}

	private void DeactivateBody()
	{
		body.bodyType = RigidbodyType2D.Kinematic;
		body.velocity = Vector2.zero;
	}

	private void ActiveBody()
	{
		body.bodyType = RigidbodyType2D.Dynamic;
	}

	private void ConfigureShooting()
	{
		bulletCount = 0;
		body.gravityScale = 0f;
		canShoot = true;
	}

	private IEnumerator ResetGroupCheck()
	{
		canCheckGround = false;
		yield return new WaitForSeconds(checkGroundTiming);
		canCheckGround = true;
	}

	private void OnLanding()
	{
		body.AddForce(Vector2.up * jumpForce);
	}

	public void Kill()
	{
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
		transform.DOMove(GameController.Spawn.position, 1f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(1f);
		GameController.LevelState = LevelState.Shooting;
	}

	private IEnumerator ShootingCore()
	{
		canShoot = false;

		OnShoot?.Invoke();

		var bullet = Instantiate(Prefabs.bulletPrefab);
		bullet.transform.position = bulletSpawn.position;
		bulletCount++;

		yield return new WaitForSeconds(shootingReload);
		canShoot = true;
	}
}
