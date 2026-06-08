using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule.DataTable
{
#region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(DataTable), true)]
    public class DataTableInspector : Editor
    {
        private int _pickerControlID;

        public async override void OnInspectorGUI()
        {
            var dataTable = target as DataTable;
            if (dataTable == null)
            {
                return;
            }
            
            
            if (dataTable.dataTableCsv)
            {
                SyncCsv(dataTable);
                GUILayout.Space(10);
                DrawTableFields(dataTable);
                GUILayout.Space(10);
                if (GUILayout.Button("Update Table", GUILayout.Height(30)))
                {
                    await dataTable.UpdateData(dataTable.dataTableCsv, null);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.ObjectField("DataTableRowBase", dataTable.dataTableRowScript, typeof(MonoScript), false);
                GUI.enabled = true;

                if (GUILayout.Button("Select", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    DataTableRowWindow.Open(selectedScript =>
                    {
                        Undo.RecordObject(dataTable, "Select DataTable Row");

                        dataTable.dataTableRowScript = selectedScript;
                        dataTable.dataTableRowName = selectedScript.GetClass().AssemblyQualifiedName;

                        EditorUtility.SetDirty(dataTable);
                        serializedObject.Update();
                    });
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
                if (GUILayout.Button("Create Table & Sync CSV", GUILayout.Height(30)))
                {
                    dataTable.dataTableCsv = dataTable.CreateCsv();

                    EditorUtility.SetDirty(dataTable);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == _pickerControlID)
                {
                    dataTable.dataTableRowScript = EditorGUIUtility.GetObjectPickerObject() as MonoScript;
                    dataTable.dataTableRowName = dataTable.GetRowName();
                    Event.current.Use();
                    GUI.changed = true;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawTableFields(DataTable dataTable)
        {
            var targetType = dataTable.dataTableRowScript.GetClass();
            
            var boldStyle = new GUIStyle(EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{targetType.Name} has {targetType.GetFields().Length} fields.", boldStyle);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Field", boldStyle, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.45f));
            GUILayout.Label("Type", boldStyle);
            EditorGUILayout.EndHorizontal();
            
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var evenStyle = new GUIStyle(EditorStyles.label);
            var oddStyle = new GUIStyle(EditorStyles.label);
            evenStyle.normal.background = CreateColorTexture(Color.gray);
            oddStyle.normal.background = CreateColorTexture(Color.black);

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var currentStyle = (i % 2 == 0) ? evenStyle : oddStyle;
                
                EditorGUILayout.BeginHorizontal(currentStyle);
                GUILayout.Label(field.Name, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.45f));
                GUILayout.Label(GetTypeName(field.FieldType));
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void SyncCsv(DataTable dataTable)
        {
            if (dataTable.name == dataTable.dataTableCsv.name)
            {
                return;
            }
            
            var path = AssetDatabase.GetAssetPath(dataTable.dataTableCsv);
            AssetDatabase.RenameAsset(path, dataTable.name);
            EditorUtility.SetDirty(dataTable);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetTypeName(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type.IsEnum)
            {
                return "enum";
            }
            else if (type == typeof(Color))
            {
                return "Color";
            }
            
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                var typeArgs = type.GetGenericArguments();

                if (genericType == typeof(System.Collections.Generic.List<>))
                {
                    return $"List<{GetTypeName(typeArgs[0])}>";
                }
            }

            return type.IsArray ? $"{GetTypeName(type.GetElementType())}[]" : type.Name;
        }

        private static Texture2D CreateColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        [MenuItem("Assets/JxModule/Copy Datatable")]
        public static void CopyAsset()
        {
            var asset = Selection.activeObject as DataTable;
            if (asset != null)
            {
                asset.CopyAsset();
            }
        }

        [MenuItem("Assets/JxModule/Validate DataTable", true)]
        public static bool ValidateCopyAsset()
        {
            return Selection.activeObject is DataTable;
        }
    }
#endif
#endregion Editor
    
    public class DataTable : ScriptableObject
    {
        [ReadOnly] public TextAsset dataTableCsv;
        [ReadOnly] public string dataTableRowName;
        [HideInInspector] public List<DataTableRowBase> dataTableRows = new();
     
        public string GetRowName()
        {
            return dataTableRowName;
        }
        
        public List<T> Get<T>() where T : DataTableRowBase
        {
            return dataTableRows.ConvertAll<T>(x => x as T);
        }

        public T Find<T>(string rowID) where T : DataTableRowBase
        {
            return Get<T>().Find(x => x.rowID == rowID);
        }

        public T Find<T>(Predicate<T> predicate) where T : DataTableRowBase
        {
            return Get<T>().Find(predicate);
        }

        public List<T> FindAll<T>() where T : DataTableRowBase
        {
            return Get<T>();
        }

        public List<T> FindAll<T>(Predicate<T> predicate) where T : DataTableRowBase
        {
            return Get<T>().FindAll(predicate);
        }

#region Editor
#if UNITY_EDITOR
        [ReadOnly] public MonoScript dataTableRowScript;
        
        [MenuItem("Assets/JxModule/Create Datatable")]
        public static void Create()
        {
            AssetExtension.CreateAsset<DataTable>("DataTable");
        }

        public TextAsset CreateCsv()
        {
            var rowType = dataTableRowScript.GetClass();
            var csvHeaders = CsvExtension.GetCsvHeader(rowType);
            
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("DataTable: Can not found DataTable asset's path.");
                return null;
            }
            
            var csvTemplatePath = CsvExtension.GetCsvTemplatePath(assetPath);
            return !CsvExtension.TryCreateCsvTemplate(csvTemplatePath, csvHeaders, out var csv) ? null : csv;
        }

        public void CopyAsset()
        {
            var originAssetPath = AssetDatabase.GetAssetPath(this);
            var originCsvPath = AssetDatabase.GetAssetPath(dataTableCsv);

            var copyAssetPath = AssetDatabase.GetAssetPath(this).Replace(".asset", "_copy.asset");
            var copyCsvPath = AssetDatabase.GetAssetPath(dataTableCsv).Replace(".asset", "_copy.asset");
            
            var copyAsset = AssetDatabase.LoadAssetAtPath<DataTable>(copyAssetPath);
            var copyCsv = AssetDatabase.LoadAssetAtPath<TextAsset>(copyCsvPath);
            copyAsset.dataTableCsv = copyCsv;
            
            EditorUtility.SetDirty(copyAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static async Task<System.Object> ProcessGenericType(Type fieldType, string rawValue)
        {
            var elementType = fieldType.GetGenericArguments()[0];

            var list = rawValue.SplitStringList();
            System.Object result = null;
            
            if (elementType == typeof(string))
            {
                result = new List<string>(list.ConvertAll(x => x.ToString()));
            }
            else if (elementType == typeof(int))
            {
                result = list.ConvertAll(int.Parse);
            }
            else if (elementType == typeof(float))
            {
                result = list.ConvertAll(float.Parse);
            }
            else if (elementType == typeof(bool))
            {
                result = list.ConvertAll(bool.Parse);
            }
            else if (elementType == typeof(Color))
            {
                var colorList = new List<Color>();

                foreach (var item in list)
                {
                    if (ColorUtility.TryParseHtmlString(item, out var color))
                    {
                        colorList.Add(color);
                    }
                    else
                    {
                        Debug.LogError($"DataTable: Fail to process Color: {item}");
                    }
                }

                result = colorList;
            }
            else if (elementType.IsEnum)
            {
                var targetListType = typeof(List<>).MakeGenericType(elementType);
                var targetList = Activator.CreateInstance(targetListType);
                var addMethod = targetListType.GetMethod("Add");
                foreach (var item in list)
                {
                    addMethod?.Invoke(targetList, new object[] { Enum.Parse(elementType, item) });
                }

                result = targetList;
            }
            else if (typeof(MonoBehaviour).IsAssignableFrom(elementType))
            {
                var targetListType = typeof(List<>).MakeGenericType(elementType);
                var targetList = Activator.CreateInstance(targetListType);
                var addMethod = targetListType.GetMethod("Add");
                foreach (string item in list)
                {
                    await AddressableExtension.LoadAsset<GameObject>(item, 
                        x => { addMethod.Invoke(targetList, new object[] { x.GetComponent(elementType) }); });
                }
                
                result = targetList;
            }
            else if (typeof(System.Object).IsAssignableFrom(elementType))
            {
                var targetListType = typeof(List<>).MakeGenericType(elementType);
                var targetList = Activator.CreateInstance(targetListType);
                var addMethod = targetListType.GetMethod("Add");
                foreach (string item in list)
                {
                    await AddressableExtension.LoadAsset<System.Object>(item,
                        x => { addMethod.Invoke(targetList, new object[] { x }); });
                }

                result = targetList; 
            }

            return result;
        }
        
        public async Task UpdateData(TextAsset csv, Action<object, Dictionary<string, string>> customAction)
        {
            dataTableRowName = GetRowType().AssemblyQualifiedName;
            
            var tableRows = new List<DataTableRowBase>();
            var csvContents = CSVReader.Read(csv);

            var rowCount = 1;
            foreach (var row in csvContents)
            {
                var isEnable = bool.Parse(row["isEnable"]);
                if (!isEnable)
                {
                    continue;
                }

                var targetRow = dataTableRows.Find(x => x.rowID == row["rowID"]);
                if (!targetRow)
                {
                    targetRow = CreateInstance(dataTableRowScript.GetClass()) as DataTableRowBase;
                    AssetDatabase.AddObjectToAsset(targetRow, this);
                    EditorUtility.SetDirty(this);
                }
                targetRow.SetRawData(row);

                var type = dataTableRowScript.GetClass();
                var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in allFields)
                {
                    if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                    {
                        continue;
                    }
                    
                    var key = row.Keys.FirstOrDefault(k => k.Equals(field.Name, StringComparison.OrdinalIgnoreCase));
                    if (key == null)
                    {
                        return;
                    }

                    var rawValue = row[key];
                    var fieldType = field.FieldType;

                    try
                    {
                        object parsedValue = null;

                        if (fieldType.IsGenericType)
                        {
                            parsedValue = await ProcessGenericType(fieldType, rawValue);
                            field.SetValue(targetRow, parsedValue);
                        }
                        else
                        {
                            if (fieldType == typeof(string))
                            {
                                parsedValue = rawValue;
                                field.SetValue(targetRow, parsedValue);
                            }
                            else if (fieldType == typeof(int))
                            {
                                parsedValue = int.Parse(rawValue);
                                field.SetValue(targetRow, parsedValue);
                            }
                            else if (fieldType == typeof(float))
                            {
                                parsedValue = float.Parse(rawValue);
                                field.SetValue(targetRow, parsedValue);
                            }
                            else if (fieldType == typeof(bool))
                            {
                                parsedValue = bool.Parse(rawValue);
                                field.SetValue(targetRow, parsedValue);
                            }
                            else if (fieldType == typeof(Color))
                            {
                                if (ColorUtility.TryParseHtmlString(rawValue, out var color))
                                {
                                    parsedValue = color;
                                    field.SetValue(targetRow, parsedValue);
                                }
                                else
                                {
                                    Debug.LogWarning($"DataTable: Color parse failed: {rawValue}");
                                }
                            }
                            else if (fieldType.IsEnum)
                            {
                                parsedValue = Enum.Parse(fieldType, rawValue);
                                field.SetValue(targetRow, parsedValue);
                            }
                            else if (typeof(MonoBehaviour).IsAssignableFrom(fieldType))
                            {
                                await AddressableExtension.LoadAsset<GameObject>(rawValue,
                                    x =>
                                    {
                                        parsedValue = x.GetComponent(fieldType);
                                        field.SetValue(targetRow, parsedValue);
                                    });
                            }
                            else if (typeof(System.Object).IsAssignableFrom(fieldType))
                            {
                                await AddressableExtension.LoadAsset<System.Object>(rawValue, x => { field.SetValue(targetRow, x); });
                            }   
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"DataTable: Fail to update [{name}]-[Column : {field.Name}, Row : {rowCount}] - {e.Message}");
                    }
                }
                customAction?.Invoke(targetRow, row);
                targetRow.name = targetRow.rowID;
                
                EditorUtility.SetDirty(targetRow);
                EditorUtility.SetDirty(this);
                tableRows.Add(targetRow);
                rowCount++;
            }
            
            var path = AssetDatabase.GetAssetPath(this);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in allAssets)
            {
                if (asset == this)
                {
                    continue;
                }

                if (asset is not DataTableRowBase oldRow)
                {
                    continue;
                }

                var existsInLatest = tableRows.Exists(newRow => newRow.rowID == oldRow.rowID);

                if (!existsInLatest || string.IsNullOrEmpty(oldRow.rowID) || string.IsNullOrEmpty(oldRow.name))
                {
                    DestroyImmediate(oldRow, true);
                }
            }

            dataTableRows = tableRows;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            DebugExtension.LogColor("DataTable: Successfully update DataTable.", Color.green);
        }

        public Type GetRowType()
        {
            return dataTableRowScript.GetClass();
        }
#endif
#endregion Editor
    }
}