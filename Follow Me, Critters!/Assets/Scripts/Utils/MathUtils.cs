using UnityEngine;

public class MathUtils
{
	// 은행원 반올림이 아닌 가장 가까운 정수로 반올림하는 함수
	public static int RoundToNearestInt(float value)
	{
		return value >= 0 ? (int)(value + 0.5f) : (int)(value - 0.5f);
	}

	public static Vector2 RoundToNearestInt(Vector2 value)
	{
		return new Vector2(
			RoundToNearestInt(value.x),
			RoundToNearestInt(value.y)
			);
	}

	public static Vector3 RoundToNearestInt(Vector3 value)
	{
		return new Vector3(
			RoundToNearestInt(value.x),
			RoundToNearestInt(value.y),
			RoundToNearestInt(value.z)
			);
	}
}
