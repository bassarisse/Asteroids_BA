﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : Waiter {

	const string NEW_LIFE_SFX = "new_life";
	const string CRASH_SFX = "ship_crash";
	const string HYPERSPACE_SFX = "hyperspace";
	const string MEGA_EXPLOSION_SFX = "mega_explosion";

	internal enum ShipState {
		Idle = 0,
		Entering = 1,
		Hyperspacing = 2,
		Dead = 3,
	}

	[Header("References")]
	public Rigidbody2D Body;
	public ParticleSystem FireParticle;
	public SpaceTeleport SpaceTeleportBehavior;
	public AudioSource EngineAudio;
	public List<TrailRenderer> WingTrails;
	[Space(20)]

	[Header("General parameters")]
	public Vector3 BeforeEnterPosition;
	public int InitialLifeCount = 3;
	public float MoveForceMultiplier = 7f;
	public float TurnRate = 3f;
	public float EnterImpulseMultiplier = 9.5f;
	public float EnterTime = 1f;
	public float AfterEnterTime = 1f;
	public float DeathTime = 2f;
	public float HyperspaceMargin = 1f;
	public float HyperspaceTransitionTime = 0.75f;
	public float HyperspaceWaitTime = 0.5f;
	[Space(20)]

	[Header("Events")]
	public IntEvent OnLifeChange;
	public UnityEvent OnEnter;
	public UnityEvent OnRestore;
	public UnityEvent OnDamage;
	public UnityEvent OnDie;
	[Space(20)]

	[Header("Object Pools")]
	public GameObjectPool MegaExplosionPool;
	public GameObjectPool ShipExplosionPool;
	public GameObjectPool HyperspacePool;
	public GameObjectPool HyperspaceFinishPool;
	public GameObjectPool LaserPool;
	public GameObjectPool LaserCrashPool;

	ShipState _state = ShipState.Idle;
	int _currentLifeCount = 3;
	bool _canHyperspace = true;
	Camera _camera;

    protected override void SetDifficulty(int difficulty)
    {
       
    }

    protected  override void Awake ()
    {
        base.Awake();

		AudioHandler.Load (NEW_LIFE_SFX, CRASH_SFX, HYPERSPACE_SFX);

		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();
		if (OnEnter == null)
			OnEnter = new UnityEvent ();
		if (OnRestore == null)
			OnRestore = new UnityEvent ();
		if (OnDamage == null)
			OnDamage = new UnityEvent ();
		if (OnDie == null)
			OnDie = new UnityEvent ();

		MegaExplosionPool.Fill ();
		ShipExplosionPool.Fill ();
		HyperspacePool.Fill ();
		HyperspaceFinishPool.Fill ();
		LaserPool.Fill ();
		LaserCrashPool.Fill ();

		_camera = Camera.main;

	}

	public void ConfigLaser(GameObject newObject) {
		var laserBehavior = newObject.GetComponent<LaserBehavior> ();
		if (laserBehavior != null) {
			laserBehavior.CrashPool = LaserCrashPool;
		}
	}

	void OnEnable() {
		OnLifeChange.Invoke (_currentLifeCount);
	}

	void Start() {
		_currentLifeCount = InitialLifeCount;
		StartCoroutine (Enter ());
	}

	void Update () {
		
		if (_state == ShipState.Dead) {
			EngineAudio.volume = 0f;
			EngineAudio.pitch = 1f;
			return;
		}

		var engineIsOn = InputExtensions.Holding.Up && _state == ShipState.Idle || _state == ShipState.Entering;

		HandleInput ();
		UpdateFire (engineIsOn);
		UpdateVolume (engineIsOn);

	}

	void HandleInput() {

		if (_state != ShipState.Idle)
			return;

		if (InputExtensions.Holding.Up) {
			this.Body.AddForce (transform.up * this.MoveForceMultiplier);
		}

		if (InputExtensions.Holding.Right) {
			this.Body.transform.Rotate (0, 0, -this.TurnRate);
		}

		if (InputExtensions.Holding.Left) {
			this.Body.transform.Rotate (0, 0, this.TurnRate);
		}

		if (InputExtensions.Pressed.A) {
			ShootLaser ();
		}

		if (InputExtensions.Pressed.X) {
			Hyperspace ();
		}

	}

	void UpdateFire(bool engineIsOn) {
		
		var main = FireParticle.main;
		var emission = FireParticle.emission;

		if (engineIsOn) {
			main.startSpeed = 6;
			emission.rateOverTime = 120;
		} else {
			main.startSpeed = 2;
			emission.rateOverTime = 30;
		}

	}

	void UpdateVolume(bool engineIsOn) {

		var volume = (engineIsOn ? 0.1f : 0.075f) + Mathf.Min (0.15f, Body.velocity.sqrMagnitude / 60f);

		if (_state == ShipState.Hyperspacing)
			volume /= 5f;

		EngineAudio.volume = EngineAudio.volume + (volume - EngineAudio.volume) * Time.deltaTime;
		EngineAudio.pitch = 1f + Mathf.Min (0.6f, Body.velocity.sqrMagnitude / 60f);

	}

	IEnumerator Enter(bool clearScreen = false) {
		
		ClearTrails ();

		if (clearScreen) {
			var megaExplosion = MegaExplosionPool.GetObject ();
			megaExplosion.SetActive (true);
			AudioHandler.Play (MEGA_EXPLOSION_SFX, 0.75f);
		}

		_state = ShipState.Entering;

		transform.position = BeforeEnterPosition;

		yield return Wait(0.1f);

		VoidHit.HitAll ();

		ClearTrails ();
		FireParticle.Clear ();
		FireParticle.Play ();

		OnEnter.Invoke ();

		this.Body.AddForce (transform.up * EnterImpulseMultiplier, ForceMode2D.Impulse);

		yield return Wait(EnterTime);

		_state = ShipState.Idle;

		yield return Wait(AfterEnterTime);

		OnRestore.Invoke ();
		SpaceTeleportBehavior.enabled = true;

	}

	void ShootLaser() {
		var laser = LaserPool.GetObject ();
		if (laser == null)
			return;
		laser.transform.position = transform.position + transform.up * 0.55f;
		laser.transform.rotation = transform.rotation;
		laser.SetActive (true);
	}

	void Hyperspace() {
		if (!_canHyperspace)
			return;
		StartCoroutine (ExecuteHyperspace ());
	}

	IEnumerator ExecuteHyperspace() {

		_state = ShipState.Hyperspacing;
		_canHyperspace = false;

		SpaceTeleportBehavior.enabled = false;
		AudioHandler.Play (HYPERSPACE_SFX);

		var hyperspacePosition = GetHyperspacePosition ();

		var hyperspaceParticle = HyperspacePool.GetObject ();
		hyperspaceParticle.transform.position = transform.position;
		hyperspaceParticle.transform.rotation = transform.rotation;
		hyperspaceParticle.SetActive (true);

		var hyperspaceFinishParticle = HyperspaceFinishPool.GetObject ();
		hyperspaceFinishParticle.transform.position = hyperspacePosition;
		hyperspaceFinishParticle.transform.rotation = transform.rotation;
		hyperspaceFinishParticle.SetActive (true);

		transform.position = BeforeEnterPosition;
		this.Body.velocity = Vector2.zero;
		this.Body.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		yield return Wait (HyperspaceTransitionTime);

		_state = ShipState.Idle;

		transform.position = hyperspacePosition;
		SpaceTeleportBehavior.enabled = true;
		ClearTrails ();
		FireParticle.Play ();

		yield return Wait (HyperspaceWaitTime);

		_canHyperspace = true;

	}

	public void Damage(GameObject originGameObject, Collider2D collider) {
		
		if (_state != ShipState.Idle)
			return;

		_state = ShipState.Dead;
		StartCoroutine (Die ());

	}

	IEnumerator Die() {

		SpaceTeleportBehavior.enabled = false;
		AudioHandler.Play (CRASH_SFX);

		var explosionParticle = ShipExplosionPool.GetObject ();
		explosionParticle.transform.position = transform.position;
		explosionParticle.transform.rotation = transform.rotation;
		explosionParticle.SetActive (true);

		transform.SetPositionAndRotation (BeforeEnterPosition, Quaternion.Euler (Vector3.zero));
		this.Body.velocity = Vector2.zero;
		this.Body.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		this._currentLifeCount -= 1;
		OnDamage.Invoke ();
		OnLifeChange.Invoke (this._currentLifeCount);

		yield return Wait (DeathTime);

		if (this._currentLifeCount == 0) {
			OnDie.Invoke();
		} else {
			StartCoroutine (Enter (true));
		}

	}

	void OnTriggerEnter2D(Collider2D collider)
    {
        if (_state != ShipState.Idle)
            return;

        var cache = ShipHit.Cache;
        var key = collider.gameObject;

        if (cache.ContainsKey(key))
            cache[key].Hit(gameObject, collider);
        
	}

	public void ReceiveNewLife() {
		if (this._currentLifeCount <= 0 || _state == ShipState.Dead)
			return;
		
		AudioHandler.Play (NEW_LIFE_SFX);
		this._currentLifeCount += 1;
		OnLifeChange.Invoke (this._currentLifeCount);

	}

	public void ClearTrails() {

		for (var i = 0; i < WingTrails.Count; i++) {
			// not sure why disabling/re-enabling is needed after Unity update
			WingTrails [i].enabled = false;
			WingTrails [i].Clear ();
			WingTrails [i].enabled = true;
		}

	}

	Vector3 GetHyperspacePosition() {

		var pos = transform.position;

		if (_camera == null)
			return pos;

		var center = _camera.transform.position;
		var height = _camera.orthographicSize * 2f;
		var width = height * _camera.aspect;

		return new Vector3(
			center.x + (Random.value - 0.5f) * (width - HyperspaceMargin * 2f),
			center.y + (Random.value - 0.5f) * (height - HyperspaceMargin * 2f),
			pos.z
		);
	}
}
