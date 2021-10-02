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

			GameController.GenerateImpulse();
			GameController.SetChromaticAberation(0.8f, 0.125f, Ease.OutSine);

			Player.Body.AddForce(breakingForce * Vector2.up);

			Destroy(join);
			Destroy(gameObject);
		}
		else
		{
			Push(forcePosition);
		}
	}
}
