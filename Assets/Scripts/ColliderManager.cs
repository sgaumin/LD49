using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class ColliderManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Collider2D deathZone;
	[SerializeField] private Collider2D shootingLimits;

	private void Awake()
	{
		GameController.OnPreparationPhase += DeactivateDeathZone;
		GameController.OnPreparationPhase += DeactivateShootingLimits;

		GameController.OnBuildingPhase += ActivateDeathZone;

		GameController.OnShootingPhase += ActivateShootingLimits;
	}

	private void ActivateDeathZone()
	{
		deathZone.enabled = true;
	}

	private void DeactivateDeathZone()
	{
		deathZone.enabled = false;
	}

	private void ActivateShootingLimits()
	{
		shootingLimits.enabled = true;
	}

	private void DeactivateShootingLimits()
	{
		shootingLimits.enabled = false;
	}
}
