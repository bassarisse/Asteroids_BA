
using UnityEngine;

public class DebugExtensions {

	public static void DrawRect (Rect rect, Color color, float z, float duration) {
        var p1 = new Vector3(rect.xMin, rect.yMin, z);
        var p2 = new Vector3(rect.xMax, rect.yMin, z);
        var p3 = new Vector3(rect.xMax, rect.yMax, z);
        var p4 = new Vector3(rect.xMin, rect.yMax, z);
        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(p2, p3, color, duration);
        Debug.DrawLine(p3, p4, color, duration);
        Debug.DrawLine(p4, p1, color, duration);
	}

}
