using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class SlotsManager : MonoBehaviour
{
	private List<Slot> slots = new List<Slot>();

	private void Awake()
	{
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
			if (child == transform)
				continue;

			var slot = Instantiate(Prefabs.slotPrefab, child);
			slots.Add(slot);

			slot.transform.localPosition = slot.transform.localPosition.withY(slot.transform.localPosition.y - 2f);
			slot.transform.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutBack);

			yield return new WaitForSeconds(0.2f);
		}
	}
}
