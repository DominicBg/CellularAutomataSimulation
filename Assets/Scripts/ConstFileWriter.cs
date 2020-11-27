using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ConstFileWriter
{
    const string fileType = ".cs";

    public static void GenerateConstFile(Object currentFile, string constFileName, string[] strings, int[] values)
    {
        System.Func<int, string> function = (i) => values[i].ToString();
        InternalGenerateConstFile(currentFile.GetType().ToString(), constFileName, strings, function, "int");
    }

    public static void GenerateConstFile(Object currentFile, string constFileName, string[] strings, bool[] values)
    {
        System.Func<int, string> function = (i) => values[i] == true ? "true" : "false";
        InternalGenerateConstFile(currentFile.GetType().ToString(), constFileName, strings, function, "bool");
    }

    public static void GenerateConstFile(Object currentFile, string constFileName, string[] strings, string[] values)
    {
        System.Func<int, string> function = (i) => AddStringSurrounding(values[i]);
        InternalGenerateConstFile(currentFile.GetType().ToString(), constFileName, strings, function, "string");
    }

    public static void GenerateConstFile(Object currentFile, string constFileName, string[] strings)
    {
        System.Func<int, string> function = (i) => AddStringSurrounding(strings[i]);
        InternalGenerateConstFile(currentFile.GetType().ToString(), constFileName, strings, function, "string");
    }

    public static void GenerateEnumConstFile(Object currentFile, string constFileName, string enumName,string[] enumNames, bool insideOfClass = false)
    {
        string path = GetPath(currentFile.GetType().ToString(), constFileName);
        string content;
        if(insideOfClass)
        {
            content = GetFileEnumContent(constFileName, enumName, enumNames);
        }
        else
        {
            content = GetFileEnumContentNoClass(enumName, enumNames);
        }
        WriteToFile(path, content);
    }

    static void InternalGenerateConstFile(string currentFile, string constFileName, string[] strings, System.Func<int, string> function, string type)
    {
        string path = GetPath(currentFile, constFileName);
        string content = GetFileContent(constFileName, strings, function, type);
        WriteToFile(path, content);
    }

    #region writer
    static string AddStringSurrounding(string value)
    {
        return "\"" + value + "\"";
    }

    static void WriteToFile(string path, string content)
    {
        using (System.IO.FileStream fs = System.IO.File.Create(path))
        {
            char[] fileContentByte = content.ToCharArray();
            for (int i = 0; i < fileContentByte.Length; i++)
            {
                fs.WriteByte((byte)fileContentByte[i]);
            }
            fs.Close();
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    static string GetFileContent(string constFileName, string[] strings, System.Func<int, string> function, string type)
    {
        StringBuilder fileContent = new StringBuilder();
        fileContent.Append("public static class " + constFileName + " \n{ \n");
        for (int i = 0; i < strings.Length; i++)
        {
            fileContent.Append("        "); //for indentation
            fileContent.Append("public const "+ type + " ");
            fileContent.Append(strings[i]);
            fileContent.Append(" = " + function(i) + "; \n");

        }
        fileContent.Append("}");

        return fileContent.ToString();
    }

    static string GetFileEnumContentNoClass(string enumName, string[] strings)
    {
        StringBuilder fileContent = new StringBuilder();
        fileContent.Append("    public enum " + enumName + "{");
        for (int i = 0; i < strings.Length; i++)
        {
            fileContent.Append(strings[i]);
            if (i < strings.Length - 1)
            {
                fileContent.Append(", ");
            }
            else
            {
                fileContent.Append("}");
            }
        }
        return fileContent.ToString();
    }

    static string GetFileEnumContent(string constFileName, string enumName, string[] strings)
    {
        StringBuilder fileContent = new StringBuilder();
        fileContent.Append("public static class " + constFileName + " \n{ \n");
        fileContent.Append("    public enum " + enumName + "{");
        for (int i = 0; i < strings.Length; i++)
        {
            fileContent.Append(strings[i]);
            if(i < strings.Length-1)
            {
                fileContent.Append(", ");
            }
            else
            {
                fileContent.Append("}");
            }
        }
        fileContent.Append("\n}");
        return fileContent.ToString();
    }

    public static string GetPath(string currentFileName, string constFileName)
    {
        string[] paths = System.IO.Directory.GetFiles(Application.dataPath, currentFileName + fileType, System.IO.SearchOption.AllDirectories);
        string path = paths[0].Replace(currentFileName+ fileType, "").Replace("\\", "/");
        return path + constFileName + fileType;
    }
    public static string GetPath(string currentFileName, string constFileName, string fileType)
    {
        string[] paths = System.IO.Directory.GetFiles(Application.dataPath, currentFileName + fileType, System.IO.SearchOption.AllDirectories);
        string path = paths[0].Replace(currentFileName+ fileType, "").Replace("\\", "/");
        return path + constFileName + fileType;
    }
    #endregion
}
