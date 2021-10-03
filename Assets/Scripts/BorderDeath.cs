using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class BorderDeath : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private float targetHeight = 6f;
	[SerializeField] private float step = 0.1f;
	[SerializeField] private float stepClosing = 0.2f;

	private bool setup;
	private bool closing;

	private void Awake()
	{
		GameController.OnPreparationPhase += Setup;

		GameController.OnEndPhase += Closing;
	}

	private void Start()
	{
		spriteRenderer.color = spriteRenderer.color.WithAlpha(0f);
	}

	private void FixedUpdate()
	{
		if (setup && spriteRenderer.size.y < targetHeight)
		{
			spriteRenderer.size = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y + step);
		}

		if (closing && spriteRenderer.size.y > 0)
		{
			spriteRenderer.size = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y - stepClosing);
		}
	}

	private void Setup()
	{
		setup = true;
		spriteRenderer.DOFade(1f, 0.2f).SetEase(Ease.OutSine);
		spriteRenderer.size = new Vector2(spriteRenderer.size.x, 0f);
	}

	private void Closing()
	{
		StartCoroutine(ClosingCore());
	}

	private IEnumerator ClosingCore()
	{
		setup = false;
		closing = true;
		yield return new WaitForSeconds(0.8f);
		spriteRenderer.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
	}
}
