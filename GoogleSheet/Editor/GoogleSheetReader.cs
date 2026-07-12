#if UNITY_EDITOR
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class GoogleSheetReader : EditorWindow
{
    public class GenreClass<T> where T : ScriptableObject, IUpdateDataFormSheet, new()
    {
        public void CreateFunc(List<string> datas, string path)
        {
            UpdateDataFormSheet.CreateData<T>(datas, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    private static readonly string ClientName = "user";
    private static readonly string dataPath = $"{UnityEngine.Application.dataPath.Replace("Assets", "")}client_secret_2_633291316510-ao56irbicvfhrm2m9n1k0scean980ufl.apps.googleusercontent.com.json";

    public static SheetsService CreateService()
    {
        var scopes = new string[] { SheetsService.Scope.SpreadsheetsReadonly };

        Debug.Log(dataPath);
        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //pass, 
            GoogleClientSecrets.FromFile($"{dataPath}").Secrets,
            scopes,
            ClientName,
            CancellationToken.None).Result;

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
        });

        return service;
    }

    
    private static string CreateService_CSV(SheetsService service, CSVData csvdata)
    {
        ValueRange value;
        try
        {
            value = service.Spreadsheets.Values.Get(csvdata.SheetID, csvdata.SheetName).Execute();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{csvdata.SheetName} request Error : {ex.Message}");
            return "";
        }

        var values = value.Values;
        int columnCount = values[0].Count;
        List<int> ignoreIndex = new();
        for (int i = 0; i < values[0].Count; i++)
        {
            string title = values[0][i].ToString();
            if (string.IsNullOrEmpty(values[1][i].ToString()))
            {
                continue;
            }
            if (title.IndexOf("(") != -1 || title.IndexOf(")") != -1 ||
                title.IndexOf("[") != -1 || title.IndexOf("]") != -1)
            {
                ignoreIndex.Add(i);
            }
        }

        StringBuilder csvInfo = new StringBuilder();
        for (int x = 1; x < values.Count; x++)
        {
            if (x != 1)
                csvInfo.AppendLine();
            for (int y = 0; y < columnCount; y++)
            {
                if (ignoreIndex.Exists(i => i == y)) continue;

                if (y != 0)
                {
                    csvInfo.Append(",");
                }
                if (values[x].Count <= y)
                    continue;

                string str = values[x][y].ToString();

                if (str.IndexOf(",") != -1)
                {
                    csvInfo.Append("\"");
                    csvInfo.Append(str);
                    csvInfo.Append("\"");
                }
                else
                {
                    csvInfo.Append(str);
                }
            }
        }
        Debug.Log(csvInfo);

        if (csvdata.IsCSV == false)
        {
            string[] csvList = csvInfo.ToString().Split("\n");
            foreach (string str in csvList)
            {
                MakeScriptable(csvdata, str.Split(",").ToList());
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return csvInfo.ToString();
    }

    protected static void MakeScriptable(CSVData data, List<string> csvInfo)
    {
        Type dataType = GetTypeFromAssemblies(data.ClassName);
        Type genreClass = typeof(GenreClass<>);

        if (genreClass == null || dataType == null)
        {
            Debug.LogError("Type Null");
            return;
        }

        Type genericProgramType = genreClass.MakeGenericType(new Type[] { dataType });
        object value = Activator.CreateInstance(genericProgramType);

        object[] args = { csvInfo, SaveScriptableDataPath };
        genericProgramType.GetMethod("CreateFunc")?.Invoke(value, args);
    }

    protected void SheetDataMaker<Data>(string csvInfo) where Data : ScriptableObject, IUpdateDataFormSheet, new()
    {
        List<string> datas = csvInfo.Split("\n").ToList();
        foreach (string s in datas)
        {
            UpdateDataFormSheet.CreateData<Data>(s.Split("\n").ToList(), SaveScriptableDataPath);
        }
    }

    #region Window

    private static string CSVSettingPath = $"{UnityEngine.Application.dataPath}/Util/GoogleSheet/Editor/CSVSettingJson.json";
    private static string CSVSavePath = $"{UnityEngine.Application.dataPath}/Util/GoogleSheet/CSVData/{{0}}.csv";
    private static string SaveScriptableDataPath = $"Assets/Util/GoogleSheet/ScriptableObject";
    private static List<CSVData> CSVDataList = new();
    private static List<bool> toggleList = new();
    private Vector2 scrollPos = Vector2.zero;
    [Serializable]
    public class CSVData
    {
        public string Name;
        public string SheetName;
        public string SheetID;
        public string ClassName;
        public bool IsCSV;
    }

    [MenuItem("Tools/CSV Loader")]
    public static void ShowMyEditor()
    {
        LoadSetting();
        EditorWindow wnd = GetWindow<GoogleSheetReader>();
        wnd.titleContent = new GUIContent("Google Sheet Loader");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < CSVDataList.Count; i++)
        {
            toggleList[i] = EditorGUILayout.BeginToggleGroup($"{CSVDataList[i].Name}" , toggleList[i]);
            GUILayout.Space(1);
            CSVDataList[i].Name = EditorGUILayout.TextField("Name", CSVDataList[i].Name);
            CSVDataList[i].SheetID = EditorGUILayout.TextField("Sheet ID", CSVDataList[i].SheetID);
            CSVDataList[i].SheetName = EditorGUILayout.TextField("Sheet Name", CSVDataList[i].SheetName);
            CSVDataList[i].ClassName = EditorGUILayout.TextField("ClassName", CSVDataList[i].ClassName);
            CSVDataList[i].IsCSV = EditorGUILayout.Toggle("Is CSVData", CSVDataList[i].IsCSV);
            EditorGUILayout.EndToggleGroup();
            if (GUILayout.Button("Remove"))
            {
                Remove(i);
                SaveSetting();
            }

        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add")) 
        {
            AddData();
            SaveSetting();
        }

        if (GUILayout.Button("ConvartCSV"))
        {
            SheetsService service = CreateService();
            for (int i = 0; i < CSVDataList.Count; i++)
            {
                if (toggleList[i])
                {
                    string csvData = CreateService_CSV(service, 
                        CSVDataList[i]);

                    if (CSVDataList[i].IsCSV)
                    {
                        File.WriteAllText(string.Format(CSVSavePath, CSVDataList[i].Name), csvData);
                        SaveSetting();
                    }
                }
            }
        }

        if(GUILayout.Button("Save"))
        {
            SaveSetting();
        }


        void AddData()
        {
            CSVDataList.Add(new());
            toggleList.Add(false);
        }

        void Remove(int index)
        {
            CSVDataList.RemoveAt(index);
            toggleList.RemoveAt(index);
        }
    }

    public static void LoadSetting()
    {
        if(!File.Exists(CSVSettingPath))
        {
            string data = "";
            File.WriteAllText(CSVSettingPath, data);
        }

        string Json =  File.ReadAllText(CSVSettingPath);
        CSVDataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CSVData>>(Json);

        if (CSVDataList == null)
        {
            CSVDataList = new();
            return;
        }

        foreach (CSVData data in CSVDataList)
        {
            toggleList.Add(false);
        }
    }

    public static void SaveSetting()
    {
        string data = Newtonsoft.Json.JsonConvert.SerializeObject(CSVDataList);
        File.WriteAllText(CSVSettingPath, data);
    }
    #endregion

    #region Util
    public static Type GetTypeFromAssemblies(string TypeName)
    {
        var type = Type.GetType(TypeName);
        if (type != null)
            return type;

        var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {
            var assembly = System.Reflection.Assembly.Load(assemblyName);
            if (assembly != null)
            {
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }
        }

        Debug.LogError($"Can't Find Type Check TypeName {TypeName}");
        return null;
    }
    #endregion

}
#endif
