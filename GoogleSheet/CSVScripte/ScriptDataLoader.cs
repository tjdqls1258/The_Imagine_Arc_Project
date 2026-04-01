using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ScriptDataLoader<DATA> where DATA : CSVData, new()
{
    readonly static string CSV_PATH = "Assets/Util/GoogleSheet/CSVData/";

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

    private static object ParseFieldRecursive(Type fieldType, string[] dataSplit, ref int columnIndex)
    {
        // 1. 일반 타입 (int, float, string, Enum 등) 처리
        if (IsSimpleType(fieldType))
        {
            string stringData = dataSplit[columnIndex].Trim();
            columnIndex++; // 사용한 만큼 인덱스 증가
            return ConvertSimpleType(stringData, fieldType);
        }

        if(fieldType.IsArray)
        {
            string stringData = dataSplit[columnIndex].Trim();
            columnIndex++;
            return ConvertSimpleType(stringData, fieldType);
        }

        // 2. 중첩 클래스 처리 (Class이며 String이 아닌 경우)
        if (fieldType.IsClass && fieldType != typeof(string))
        {
            object nestedObj = Activator.CreateInstance(fieldType);
            FieldInfo[] nestedFields = fieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var nField in nestedFields)
            {
                if (columnIndex < dataSplit.Length)
                {
                    // 내부 필드들에 대해 다시 재귀 호출
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

public class CSVHelper
{
    private const string PATH = "CSVData/{0}.csv";
    private readonly string PATH_LOCAL = $"{Application.dataPath}/Util/GoogleSheet/CSVData/{{0}}.csv";
    protected Dictionary<Type, object> m_scriptDataList = new();

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

    public void InitCSVData()
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
            var file = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<TextAsset>(string.Format(PATH, data.Item1.ToString()));
            data.Item2.SetDatas(file.text);
            if (m_scriptDataList.ContainsKey(data.Item2.GetType()) == false)
                m_scriptDataList.Add(data.Item2.GetType(), data.Item2);
            else
                Logger.Log($"{data.Item2.GetType()} Same");
        }
    }

    public T GetScripteData<T>() where T : class
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
    protected Dictionary<int, Data> m_dataList = new();

    public virtual void SetDatas(TextAsset file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile((file.text, typeof(Data)), this);
    }

    public virtual void SetDatas(string file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile((file, typeof(Data)), this);
    }

    public virtual Data GetData(int id)
    {
        return m_dataList[id];
    }
}

public interface ICsvListHelper
{
    public void SetDatas(TextAsset csvFile);
    public void SetDatas(string csvFilePath);
}
