using Unity.Mathematics;
using UnityEngine;

public class MapDrawerManager : MonoBehaviour
{
    public ParticleType type;
    public int brushSize = 2;

    public GameLevelManager gameLevelManager;

    static Map m_map;

    public static void SetMap(Map map)
    {
        m_map = map;
    }

    // Update is called once per frame
    void Update()
    {
        if (false) //Input.GetMouseButton(0))
        {
            int2 pos = GridPicker.GetGridPosition(GameManager.RenderSizes);

            int halfSize = brushSize / 2;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    m_map.SetParticleType(new int2(pos.x + x, pos.y + y), type);
                }
            }
        }
    }
}
