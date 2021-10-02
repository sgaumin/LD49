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

	private int currentTimer;
	private Coroutine timerCore;
	private Coroutine setupTimerCore;
	private List<Image> icons = new List<Image>();

	private void Awake()
	{
		GameController.OnPreparationPhase += SetupTimer;

		GameController.OnBuildingPhase += StartTimer;

		GameController.OnTransitionPhase += StopTimer;
		GameController.OnTransitionPhase += SetupBulletIcons;

		Player.OnShoot += RemoveShootIcon;
	}

	private void SetupTimer()
	{
		timer.color = timer.color.WithAlpha(0f);
		timer.DOKill();
		timer.DOFade(1f, 0.15f);

		setupTimerCore = StartCoroutine(SetupTimerCore());
	}

	private IEnumerator SetupTimerCore()
	{
		for (int i = 0; i <= GameController.Timer; i++)
		{
			timer.text = i.ToString();

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
			timer.text = currentTimer.ToString();

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

	private void StopTimer()
	{
		if (timerCore != null)
		{
			StopCoroutine(timerCore);
		}
		if (timer.color.a > 0f)
		{
			timer.DOKill();
			timer.DOFade(0f, 0.2f);
		}
	}
}
