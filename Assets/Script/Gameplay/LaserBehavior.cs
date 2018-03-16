using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LaserType {
	Friend = 1,
	Enemy = 2,
}

public class LaserBehavior : MonoBehaviour {

	const string LASER_FIRE_SFX = "laser1";
	const string LASER_HIT_SFX = "laser_hit";
	
	public Rigidbody2D TargetBody;
	public LaserType Type = LaserType.Friend;
	public float ImpulseForce = 4f;
	public float MaxLifeSeconds = 1f;
	public GameObjectPool CrashPool;

	bool _paused = false;
	bool _isAlive = false;
	float _lifeTime = 0f;

	void Awake() {

		AudioHandler.Load (LASER_FIRE_SFX, LASER_HIT_SFX);

		CrashPool.Fill ();
		
	}

	void OnEnable() {
		
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.AddForce (this.transform.up * this.ImpulseForce, ForceMode2D.Impulse);

		this._isAlive = true;
		this._lifeTime = 0f;

		AudioHandler.Play (LASER_FIRE_SFX, Random.Range(0.25f, 0.45f));

	}

	void FixedUpdate() {
		
		if (this._paused || !this._isAlive)
			return;
		
		_lifeTime += Time.deltaTime;

		if (_lifeTime > this.MaxLifeSeconds)
			this.Die ();
		
	}

	void OnTriggerEnter2D(Collider2D collider) {
		
		if (!this._isAlive)
			return;
		
		var cache = LaserHit.GetCache(Type);
		var key = collider.gameObject;

		if (cache.ContainsKey (key)) {
			AudioHandler.Play (LASER_HIT_SFX);
			cache [key].Hit (gameObject, collider);
			this.Explode ();
		}

	}

	public void Explode() {
		
		var crashParticle = CrashPool.GetObject ();
		if (crashParticle != null) {
			crashParticle.transform.position = transform.position;
			crashParticle.SetActive (true);
		}

		this.Die ();
	}

	public void Die() {
		this._isAlive = false;
		this.gameObject.SetActive (false);
	}

	public void Pause() {
		this._paused = true;
	}

	public void Resume() {
		this._paused = false;
	}

}
