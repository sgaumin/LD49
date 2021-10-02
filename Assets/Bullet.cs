using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using static Facade;

public class Bullet : MonoBehaviour, ICanTakeDamage
{
	[SerializeField] private Dependency<Rigidbody2D> _body;
	private Rigidbody2D body => _body.Resolve(this);

	public void Kill()
	{
		body.bodyType = RigidbodyType2D.Kinematic;
		body.velocity = Vector2.zero;
		transform.DOMove(transform.position, 0.2f);
		transform.DOScale(0f, 0.2f);
	}
}
