using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class Cinematics : MonoBehaviour
{
	[SerializeField] private CanvasGroup cinematic1;
	[SerializeField] private CanvasGroup cinematic2;
	[SerializeField] private CanvasGroup cinematic3;
	[SerializeField] private TextMeshProUGUI start;

	private bool isLoading;
	private bool complete;

	[SerializeField] private FadScreen fader;

	private void Start()
	{
		fader.FadIn(fadDuration: 0.5f);
		cinematic1.alpha = 0f;
		cinematic2.alpha = 0f;
		cinematic3.alpha = 0f;
		start.color = start.color.WithAlpha(0f);

		StartCoroutine(MainCore());
	}

	private IEnumerator MainCore()
	{
		yield return new WaitForSeconds(1f);
		DOTween.To(() => cinematic1.alpha, x => cinematic1.alpha = x, 1f, 1f).SetEase(Ease.OutCubic);
		yield return new WaitForSeconds(3f);
		DOTween.To(() => cinematic2.alpha, x => cinematic2.alpha = x, 1f, 1f).SetEase(Ease.OutCubic);
		yield return new WaitForSeconds(3f);
		DOTween.To(() => cinematic3.alpha, x => cinematic3.alpha = x, 1f, 1f).SetEase(Ease.OutCubic);
		yield return new WaitForSeconds(3f);
		start.DOFade(1f, 0.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
		complete = true;
	}

	private void Update()
	{
		if (complete && !isLoading && Input.GetButtonDown("Action"))
		{
			isLoading = true;
			StartCoroutine(LoadingCore());
		}
	}

	private IEnumerator LoadingCore()
	{
		yield return fader.FadOutCore(fadDuration: 0.5f);
		LevelLoader.LoadNextLevel();
	}
}
