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
	public static SlotsManager Instance { get; private set; }

	public List<Slot> Slots { get; private set; } = new List<Slot>();

	[SerializeField] private AudioExpress completeSound;

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
			if (child == transform)
				continue;

			var slot = Instantiate(Prefabs.slotPrefab, child);
			Slots.Add(slot);

			slot.transform.localPosition = slot.transform.localPosition.withY(slot.transform.localPosition.y - 1f);
			slot.transform.DOLocalMoveY(0f, 1f).SetEase(Ease.OutBack);

			yield return new WaitForSeconds(0.5f);
		}
	}

	public void PlayCollectSound()
	{
		completeSound.Play();
	}

	public void CheckAllComplete()
	{
		if (GameController.LevelState == LevelState.Shooting && Slots.All(x => x.IsComplete))
		{
			GameController.EndLevel();
		}
	}
}
