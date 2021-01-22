//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(LevelContainer))]
//public class LevelContainerData : MonoBehaviour
//{
//    [HideInInspector] public ParticleType[] grid = new ParticleType[GameManager.GridLength];

//    public void SaveGrid(ParticleType[,] particleGrid)
//    {
//        grid = ArrayHelper.GetArrayFromGrid(particleGrid, GameManager.GridSizes);
//    }

//    public Map LoadMap()
//    {
//        if (grid.Length != GameManager.GridLength)
//            grid = new ParticleType[GameManager.GridLength];

//        ParticleType[,] particleGrid = ArrayHelper.GetGridFromArray(grid, GameManager.GridSizes);
//        return new Map(particleGrid, GameManager.GridSizes);
//    }

//    public ParticleType[,] LoadGrid()
//    {
//        return ArrayHelper.GetGridFromArray(grid, GameManager.GridSizes);
//    }
//}
