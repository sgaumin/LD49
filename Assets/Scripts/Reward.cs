using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class Reward : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private AudioExpress collectSound;

	public bool isCollected { get; set; }

	private void Awake()
	{
		GameController.OnTransitionPhase += Disapear;
	}

	private void Start()
	{
		transform.localScale = Vector3.zero;
		transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutSine);
		transform.DOLocalMoveY(0.5f, 1f).SetRelative().SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!isCollected && collision.gameObject == Player.gameObject && GameController.LevelState == LevelState.Building)
		{
			isCollected = true;
			collectSound.Play();

			Disapear();
		}
	}

	private void Disapear()
	{
		transform.DOKill();
		spriteRenderer.DOFade(0f, 0.5f);
		transform.DOLocalMoveY(2f, 0.5f).SetRelative().OnComplete(() => Destroy(gameObject));
	}

	private void OnDestroy()
	{
		GameController.OnTransitionPhase -= Disapear;
	}
}
