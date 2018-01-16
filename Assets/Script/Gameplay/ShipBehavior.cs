using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : MonoBehaviour {

	const string CRASH_SFX = "ship_crash";

	public Rigidbody2D TargetBody;
	public ParticleSystem FireParticle;
	public SpaceTeleport SpaceTeleportBehavior;
	public List<TrailRenderer> WingTrails;
	public AudioSource EngineAudio;
	public GameObject LaserObject;
	public GameObject LaserCrashObject;
	public GameObject ShipExplosionObject;
	public Vector3 BeforeEnterPosition;
	public float MoveForceMultiplier = 0.2f;
	public float TurnRate = 3f;
	public int Life = 3;
	public int MaxBullets = 4;
	public float EnterTime = 2f;
	public IntEvent OnLifeChange;
	public UnityEvent OnEnter;
	public UnityEvent OnRestore;

	ObjectPool _shipExplosionPool;
	ObjectPool _bulletPool;
	ObjectPool _bulletCrashPool;
	float _enterTime = 0f;
	float _deathTime = 0f;

	void Awake () {
		AudioHandler.Load (CRASH_SFX);

		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();
		if (OnEnter == null)
			OnEnter = new UnityEvent ();
		if (OnRestore == null)
			OnRestore = new UnityEvent ();

		_shipExplosionPool = new ObjectPool (ShipExplosionObject, 1, 1);
		_bulletCrashPool = new ObjectPool (LaserCrashObject, MaxBullets, 2);
		_bulletPool = new ObjectPool (LaserObject, MaxBullets, 0, ConfigBullet);

	}

	void ConfigBullet(GameObject newObject) {
		var bulletBehavior = newObject.GetComponent<BulletBehavior> ();
		if (bulletBehavior != null) {
			bulletBehavior.CrashParticlePool = _bulletCrashPool;
		}
	}

	void OnEnable() {
		OnLifeChange.Invoke (Life);
		Enter ();
	}

	void Enter() {
		
		transform.position = BeforeEnterPosition;

		var hitList = new List<VoidHit> ();
		foreach (var item in VoidHit.Cache.Values)
			hitList.Add(item);
		foreach (var item in hitList)
			item.Hit ();

		_enterTime = EnterTime;
		FireParticle.Clear ();
		FireParticle.Play ();

		OnEnter.Invoke ();

	}

	void Update () {
		
		if (_deathTime > 0f) {
			EngineAudio.volume = 0f;
			EngineAudio.pitch = 1f;
			return;
		}

		var engineIsOn = InputExtensions.Holding.Up || _enterTime > 0;

		var main = FireParticle.main;
		var emission = FireParticle.emission;

		if (engineIsOn) {
			main.startSpeed = 6;
			emission.rateOverTime = 120;
		} else {
			main.startSpeed = 2;
			emission.rateOverTime = 30;
		}

		if (_enterTime <= 0f) {
			
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

		EngineAudio.volume = (engineIsOn ? 0.125f : 0.1f) + Mathf.Min (0.15f, TargetBody.velocity.sqrMagnitude / 60f);
		EngineAudio.pitch = 1f + Mathf.Min (0.6f, TargetBody.velocity.sqrMagnitude / 60f);
		
	}

	void FixedUpdate() {

		if (_enterTime > 0f) {
			_enterTime -= Time.fixedDeltaTime;
			this.transform.position = BeforeEnterPosition * (_enterTime / EnterTime);
			if (_enterTime <= 0f) {
				OnRestore.Invoke ();
				SpaceTeleportBehavior.enabled = true;
			}
		}

		if (_deathTime > 0f) {
			_deathTime -= Time.fixedDeltaTime;
			if (_deathTime <= 0f) {
				if (this.Life == 0) { // LOSE!!!
					SceneManager.LoadScene ("Game");
				} else {
					Enter ();
				}
			}
		}
		
	}

	void FireLaser() {
		var bullet = _bulletPool.GetObject ();
		if (bullet == null)
			return;
		bullet.transform.position = transform.position;
		bullet.transform.rotation = transform.rotation;
		bullet.SetActive (true);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_deathTime > 0f || _enterTime > 0f)
			return;
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {

		var explosionParticle = _shipExplosionPool.GetObject ();
		explosionParticle.transform.position = transform.position;
		explosionParticle.SetActive (true);

		SpaceTeleportBehavior.enabled = false;
		
		AudioHandler.Play (CRASH_SFX);
		_deathTime = 2f;

		this.transform.SetPositionAndRotation (BeforeEnterPosition, Quaternion.Euler (Vector3.zero));
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		this.Life -= 1;
		OnLifeChange.Invoke (this.Life);
		
	}

	public void ClearTrails() {

		if (WingTrails != null) {
			for (var i = 0; i < WingTrails.Count; i++) {
				WingTrails [i].Clear ();
			}
		}

	}
}
