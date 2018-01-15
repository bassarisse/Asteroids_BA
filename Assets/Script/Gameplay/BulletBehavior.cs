using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {
	
	public Rigidbody2D TargetBody;
	public float ImpulseForce = 4f;
	public float MaxLifeSeconds = 1f;

	bool _isAlive = false;
	bool _didHit = false;
	float _lifeTime = 0f;

	void OnEnable() {
		
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.AddForce (this.transform.up * this.ImpulseForce, ForceMode2D.Impulse);

		this._isAlive = true;
		this._didHit = false;
		this._lifeTime = 0f;

	}

	void FixedUpdate() {
		
		if (!this._isAlive)
			return;
		
		_lifeTime += Time.fixedDeltaTime;

		if (_lifeTime > this.MaxLifeSeconds)
			this.Die ();
		
	}

	void OnTriggerEnter2D(Collider2D collider) {
		
		if (this._didHit)
			return;
		
		var cache = BulletHit.Cache;
		var key = collider.gameObject;

		if (cache.ContainsKey (key)) {
			cache [key].Hit (gameObject, collider);
			this._didHit = true;
		}

	}

	public void Die() {
		this._isAlive = false;
		this.gameObject.SetActive (false);
	}

	public void Explode() {
		this.Die ();
	}
}
