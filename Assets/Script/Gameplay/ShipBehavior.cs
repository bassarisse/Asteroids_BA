using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : MonoBehaviour {

	public Rigidbody2D TargetBody;
	public GameObject BulletObject;
	public float MoveForceMultiplier = 0.2f;
	public float TurnRate = 3f;

	public int Life = 3;

	public IntEvent OnDamage;

	void Awake () {
		if (OnDamage == null)
			OnDamage = new IntEvent ();
	}

	void Start () {
		
	}

	void Update () {

		if (InputExtensions.Holding.Up) {
			this.TargetBody.AddForce (this.TargetBody.transform.up * this.MoveForceMultiplier);
		}

		if (InputExtensions.Holding.Right) {
			this.TargetBody.transform.Rotate (0, 0, -this.TurnRate);
		}

		if (InputExtensions.Holding.Left) {
			this.TargetBody.transform.Rotate (0, 0, this.TurnRate);
		}

		if (InputExtensions.Pressed.A) {
			this.FireLaser ();
		}
		
	}

	void FireLaser() {

		if (BulletObject == null)
			return;

		Instantiate (BulletObject, transform.position, transform.rotation);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {
		this.Life -= 1;
		OnDamage.Invoke (this.Life);
		if (this.Life == 0)
			SceneManager.LoadScene ("Game");
	}
}
