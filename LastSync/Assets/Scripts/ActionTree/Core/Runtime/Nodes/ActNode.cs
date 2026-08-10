using System;

namespace ActionTree
{
	[Serializable]
	public readonly struct ActNode : IEquatable<ActNode>
	{
		public bool Value { get; }

		public ActNode(bool value)
		{
			Value = value;
		}

		public static implicit operator bool(ActNode node)
		{
			return node.Value;
		}

		public static implicit operator ActNode(bool value)
		{
			return new ActNode(value);
		}

		public bool Equals(ActNode other)
		{
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			return obj is ActNode other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static bool operator ==(ActNode left, ActNode right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ActNode left, ActNode right)
		{
			return !left.Equals(right);
		}
	}
}