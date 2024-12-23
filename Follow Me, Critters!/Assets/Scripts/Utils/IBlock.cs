using UnityEngine;

public interface IBlock
{
	Define.BlockType BlockType { get; }
}

public interface IExplodable
{
	public void Explode();
}

public interface ICarrable
{
	public void Carry();
}

public interface IConnectable
{
	public void Connect();
}