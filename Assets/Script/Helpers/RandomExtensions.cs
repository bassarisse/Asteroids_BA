
using UnityEngine;

public class RandomExtensions {

	public static bool Bool () {
		return Random.Range (0, 2) == 0;
	}

	public static Vector2 PointInsideRect (Rect rect) {
		var center = rect.center;
		return new Vector2(
			center.x + (Random.value - 0.5f) * rect.width,
			center.y + (Random.value - 0.5f) * rect.height
		);
	}

}
