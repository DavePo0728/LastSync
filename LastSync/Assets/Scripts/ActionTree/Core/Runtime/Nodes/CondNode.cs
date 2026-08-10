using System;

namespace ActionTree
{
	[Serializable]
	public readonly struct CondNode : IEquatable<CondNode>
	{
		public bool Value { get; }

		public CondNode(bool value)
		{
			Value = value;
		}

		public static implicit operator bool(CondNode node)
		{
			return node.Value;
		}

		public static implicit operator CondNode(bool value)
		{
			return new CondNode(value);
		}

		public bool Equals(CondNode other)
		{
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			return obj is CondNode other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static bool operator ==(CondNode left, CondNode right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CondNode left, CondNode right)
		{
			return !left.Equals(right);
		}
	}
}