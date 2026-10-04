using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorMap/StructureData")]
public class StructureData : ScriptableObject
{
	public List<Structure> Structures = new();

	public List<Structure> Doors = new();

	public string StructureID;

	public Vector2Int Size;

	// �ж������]Normal�BBoss�BBridge...�^
	public RoomCategory Category;

	// �T�w�γ~�]Boss�СBSpawn��...�^
	public RoomRole PresetRole = RoomRole.None;

	// �������d
	public StageType Stage;

	// �Q�⤤���v��
	public int Weight = 1;

	// �ж��S��
	public List<RoomTag> Tags = new();
}
public enum RoomTag
{
	None,

	// �a��
	DeadEnd,       // ����
	Junction,      // �ø�
	Corridor,      // �����D

	// �j�p
	Small,
	Medium,
	Large,

	// �S��
	Indoor,
	Outdoor,

	// �O�d
	Secret,
	Puzzle
}