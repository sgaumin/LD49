using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using Tools.Utils;
using UnityEngine;
using UnityEngine.UI;
using static Facade;

public class HUD : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform bulletIconHolder;
	[SerializeField] private TextMeshProUGUI timer;
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI help;
	[SerializeField] private TextMeshProUGUI end;
	[SerializeField] private TextMeshProUGUI endScore;
	[SerializeField] private CanvasGroup group;

	private int currentTimer;
	private Coroutine timerCore;
	private Coroutine setupTimerCore;
	private List<Image> icons = new List<Image>();
	private Coroutine setupCore;

	private void Awake()
	{
		GameController.OnPreparationPhase += Setup;

		GameController.OnBuildingPhase += StartTimer;
		GameController.OnBuildingPhase += StopSetupCore;

		GameController.OnTransitionPhase += StopTimer;
		GameController.OnTransitionPhase += SetupBulletIcons;

		GameController.OnEndPhase += End;
		GameController.OnEndPhase += StartRemoveAllIcon;

		Player.OnShoot += RemoveShootIcon;
	}

	private void Setup()
	{
		// Title
		title.text = GameController.LevelName;
		title.color = title.color.WithAlpha(0f);
		help.color = help.color.WithAlpha(0f);
		end.color = end.color.WithAlpha(0f);

		// Timer
		timer.color = timer.color.WithAlpha(0f);
		timer.DOKill();
		timer.DOFade(1f, 0.15f);

		setupTimerCore = StartCoroutine(SetupTimerCore());

		setupCore = StartCoroutine(SetupCore());
	}

	private IEnumerator SetupCore()
	{
		yield return new WaitForSeconds(2f);
		title.DOFade(1f, 1f).SetEase(Ease.OutSine);

		yield return new WaitForSeconds(4f);
		help.DOFade(1f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
	}

	private void StopSetupCore()
	{
		if (setupCore != null)
		{
			StopCoroutine(setupCore);
		}

		title?.DOKill();
		title.DOFade(0f, 0.5f).SetEase(Ease.OutSine);

		help?.DOKill();
		help.DOFade(0f, 0.5f).SetEase(Ease.OutSine);
	}

	private IEnumerator SetupTimerCore()
	{
		for (int i = 0; i <= GameController.Timer; i++)
		{
			timer.text = $"{i}s";

			timer.transform.DOKill();
			timer.transform.localScale *= 1.2f;
			timer.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);

			yield return new WaitForSeconds(1f / GameController.Timer);
		}
	}

	private void StartTimer()
	{
		if (setupTimerCore != null)
		{
			StopCoroutine(setupTimerCore);
		}
		timerCore = StartCoroutine(StartTimerCore());
	}

	private IEnumerator StartTimerCore()
	{
		currentTimer = GameController.Timer;
		while (currentTimer >= 0)
		{
			timer.text = $"{currentTimer}s";

			timer.transform.DOKill();
			timer.transform.localScale *= 1.2f;
			timer.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);

			yield return new WaitForSeconds(1f);
			currentTimer--;
		}
		GameController.StartShootingPhase();
	}

	private void SetupBulletIcons()
	{
		StartCoroutine(SetupBulletIconsCore());
	}

	private IEnumerator SetupBulletIconsCore()
	{
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < GameController.BulletCount; i++)
		{
			var icon = Instantiate(Prefabs.bulletIconPrefab, bulletIconHolder);
			icon.transform.localScale *= 1.2f;
			icon.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutSine);
			icons.Add(icon);

			yield return new WaitForSeconds(0.2f);
		}
	}

	private void RemoveShootIcon()
	{
		if (!icons.IsEmpty())
		{
			var icon = icons.Last();
			icons.Remove(icon);
			icon.transform.DOScale(0f, 0.2f).SetEase(Ease.OutSine).OnComplete(() => Destroy(icon.gameObject));
		}
	}

	private void StartRemoveAllIcon()
	{
		StartCoroutine(StartRemoveAllIconCore());
	}

	private IEnumerator StartRemoveAllIconCore()
	{
		while (!icons.IsEmpty())
		{
			var icon = icons.Last();
			icons.Remove(icon);
			icon.transform.DOScale(0f, 0.2f).SetEase(Ease.OutSine).OnComplete(() => Destroy(icon.gameObject));
			yield return new WaitForSeconds(0.2f);
		}
	}

	private void StopTimer()
	{
		StartCoroutine(StopTimerCore());
	}

	private IEnumerator StopTimerCore()
	{
		while (currentTimer >= 0)
		{
			timer.text = $"{currentTimer}s";

			timer.transform.DOKill();
			timer.transform.localScale *= 1.2f;
			timer.transform.DOScale(Vector3.one, 0.025f).SetEase(Ease.OutSine);

			yield return new WaitForSeconds(0.025f);
			currentTimer--;
		}

		if (timerCore != null)
		{
			StopCoroutine(timerCore);
		}
		if (timer.color.a > 0f)
		{
			timer.DOKill();
			timer.DOFade(0f, 0.2f);
		}

		yield return new WaitForSeconds(1f);
	}

	private void End()
	{
		StartCoroutine(EndCore());
	}

	private IEnumerator EndCore()
	{
		endScore.text = $"0/{ Slots.Slots.Count()}";

		yield return new WaitForSeconds(1f);
		DOTween.To(() => group.alpha, x => group.alpha = x, 1, 0.2f).SetEase(Ease.OutCubic);

		yield return new WaitForSeconds(0.6f);
		int i = 0;
		var winnerSlots = Slots.Slots.Where(x => x.IsComplete).ToList();
		foreach (var slot in winnerSlots)
		{
			var b = Instantiate(Prefabs.bulletPrefab);
			b.Body.bodyType = RigidbodyType2D.Kinematic;
			b.transform.position = slot.transform.position;
			b.transform.DOMove(endScore.transform.position, 0.35f).SetEase(Ease.OutSine).OnComplete(() =>
			{
				Destroy(b);
				GameController.SetChromaticAberation(1f, 0.1f, Ease.OutSine);
				endScore.text = $"{++i}/{ Slots.Slots.Count()}";
			});
			yield return new WaitForSeconds(0.6f);
		}

		yield return new WaitForSeconds(1f);
		end.DOFade(1f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
	}
}
