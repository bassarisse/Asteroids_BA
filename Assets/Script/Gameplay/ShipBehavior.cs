using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : MonoBehaviour {

	const string CRASH_SFX = "ship_crash";

	public Rigidbody2D TargetBody;
	public GameObject BulletObject;
	public float MoveForceMultiplier = 0.2f;
	public float TurnRate = 3f;
	public int Life = 3;
	public IntEvent OnLifeChange;

	ObjectPool _bulletPool;

	void Awake () {
		AudioHandler.Load (CRASH_SFX);
		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();

		_bulletPool = new ObjectPool (BulletObject, 6, 4);

	}

	void OnEnable() {
		OnLifeChange.Invoke (Life);
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
		var bullet = _bulletPool.GetObject ();
		bullet.transform.position = transform.position;
		bullet.transform.rotation = transform.rotation;
		bullet.SetActive (true);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {
		AudioHandler.Play (CRASH_SFX);
		this.Life -= 1;
		OnLifeChange.Invoke (this.Life);
		if (this.Life == 0) // LOSE!!!
			SceneManager.LoadScene ("Game");
	}
}
