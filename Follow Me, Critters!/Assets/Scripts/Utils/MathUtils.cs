using UnityEngine;

public class MathUtils
{
	// ����� �ݿø��� �ƴ� ���� ����� ������ �ݿø��ϴ� �Լ�
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
