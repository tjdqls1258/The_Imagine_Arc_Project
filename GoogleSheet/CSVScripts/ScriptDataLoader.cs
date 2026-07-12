using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using VContainer;

public static class ScriptDataLoader<DATA> where DATA : CSVData, new()
{
    public static Dictionary<int, DATA> ReadFile((string, Type) strFileType, ICsvListHelper csvList) 
    {
        string reader = strFileType.Item1;
        string[] dataList = reader.Split("\n");
        int currentLine = 0;
        bool isLast = false;

        Type type = strFileType.Item2;
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<int, DATA> result = new();

        while (!isLast)
        {
            DATA data = new();
            
            if (currentLine >= dataList.Length)
            {
                isLast = true;
                break;
            }

            string datas = dataList[currentLine].Replace("\r", "").Trim();
            currentLine++;

            var dataSplit = datas.Split(',');

            int columnIndex = 0;
            foreach (FieldInfo field in fieldInfos)
            {
                if (columnIndex >= dataSplit.Length) break;

                try
                {
                    object fieldValue = ParseFieldRecursive(field.FieldType, dataSplit, ref columnIndex);
                    field.SetValue(data, fieldValue);
                }
                catch (Exception e)
                {
                    Logger.LogError("ex : " + e.Message);
                    Logger.LogError($"i : {columnIndex}, fieldType : {field}, list[i] : {dataSplit[columnIndex]}");
                }
            }

            result.Add(data.GetID(), data);
        }

        return result;
    }

    public static Dictionary<long, DATA> ReadFile_long((string, Type) strFileType, ICsvListHelper csvList)
    {
        string reader = strFileType.Item1;
        string[] dataList = reader.Split("\n");
        int currentLine = 0;
        bool isLast = false;

        Type type = strFileType.Item2;
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<long, DATA> result = new();

        while (!isLast)
        {
            DATA data = new();

            if (currentLine >= dataList.Length)
            {
                isLast = true;
                break;
            }

            string datas = dataList[currentLine].Replace("\r", "").Trim();
            currentLine++;

            var dataSplit = datas.Split(',');

            int columnIndex = 0;
            foreach (FieldInfo field in fieldInfos)
            {
                if (columnIndex >= dataSplit.Length) break;

                try
                {
                    object fieldValue = ParseFieldRecursive(field.FieldType, dataSplit, ref columnIndex);
                    field.SetValue(data, fieldValue);
                }
                catch (Exception e)
                {
                    Logger.LogError("ex : " + e.Message);
                    Logger.LogError($"i : {columnIndex}, fieldType : {field}, list[i] : {dataSplit[columnIndex]}");
                }
            }

            result.Add(data.GetID(), data);
        }

        return result;
    }
    private static object ParseFieldRecursive(Type fieldType, string[] dataSplit, ref int columnIndex)
    {
        if (IsSimpleType(fieldType))
        {
            string stringData = dataSplit[columnIndex].Trim();
            columnIndex++;
            return ConvertSimpleType(stringData, fieldType);
        }

        if(fieldType.IsArray)
        {
            string stringData = dataSplit[columnIndex].Trim();
            columnIndex++;
            return ConvertSimpleType(stringData, fieldType);
        }

        if (fieldType.IsClass && fieldType != typeof(string))
        {
            object nestedObj = Activator.CreateInstance(fieldType);
            FieldInfo[] nestedFields = fieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var nField in nestedFields)
            {
                if (columnIndex < dataSplit.Length)
                {
                    object nValue = ParseFieldRecursive(nField.FieldType, dataSplit, ref columnIndex);
                    nField.SetValue(nestedObj, nValue);
                }
            }
            return nestedObj;
        }

        return null;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(float);
    }

    private static object ConvertSimpleType(string stringData, Type fieldType, string splitChar = ";")
    {
        if (fieldType.IsEnum)
        {
            if (int.TryParse(stringData, out int enumNumber))
                return Enum.ToObject(fieldType, enumNumber);
            return Enum.Parse(fieldType, stringData);
        }

        if (fieldType.IsArray)
        {
            Type elemnetType = fieldType.GetElementType();
            var datas = stringData.Split(splitChar);

            Array newArray = Array.CreateInstance(elemnetType, datas.Length);

            for(int i = 0; i < datas.Length; i++)
            {
                object value = Convert.ChangeType(datas[i], elemnetType);
                newArray.SetValue(value, i);
            }

            return newArray;
        }

        if (fieldType == typeof(string))
            return stringData;

        return Convert.ChangeType(stringData, fieldType);
    }
}

public class CSVHelper : ICSVProvider, ICSVLoader
{
    [Inject] private AddressableManager addressableManager;
    private const string PATH = "CSVData/{0}.csv";
    private readonly string PATH_LOCAL = $"{Application.dataPath}/Util/GoogleSheet/CSVData/{{0}}.csv";
    private readonly Dictionary<Type, object> m_scriptDataList = new();

    public enum CSVFile
    {
        CharacterData,
        EnemyData
    }

    private readonly (CSVFile, ICsvListHelper)[] m_csvData =
    {
        (CSVFile.CharacterData ,(new CharacterDataList())),
        (CSVFile.EnemyData ,(new EnemyDataList())),
    };

    public void InitLocalData()
    {
        foreach(var data in m_csvData)
        {
            StreamReader reader = new StreamReader(string.Format(PATH_LOCAL, data.Item1.ToString()));
            data.Item2.SetDatas(reader.ReadToEnd());
            m_scriptDataList.Add(data.Item2.GetType(), data.Item2);
        }
    }

    public async UniTask InitCSVDataAsync()
    {
        List<UniTask> TaskList = new(); 
        foreach(var data in m_csvData)
        {
            TaskList.Add(InitItem(data));
        }

        await UniTask.WhenAll(TaskList);

        async UniTask InitItem((CSVFile, ICsvListHelper) data)
        {
            var file = await addressableManager.LoadAssetAndCacheAsync<TextAsset>(string.Format(PATH, data.Item1.ToString()));
            data.Item2.SetDatas(file.text);
            if (m_scriptDataList.ContainsKey(data.Item2.GetType()) == false)
                m_scriptDataList.Add(data.Item2.GetType(), data.Item2);
            else
                Logger.Log($"{data.Item2.GetType()} Same");
        }
    }

    public T GetScriptData<T>() where T : class
    {
        return m_scriptDataList[typeof(T)] as T;
    }
}

public abstract class CSVData
{
    public abstract int GetID();
}

public class CSVDataList<Data> : ICsvListHelper where Data : CSVData, new()
{
    protected Dictionary<long, Data> m_dataList = new();

    public virtual void SetDatas(TextAsset file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile_long((file.text, typeof(Data)), this);
    }

    public virtual void SetDatas(string file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile_long((file, typeof(Data)), this);
    }

    public virtual Data GetData(long id)
    {
        return m_dataList[id];
    }
}

public interface ICsvListHelper
{
    public void SetDatas(TextAsset csvFile);
    public void SetDatas(string csvFilePath);
}

public interface ICSVProvider
{
    T GetScriptData<T>() where T : class;
}

public interface ICSVLoader
{
    UniTask InitCSVDataAsync();
    void InitLocalData();
}
