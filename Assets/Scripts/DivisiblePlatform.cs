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

	private List<Platform> platforms = new List<Platform>();

	protected override void OnStart()
	{
		platforms = GetComponentsInChildren<Platform>(true).ToList();
		base.OnStart();
	}

	public void Divide(GameObject point, Vector2 forcePosition)
	{
		if (point == join)
		{
			platforms.ForEach(x => x.gameObject.SetActive(true));
			platforms.ForEach(x => x.transform.SetParent(transform.parent));
			platforms.ForEach(x => x.Init());
			Destroy(join);

			DestroyAfterImpact();
		}
		else
		{
			Push(forcePosition);
		}
	}
}
