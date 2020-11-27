using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpriteRegistry : MonoBehaviour
{
    public static SpriteRegistry instance;

    public Texture2D[] textures;

    NativeSprite[] spriteArray;

    Dictionary<SpriteEnum, NativeSprite> dictionary = new Dictionary<SpriteEnum, NativeSprite>();

    public static NativeSprite GetSprite(SpriteEnum spriteEnum)
    {
        return instance.dictionary[spriteEnum];
    }

    void Awake()
    {
        instance = this;
        spriteArray = new NativeSprite[textures.Length];
        for (int i = 0; i < spriteArray.Length; i++)
        {
            spriteArray[i] = new NativeSprite(textures[i]);
            dictionary.Add((SpriteEnum)i, spriteArray[i]);
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < spriteArray.Length; i++)
        {
            spriteArray[i].Dispose();
        }
    }


    [ContextMenu("Generate Enum")]
    public void GenerateEnum()
    {
        string[] enumNames = new string[textures.Length];
        for (int i = 0; i < enumNames.Length; i++)
        {
            string name = textures[i].name;
            enumNames[i] = char.IsDigit(name[0]) ? "_" + textures[i].name : textures[i].name;
        }
        ConstFileWriter.GenerateEnumConstFile(this, "SpriteEnum", "SpriteEnum", enumNames, false);
    }
}
