using Unity.Mathematics;
using UnityEngine;

public class MapDrawerManager : MonoBehaviour
{
    public ParticleType type;
    public int brushSize = 2;

    public CellularAutomata cellularAutomata;
    public GridPicker gridPicker;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            int2 pos = gridPicker.GetGridPosition(cellularAutomata.sizes);

            int halfSize = brushSize / 2;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    cellularAutomata.map.SetParticleType(new int2(pos.x + x, pos.y + y), type);
                }
            }
        }
    }
}
