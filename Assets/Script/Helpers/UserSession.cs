using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class UserSession {

	const string LAST_USERNAME_KEY = "last_username";
	const string HIGHSCORE_FILE = "highscores.json";

	public static int LastScore = 0;

    public const int EASY = 0;
    public const int MEDIUM = 1;
    public const int HARD = 2;

    // 0 = easy , 1 = medium , 2 = hard
    public static int CurrentDifficulty { get; private set; }

    public static string LastUserName {
		get {
			return PlayerPrefs.GetString (LAST_USERNAME_KEY, "");
		}
		set {
			PlayerPrefs.SetString (LAST_USERNAME_KEY, value);
		}
	}

	public static string SessionId = GetSessionId();

	public static UserScoreCollection _scores = null;

	private static string GetFilePath() {
		#if UNITY_EDITOR
		return Path.Combine (Application.temporaryCachePath, HIGHSCORE_FILE);
		#else
		return Path.Combine (Application.dataPath, HIGHSCORE_FILE);
		#endif
	}

	private static string GetSessionId() {
		EnsureScoresAreLoaded ();

		string newSessionId;
		do {
			newSessionId = Guid.NewGuid().ToString();
		} while (_scores.Values.Any(s => s.Id == newSessionId));

		return newSessionId;
	}

	public static void ResetSessionId() {
		SessionId = GetSessionId();
	}

	public static void EnsureScoresAreLoaded() {
		if (_scores == null)
			LoadScores ();
	}

	public static void LoadScores() {
		_scores = new UserScoreCollection ();
		try
		{
			var filePath = GetFilePath();
			if (File.Exists(filePath)) {
				var dataAsJson = File.ReadAllText(filePath); 
				_scores = JsonUtility.FromJson<UserScoreCollection>(dataAsJson);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError ("Error loading highscore data! Assuming as empty...");
			Debug.LogError (ex);
		}
	}

	public static void SaveScores() {
		if (_scores == null)
			return;

		try
		{
			var filePath = GetFilePath();
			var dataAsJson = JsonUtility.ToJson(_scores);
			File.WriteAllText(filePath, dataAsJson); 
		}
		catch (Exception ex)
		{
			Debug.LogError ("Error saving highscore data!");
			Debug.LogError (ex);
		}
	}

	public static void SetScore() {
		if (LastScore <= 0)
			return;
		
		EnsureScoresAreLoaded ();
		
		var currentScore = _scores.Values.FirstOrDefault (s => s.Id == SessionId);

		if (currentScore == null) {
			_scores.Values.Add (new UserScore () {
				Id = SessionId,
				UserName = LastUserName,
				Score = LastScore
			});
		} else {
			currentScore.UserName = LastUserName;
		}

		SaveScores ();
	}

    public static void SetDifficulty(int difficulty)
    {
        CurrentDifficulty = difficulty;
    }


	public static IList<UserScore> GetScores(int limit = 0) {
		EnsureScoresAreLoaded ();

		var query = _scores.Values
			.Where (s => !string.IsNullOrEmpty (s.UserName))
			.OrderByDescending (s => s.Score);

		if (limit > 0)
			return query.Take (limit).ToList();

		return query.ToList ();
	}

}

[Serializable]
public class UserScoreCollection {
	public List<UserScore> Values = new List<UserScore>();
}

[Serializable]
public class UserScore {
	public string Id;
	public string UserName;
	public int Score;
}
