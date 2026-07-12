#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class UpdateDataFormSheet
{
    public static void CreateData<Data>(List<string> datas, string path) where Data : ScriptableObject, IUpdateDataFormSheet, new()
    {
        string ID = datas[0];
        Data data = SetData<Data>(datas);

        MakeScriptable(data, path, ID);
    }

    public static Data SetData<Data>(string[] datas) where Data : new()
    {
        return SetData<Data>(datas.ToList());
    }

    public static Data SetData<Data>(List<string> datas) where Data : new()
    {
        Data data = new Data();
        var fields = typeof(Data).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++)
        {
            if (datas.Count <= i)
                break;

            FieldInfo field = fields[i];
            Type fieldType = fields[i].FieldType;

            object valueToSet;
            object value = datas[i];

            try
            {
                if (fieldType.IsEnum)
                {
                    if (int.TryParse(datas[i], out int enumNumber))
                        valueToSet = Enum.ToObject(fieldType, enumNumber);
                    else
                        valueToSet = Enum.Parse(fieldType, datas[i]);
                }
                else
                {
                    if (fieldType == typeof(string))//안드로이드 경우 \r가 붙어서 제거.
                        valueToSet = ((string)(Convert.ChangeType(value, fieldType))).Replace("\r", "") ?? string.Empty;
                    else
                        valueToSet = Convert.ChangeType(value, fieldType);
                }

                field.SetValue(data, valueToSet);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError($"i : {i}, fieldType : {fieldType}, datas[i] : {datas[i]}");
            }
        }

        return data;
    }

    public static void MakeScriptable<Data>(Data data, string path, string ID) where Data : ScriptableObject, new()
    {
        Data d = data;
        string foldPath = path + $"/{typeof(Data).Name}";
        if (AssetDatabase.IsValidFolder(foldPath) == false)
        {
            string parentFolder = path.TrimEnd('/');
            AssetDatabase.CreateFolder(parentFolder, typeof(Data).Name);
        }
        string datapath = $"{foldPath}/{typeof(Data).Name}_{ID}.asset";

        Data baseData = AssetDatabase.LoadAssetAtPath<Data>(datapath);
        if(baseData != null)
        {
            FieldInfo[] fields = typeof(Data).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttributes(typeof(SerializeReference), true).Length > 0)
                    continue;

                if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    continue;

                object newValue = field.GetValue(data);
                field.SetValue(baseData, newValue);
            }
            EditorUtility.SetDirty(baseData);
        }
        else
            AssetDatabase.CreateAsset(data, datapath);


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
public interface IUpdateDataFormSheet
{
}
