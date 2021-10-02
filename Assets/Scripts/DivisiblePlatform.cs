using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class DivisiblePlatform : Platform
{
	[SerializeField] private GameObject join;
	[SerializeField] private float breakingForce = 500f;

	private List<Platform> platforms = new List<Platform>();

	protected override void OnStart()
	{
		platforms = GetComponentsInChildren<Platform>().ToList();
		base.OnStart();
	}

	public void Divide(GameObject point, Vector2 forcePosition)
	{
		if (point == join)
		{
			platforms.ForEach(x => x.transform.SetParent(transform.parent));
			platforms.ForEach(x => x.Init());

			Destroy(join);
			Destroy(gameObject);

			Player.Body.AddForce(breakingForce * Vector2.up);

			GameController.SetZoom(2f, 1.5f, Ease.OutSine);

			Time.timeScale = 0.0001f;
			DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 2f).SetEase(Ease.OutSine).SetUpdate(true);
			Time.fixedDeltaTime = 0.0001f;
			DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, 0.02f, 2f).SetEase(Ease.OutSine).SetUpdate(true); ;
		}
		else
		{
			Push(forcePosition);
		}
	}
}
