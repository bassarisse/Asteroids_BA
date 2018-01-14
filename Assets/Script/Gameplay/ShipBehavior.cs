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

	List<GameObject> _bulletPool;

	void Awake () {
		if (OnDamage == null)
			OnDamage = new IntEvent ();
	}

	void Start () {

		_bulletPool = new List<GameObject> ();
		for (var i = 0; i < 10; i++) {
			_bulletPool.Add (CreateBullet ());
		}
		
	}

	GameObject CreateBullet() {
		var bullet = Instantiate (this.BulletObject);
		bullet.SetActive (false);
		return bullet;
	}

	GameObject GetBullet() {
		for (var i = 0; i < _bulletPool.Count; i++) {
			var bullet = _bulletPool [i];
			if (!bullet.activeInHierarchy)
				return bullet;
		}
		for (var i = 0; i < 4; i++) {
			_bulletPool.Add (CreateBullet ());
		}
		return _bulletPool [_bulletPool.Count - 1];
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
		var bullet = GetBullet ();
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
		this.Life -= 1;
		OnDamage.Invoke (this.Life);
		if (this.Life == 0)
			SceneManager.LoadScene ("Game");
	}
}
