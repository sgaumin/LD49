using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class Blinking : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI text;

	private void Start()
	{
		text.color = text.color.WithAlpha(0f);
		text.DOFade(1f, 0.4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
	}
}
