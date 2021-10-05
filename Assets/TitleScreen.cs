using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class TitleScreen : GameSystem
{
	private bool isLoading;

	[SerializeField] private FadScreen fader;

	private void Start()
	{
		fader.FadIn(fadDuration: 0.5f);
	}

	protected override void Update()
	{
		base.Update();
		if (!isLoading && Input.GetButtonDown("Action"))
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
