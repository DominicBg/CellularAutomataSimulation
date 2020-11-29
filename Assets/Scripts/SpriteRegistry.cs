using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpriteRegistry : MonoBehaviour
{
    public static SpriteRegistry instance;

    public TextureGroup[] textureGroups;

    NativeSprite[] spriteArray;

    Dictionary<SpriteEnum, NativeSprite> dictionary = new Dictionary<SpriteEnum, NativeSprite>();

    public static NativeSprite GetSprite(SpriteEnum spriteEnum)
    {
        return instance.dictionary[spriteEnum];
    }

    void Awake()
    {
        instance = this;

        var textures = GetFlattenTextureGroup();
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

    Texture2D[] GetFlattenTextureGroup()
    {
        List<Texture2D> outputTextures = new List<Texture2D>();
        for (int i = 0; i < textureGroups.Length; i++)
        {
            outputTextures.AddRange(textureGroups[i].textures);
        }
        return outputTextures.ToArray();
    }

    [ContextMenu("Generate Enum")]
    public void GenerateEnum()
    {
        var textures = GetFlattenTextureGroup();
        string[] enumNames = new string[textures.Length];
        for (int i = 0; i < enumNames.Length; i++)
        {
            string name = textures[i].name;
            enumNames[i] = char.IsDigit(name[0]) ? "_" + textures[i].name : textures[i].name;
        }
        ConstFileWriter.GenerateEnumConstFile(this, "SpriteEnum", "SpriteEnum", enumNames, false);
        Debug.Log("Generated SpriteEnum");
    }

    [System.Serializable]
    public class TextureGroup
    {
        public Texture2D[] textures;
    }
}
