using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "LevelsGenerator/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    public List<BallType> BallTypes;
    public int MaxEmptyTubeCount;
}
