using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {

	const string LASER_FIRE_SFX = "laser1";
	const string LASER_HIT_SFX = "laser_hit";
	
	public Rigidbody2D TargetBody;
	public float ImpulseForce = 4f;
	public float MaxLifeSeconds = 1f;

	bool _playedSound = false;
	bool _isAlive = false;
	bool _didHit = false;
	float _lifeTime = 0f;

	void Awake() {
		AudioHandler.Load (LASER_FIRE_SFX, LASER_HIT_SFX);
	}

	void OnEnable() {
		
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.AddForce (this.transform.up * this.ImpulseForce, ForceMode2D.Impulse);

		this._playedSound = false;
		this._isAlive = true;
		this._didHit = false;
		this._lifeTime = 0f;

	}

	void Update() {
		if (!this._playedSound) {
			AudioHandler.Play (LASER_FIRE_SFX);
			this._playedSound = true;
		}
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
			AudioHandler.Play (LASER_HIT_SFX);
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
