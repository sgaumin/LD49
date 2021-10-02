using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class DeathZone : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		var d = collision.GetComponent<ICanTakeDamage>();
		if (d != null)
		{
			d.Kill();
		}
	}
}
