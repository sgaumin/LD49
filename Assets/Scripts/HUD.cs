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
	[SerializeField] private TextMeshProUGUI jumpCount;
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI help;
	[SerializeField] private TextMeshProUGUI end;
	[SerializeField] private TextMeshProUGUI endScore;
	[SerializeField] private TextMeshProUGUI endReward;
	[SerializeField] private TextMeshProUGUI phase;
	[SerializeField] private CanvasGroup group;
	[SerializeField] private CanvasGroup tutorialBuilding;
	[SerializeField] private CanvasGroup tutorialShooting;

	private Coroutine setupJumpCountCore;
	private List<Image> icons = new List<Image>();
	private Coroutine setupCore;

	private void Awake()
	{
		GameController.OnPreparationPhase += Setup;

		GameController.OnBuildingPhase += StartBuildingPhase;
		GameController.OnBuildingPhase += StopSetupCore;
		GameController.OnBuildingPhase += () => ShowPhaseName("UNSTABLE PHASE");

		GameController.OnTransitionPhase += StopBuildingPhase;
		GameController.OnTransitionPhase += HidePhaseName;
		GameController.OnTransitionPhase += SetupBulletIcons;

		GameController.OnShootingPhase += () => ShowPhaseName("CONNECTION PHASE");
		GameController.OnShootingPhase += StartShootingPhase;

		GameController.OnEndPhase += End;
		GameController.OnEndPhase += HidePhaseName;
		GameController.OnEndPhase += StartRemoveAllIcon;

		Player.OnPush += DisplayJumpCount;
		Player.OnShoot += RemoveShootIcon;
	}

	private void Setup()
	{
		// Title
		title.text = GameController.LevelName;
		title.color = title.color.WithAlpha(0f);
		help.color = help.color.WithAlpha(0f);
		end.color = end.color.WithAlpha(0f);
		phase.color = end.color.WithAlpha(0f);
		tutorialBuilding.alpha = 0f;
		tutorialShooting.alpha = 0f;

		// Timer
		jumpCount.color = jumpCount.color.WithAlpha(0f);
		jumpCount.DOKill();
		jumpCount.DOFade(1f, 0.15f);

		setupJumpCountCore = StartCoroutine(SetupJumpCountCore());

		setupCore = StartCoroutine(SetupCore());
	}

	private IEnumerator SetupCore()
	{
		yield return new WaitForSeconds(1f);
		title.DOFade(1f, 1f).SetEase(Ease.OutSine);

		yield return new WaitForSeconds(2f);
		help.DOFade(1f, 0.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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

	private IEnumerator SetupJumpCountCore()
	{
		for (int i = 0; i <= GameController.JumpCount; i++)
		{
			jumpCount.text = $"{i}";

			jumpCount.transform.DOKill();
			jumpCount.transform.localScale *= 1.2f;
			jumpCount.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);

			yield return new WaitForSeconds(1f / GameController.JumpCount);
		}
	}

	private void StartBuildingPhase()
	{
		jumpCount.text = $"{GameController.JumpCount}";
		if (setupJumpCountCore != null)
		{
			StopCoroutine(setupJumpCountCore);
		}

		if (GameController.ShowTutorials)
		{
			DOTween.To(() => tutorialBuilding.alpha, x => tutorialBuilding.alpha = x, 1f, 0.2f).SetEase(Ease.OutCubic);
		}
	}

	private void DisplayJumpCount(int value)
	{
		if (value < 0)
			return;

		jumpCount.text = $"{value}";

		jumpCount.transform.DOKill();
		jumpCount.transform.localScale = Vector3.one * 1.2f;
		jumpCount.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);

		if (value == 0 && jumpCount.color.a > 0f)
		{
			jumpCount.DOKill();
			jumpCount.DOFade(0f, 0.2f);
		}
	}

	private void StartShootingPhase()
	{
		if (GameController.ShowTutorials)
		{
			DOTween.To(() => tutorialShooting.alpha, x => tutorialShooting.alpha = x, 1f, 0.2f).SetEase(Ease.OutCubic);
		}
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

	private void StopBuildingPhase()
	{
		if (GameController.ShowTutorials)
		{
			tutorialBuilding.DOKill();
			DOTween.To(() => tutorialBuilding.alpha, x => tutorialBuilding.alpha = x, 0f, 0.2f).SetEase(Ease.OutCubic);
		}
	}

	private void End()
	{
		StartCoroutine(EndCore());

		if (GameController.ShowTutorials)
		{
			tutorialShooting.DOKill();
			DOTween.To(() => tutorialShooting.alpha, x => tutorialShooting.alpha = x, 0f, 0.2f).SetEase(Ease.OutCubic);
		}
	}

	private IEnumerator EndCore()
	{
		endScore.text = $"COMPLETION\n0/{ Slots.Slots.Count()}";
		endReward.text = $"BONUS\n{Rewards.Rewards.Where(x => x.isCollected).Count()}/{ Rewards.Rewards.Count()}";

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
				Destroy(b.gameObject);
				GameController.SetChromaticAberation(0.8f, 0.1f, Ease.OutSine);
				endScore.text = $"COMPLETION\n{++i}/{ Slots.Slots.Count()}";
			});
			Slots.PlayCollectSound();

			yield return new WaitForSeconds(0.6f);
		}

		yield return new WaitForSeconds(1f);

		end.DOFade(1f, 0.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

		if (Slots.Slots.All(x => x.IsComplete))
		{
			end.text = "CLICK TO CONTINUE";
		}
		else
		{
			end.text = "CLICK TO RETRY";
		}

		GameController.canListenEndInput = true;
	}

	private void ShowPhaseName(string name)
	{
		phase.text = name;
		phase?.DOKill();
		phase.DOFade(1f, 0.2f);
	}

	private void HidePhaseName()
	{
		phase?.DOKill();
		phase.DOFade(0f, 0.2f);
	}
}
