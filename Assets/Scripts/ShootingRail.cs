using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class ShootingRail : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private float targetWidth = 6f;
	[SerializeField] private float step = 0.1f;
	[SerializeField] private float stepClosing = 0.1f;

	private bool setup;
	private bool closing;

	private void Awake()
	{
		GameController.OnTransitionPhase += Setup;

		GameController.OnEndPhase += Closing;
	}

	private void Start()
	{
		spriteRenderer.color = spriteRenderer.color.WithAlpha(0f);
	}

	private void FixedUpdate()
	{
		if (setup && spriteRenderer.size.x < targetWidth)
		{
			spriteRenderer.size = new Vector2(spriteRenderer.size.x + step, spriteRenderer.size.y);
		}

		if (closing && spriteRenderer.size.x > 0)
		{
			spriteRenderer.size = new Vector2(spriteRenderer.size.x - stepClosing, spriteRenderer.size.y);
		}
	}

	private void Setup()
	{
		setup = true;
		spriteRenderer.DOFade(1f, 0.2f).SetEase(Ease.OutSine);
		spriteRenderer.size = new Vector2(0f, spriteRenderer.size.y);
	}

	private void Closing()
	{
		StartCoroutine(ClosingCore());
	}

	private IEnumerator ClosingCore()
	{
		setup = false;
		closing = true;

		yield return new WaitForSeconds(0.4f);
		spriteRenderer.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
	}
}
