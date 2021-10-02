using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class SlotDetector : MonoBehaviour
{
	[SerializeField] private Slot parent;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		var bullet = collision.GetComponent<Bullet>();
		if (bullet != null && GameController.LevelState == LevelState.Shooting)
		{
			bullet.Kill();
			parent.IsComplete = true;
			GameController.GenerateImpulse();
		}
	}
}
