using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public static class CsvExtension
    {
        public static string GetCsvHeader(Type rowType)
        {
            var fields = rowType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            
            var headers = new List<string> { "rowID" };

            foreach (var field in fields)
            {
                if (field.Name.Equals("rowID", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                {
                    continue;
                }
                
                headers.Add(field.Name);
            }

            return string.Join(",", headers);
        }

        public static string GetCsvTemplatePath(string assetPath)
        {
            var directoryPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                Debug.LogError("CsvExtension: Can not found DataTable asset's path.");
                return null;
            }
            
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
            var csvTemplatePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}.csv");

            return csvTemplatePath;
        }

#if UNITY_EDITOR
        public static bool TryCreateCsvTemplate(string templatePath, string csvHeaders, out TextAsset csv)
        {
            try
            {
                File.WriteAllText(templatePath, csvHeaders, System.Text.Encoding.UTF8);
                AssetDatabase.ImportAsset(templatePath);
                
                var newAsset =  AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
                if (newAsset != null)
                {
                    EditorGUIUtility.PingObject(newAsset);
                    DebugExtension.LogColor("CsvExtension: Successfully create CSV template.", Color.green);
                }

                csv = newAsset;
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError($"CsvExtension: Fail to create CSV template: {e.Message}");
            }

            csv = null;
            return false;
        }

        public static List<string> SplitStringList(this string input, char seperator = '|')
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            if (!input.Contains(seperator))
            {
                return new List<string> { input };
            }
            
            return input.Split(seperator)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
        }
    }
#endif
}