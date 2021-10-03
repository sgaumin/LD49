using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class RewardsManager : MonoBehaviour
{
	public static RewardsManager Instance { get; private set; }

	public List<Reward> Rewards { get; private set; } = new List<Reward>();

	private void Awake()
	{
		Instance = this;
		GameController.OnPreparationPhase += Setup;
	}

	private void Setup()
	{
		StartCoroutine(SetupCore());
	}

	private IEnumerator SetupCore()
	{
		foreach (Transform child in transform)
		{
			if (child == transform || !child.gameObject.activeSelf)
				continue;

			var reward = Instantiate(Prefabs.rewardPrefab, child);
			Rewards.Add(reward);

			yield return new WaitForSeconds(0.5f);
		}
	}
}
