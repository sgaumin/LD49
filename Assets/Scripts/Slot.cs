using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class Slot : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject unactiveForm;
	[SerializeField] private GameObject activeForm;
	[SerializeField] private GameObject completeForm;

	private bool isComplete;

	public bool IsComplete
	{
		get => isComplete;
		set
		{
			isComplete = value;
			if (isComplete)
			{
				unactiveForm.SetActive(false);
				activeForm.SetActive(false);
				completeForm.SetActive(true);

				Slots.CheckAllComplete();
			}
		}
	}

	private void Awake()
	{
		GameController.OnShootingPhase += SetupShooting;
	}

	private void Start()
	{
		unactiveForm.FadIn();
	}

	private void SetupShooting()
	{
		activeForm.SetActive(true);
		activeForm.FadIn();
	}
}
