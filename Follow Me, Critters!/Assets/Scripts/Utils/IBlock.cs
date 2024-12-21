using UnityEngine;

public interface IBlock
{
	Define.BlockType BlockType { get; }
	public void Contact();
	public void WrapMove();
}
