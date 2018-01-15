using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioHandlerHelper : MonoBehaviour {

	public Vector3 DefaultAudioPoint;
	public Rect? HearableArea = null;
	public bool AutoCleanup = false;

	private IDictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
	private IDictionary<string, float> _audioTimes = new Dictionary<string, float>();
	private string _currentMusicName;
	private GameObject _musicGameObject;
	private AudioSource _music;
	
	private Dictionary<GameObject, AudioSource> _audioSourceCache;
	private ObjectPool _pool;

	void Awake() {
		_audioSourceCache = new Dictionary<GameObject, AudioSource> ();
		_pool = new ObjectPool (new GameObject ("SoundEffect"), 15, 5, ConfigAudio);
		DontDestroyOnLoad(gameObject);	
	}

	void Start() {
		var camera = Camera.main;
		DefaultAudioPoint = camera.transform.position;
		EnsureMusicAudioSource ();
	}

	void ConfigAudio(GameObject newObject) {

		newObject.AddComponent<AutoKillAudio> ();

		var audioSource = newObject.AddComponent<AudioSource>();
		audioSource.volume = 1.0f;
		audioSource.spatialBlend = 0f;

		_audioSourceCache [newObject] = audioSource;

		DontDestroyOnLoad(newObject);	

	}

	void OnEnable()
	{
		if (AutoCleanup)
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		if (AutoCleanup)
			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		Cleanup ();
	}

	public void Cleanup() {
		foreach (var item in _audioClips) {
			Resources.UnloadAsset (item.Value);
		}
		_audioClips.Clear ();
		_audioTimes.Clear ();
		_audioSourceCache.Clear ();
		_pool.Drain ();
		_musicGameObject = null;
		_music = null;
		Destroy (_musicGameObject);
	}
	
	public bool Load(string audioName) {
		
		if (_audioClips.ContainsKey (audioName))
			return true;
		
		var audioClip = Resources.Load ("Audio/" + audioName) as AudioClip;
		if (audioClip == null)
			return false;
		
		_audioClips.Add (audioName, audioClip);
		
		return true;
		
	}

	public AudioClip GetAudioClip(string audioName) {

		if (!_audioClips.ContainsKey (audioName)) {
			if (!Load(audioName))
				return null;
		}

		return _audioClips[audioName];
	}

	public bool Play(string audioName) {
		return Play (audioName, DefaultAudioPoint);
	}

	public bool Play(string audioName, float volume) {
		return Play (audioName, DefaultAudioPoint, volume);
	}
	
	public bool Play(string audioName, Vector3 point, float volume = 1.0f) {
		
		var audioClip = GetAudioClip(audioName);
		if (audioClip == null)
			return false;
		
		PlayAudioSource (audioClip, point, volume, audioName);
		
		return true;
	}

	public void PlayAudioSource(AudioClip audioClip, Vector3 point, float volume, string audioName = "") {

		if (HearableArea.HasValue && !HearableArea.Value.Contains (point))
			return;

		if (!string.IsNullOrEmpty (audioName)) {

			var time = Time.time;

			if (_audioTimes.ContainsKey (audioName)) {
				var lastPlayTime = _audioTimes [audioName];
				if (time - lastPlayTime < 0.1f)
					return;
			}

			_audioTimes [audioName] = time;
		}

		var newGameObject = _pool.GetObject ();
		newGameObject.transform.position = point;
		newGameObject.SetActive (true);

		var audioSource = _audioSourceCache[newGameObject];
		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.Play ();

	}

	private void EnsureMusicAudioSource() {
		if (_music != null)
			return;

		var musicGameObject = new GameObject("Music");
		musicGameObject.transform.position = DefaultAudioPoint;

		var music = musicGameObject.AddComponent<AudioSource>();
		music.spatialBlend = 0f;
		music.volume = 0.7f;
		music.loop = true;
		music.playOnAwake = false;

		_musicGameObject = musicGameObject;
		_music = music;

		DontDestroyOnLoad(musicGameObject);	

	}

	public bool PlayMusic(string audioName, float? volume = null) {

		if (_currentMusicName == audioName) {
			if (volume.HasValue)
				SetMusicVolume (volume.Value);
			return true;
		}

		var audioClip = GetAudioClip(audioName);
		if (audioClip == null)
			return false;

		EnsureMusicAudioSource ();

		_music.Stop ();
		_music.clip = audioClip;
		if (volume.HasValue)
			_music.volume = volume.Value;
		_music.Play ();

		_currentMusicName = audioName;

		return true;
	}

	public void SetMusicVolume(float volume) {
		EnsureMusicAudioSource ();
		_music.volume = volume;
	}

	public void StopMusic() {
		_currentMusicName = null;
		if (_music == null)
			return;
		_music.Stop();
	}

}

static internal class AudioHandler {
	
	//Disable the unused variable warning
	#pragma warning disable 0414
	static private AudioHandlerHelper _audioHandlerHelper = ( new GameObject("AudioHandlerHelper") ).AddComponent<AudioHandlerHelper>();
	#pragma warning restore 0414

	static public AudioHandlerHelper GetHelper() {
		return _audioHandlerHelper;
	}

	static public void Cleanup() {
		_audioHandlerHelper.Cleanup ();
	}

	static public bool Load(params string[] args) {
		var l = args.Length;
		for (int i = 0; i < l; i++) {
			if (!_audioHandlerHelper.Load (args [i]))
				return false;
		}
		return true;
	}

	static public bool Play(string audioName) {
		return _audioHandlerHelper.Play (audioName);
	}

	static public bool Play(string audioName, float volume) {
		return _audioHandlerHelper.Play (audioName, volume);
	}

	static public bool Play(string audioName, Vector3 point) {
		return _audioHandlerHelper.Play (audioName, point);
	}

	static public bool Play(string audioName, Vector3 point, float volume) {
		return _audioHandlerHelper.Play (audioName, point, volume);
	}

	static public bool PlayMusic(string audioName) {
		return _audioHandlerHelper.PlayMusic (audioName);
	}

	static public bool PlayMusic(string audioName, float volume) {
		return _audioHandlerHelper.PlayMusic (audioName, volume);
	}

	static public void SetMusicVolume(float volume) {
		_audioHandlerHelper.SetMusicVolume (volume);
	}

	static public void StopMusic() {
		_audioHandlerHelper.StopMusic ();
	}

}