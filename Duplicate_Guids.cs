using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
// Greenf1re
// Instructions:
// 1. On old SDK (where prefab's scripts aren't missing) right click prefab, then 1. Generate GUID Database
// 2. Copy generated database to new SDK. Right click, Set as GuidDB
// 3. Update GuidDB to V3
// 4. Right click on prefabs to fix, then "Fix by V3"  
public class Duplicate_Guids : MonoBehaviour
{
    static List<string> guids = new List<string>();
    static GuidDatabase guidDB;
    public static string DB_Path;
#if UNITY_EDITOR
    public static void ImportMaterialsRightClick()
    {
        guids = new List<string>();
        DB_Path = "Assets/Editor/Textures.asset";
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                string cur_guid = AssetDatabase.AssetPathToGUID(path);
                guids.Add(cur_guid);
                // Debug.Log(path);
            }
            catch { }
        }
    }
    // Sets selected asset path as DB_Path
    [MenuItem("Assets/Greenf1re/GuidDb/Set as GuidDB")]
    public static void SetAsGuidDB()
    {
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                // check if it's a GuidDatabase asset
                if (path.Substring(path.LastIndexOf("."), path.Length - path.LastIndexOf(".")) != ".asset") return;
                DB_Path = path;
                Debug.Log("SET DB Path: " + DB_Path);
            }
            catch { }
        }
    }
    
    [MenuItem("Assets/Greenf1re/Get Duplicate Guids")]
    public static void FindDuplicates()
    {
        ImportMaterialsRightClick();
        guidDB = AssetDatabase.LoadAssetAtPath<GuidDatabase>(DB_Path);
        foreach (string guid in guids)
        {
            if(guid == guids[0]) continue;
            else guidDB.AddGuidPair(guid, guids[0]); 
       }
    }

    /*[MenuItem("Assets/Greenf1re/Fix Asset Guids")]
    public static void FixGuidRefs()
    {
        // DB_Path = "Assets/Plugins/Greenf1re/Textures.asset";
        if(!guidDB){
            Debug.Log("Loading GUID DB");
            guidDB = AssetDatabase.LoadAssetAtPath<GuidDatabase>(DB_Path);
        } 

        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                string text = File.ReadAllText(path);
                var (indexes, newYaml) = AllIndexesOf(text);
                File.WriteAllText(path, newYaml);
            }
            catch{Debug.Log("Error in " + AssetDatabase.GetAssetPath(o));}
        }
        AssetDatabase.Refresh();
        //
    }*/

    // Generates a GUID database from selected assets
    [MenuItem("Assets/Greenf1re/1. Generate Guid Database")]
    public static void ShowUniqueGuids()
    {
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                guidDB = CreateNewDatabase(path.Substring(0, path.LastIndexOf(".")) + ".asset");
                guidDB.version = 2;
                string text = File.ReadAllText(path);
                List<string> newYaml = GetUniqueGuidList(text);
                foreach (string guid in newYaml){
                    string guidAssetPath = GetAssetPath(guid);
                    if(guidAssetPath.Length < 1){
                        Debug.Log("Not found\n" + guid);
                        continue;
                    }
                    Debug.Log(guidAssetPath.Substring(guidAssetPath.LastIndexOf("/") + 1 ,guidAssetPath.Length - guidAssetPath.LastIndexOf("/") - 1) + "\nAsset: " + guid);
                    guidDB.AddGuidPair(guidAssetPath.Substring(guidAssetPath.LastIndexOf("/") + 1 ,guidAssetPath.Length - guidAssetPath.LastIndexOf("/") - 1), guid);
                }
                // File.WriteAllText(path, newYaml);
            }
            catch{Debug.Log("Error in " + AssetDatabase.GetAssetPath(o));}
        }
    }
    [MenuItem("Assets/Greenf1re/GuidDb/Update GuidDB to V3")]
    public static void UpdateToV3()
    {
        // iterate through guidb assets and find guids based on script names
        guidDB = AssetDatabase.LoadAssetAtPath<GuidDatabase>(AssetDatabase.GetAssetPath(Selection.objects[0]));
        if(!guidDB || guidDB.version != 2){
            Debug.Log("Error loading DB " + guidDB);
            return;
        }
        foreach (var guid in guidDB.guid_key)
        {
            if(guid.LastIndexOf(".cs") > 0){
                Debug.Log("Script " + guid);
                var itemsss = AssetDatabase.FindAssets(guid.Substring(0, guid.LastIndexOf(".")) + " t:script");
                if(itemsss.Length == 0){
                    Debug.Log("Adding NOT_FOUND guid");
                    guidDB.AddGuidNew("NOT_FOUND");
                }
                else
                foreach(string item in itemsss){
                    string assetPath = GetAssetPath(item);
                    string testString = assetPath.Substring(assetPath.LastIndexOf("/") + 1, assetPath.Length - assetPath.LastIndexOf("/") - 1);
                    // Debug.Log("TESTING " + testString + " == "  + newGuidAsset);
                    if(testString == guid){
                        Debug.Log(itemsss.Length +" "+ item + " " + assetPath);
                        guidDB.AddGuidNew(item);
                        break;
                    }
                }
                
            }
            else{
                // add blank guid
                Debug.Log("Adding NOT_SCRIPT guid");
                guidDB.AddGuidNew("NOT_SCRIPT");
            }
        }
        guidDB.version = 3;
    }
    [MenuItem("Assets/Greenf1re/Fix From GuidDB v2")]
    public static void FixFromGuidDbv2()
    {
        foreach (Object o in Selection.objects)
        {
            try
            {
                Debug.Log("Starting search");
                var path = AssetDatabase.GetAssetPath(o);
                guidDB = AssetDatabase.LoadAssetAtPath<GuidDatabase>(path);
                if(guidDB.version != 2){
                    Debug.Log("Db Version Error");
                    return;
                }
                try
                {
                    string text = File.ReadAllText(path.Substring(0, path.LastIndexOf(".")) + ".prefab");
                    // FixByGuidDbV2(text);
                    //;
                    //var (indexes, newYaml) = AllIndexesOf(text);
                    File.WriteAllText(path.Substring(0, path.LastIndexOf(".")) + ".prefab", FixByGuidDbV2(text));
                }
                catch{Debug.Log("Error in " + AssetDatabase.GetAssetPath(o));}
            }
            catch{Debug.Log("Error loading " + AssetDatabase.GetAssetPath(o));}
        }
    }

    [MenuItem("Assets/Greenf1re/Fix Guids V3")]
    public static void FixGuidRefsV3()
    {
        Debug.Log("Starting searchV3, DB = " + DB_Path);
        guidDB = AssetDatabase.LoadAssetAtPath<GuidDatabase>(DB_Path);
        if(!guidDB || guidDB.version != 3){
            Debug.Log("Db Version Error");
            return;
        }
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                Debug.Log("PATH " + path);
                if (path.Contains(".prefab")){
                    string text = File.ReadAllText(path.Substring(0, path.LastIndexOf(".")) + ".prefab");
                    File.WriteAllText(path.Substring(0, path.LastIndexOf(".")) + ".prefab", FixByV3(text, true));
                }
                else if (path.Contains(".asset")){ 
                    Debug.Log("FIXING Asset " + path);
                    string text = File.ReadAllText(path.Substring(0, path.LastIndexOf(".")) + ".asset");
                    File.WriteAllText(path.Substring(0, path.LastIndexOf(".")) + ".asset", FixByV3(text, true));
                }
                else if(path.Contains(".unity")){
                    Debug.Log("FIXING Scene " + path);
                    string text = File.ReadAllText(path);
                    File.WriteAllText(path, FixByV3(text, true));
                }
                // var (indexes, newYaml) = FixByV3(text);
            }
            catch{
                Debug.Log("Error in " + AssetDatabase.GetAssetPath(o));
                // print caught error
            }
        }
        AssetDatabase.Refresh();
        //
    }
    // For every selected asset, shows GUIDs inside it
    [MenuItem("Assets/Greenf1re/Inspect/Show Guids In Asset")]
    public static void GetAssetGuids()
    {
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                string text = File.ReadAllText(path);
                Debug.Log("Current ASSET: ");
                AllIndexesOf(text,true);
            }
            catch{}
        }
    }

    // Shows GUID of selected assets
    [MenuItem("Assets/Greenf1re/Inspect/Show Asset Guid")]
    public static void ShowGuids()
    {
        foreach (Object o in Selection.objects)
        {
            try
            {
                var path = AssetDatabase.GetAssetPath(o);
                string cur_guid = AssetDatabase.AssetPathToGUID(path);
                Debug.Log("Asset PATH: " + path);
                Debug.Log(cur_guid);
            }
            catch{}
        }
    }
    static string GetAssetPath(string guid)
    {
        string p = AssetDatabase.GUIDToAssetPath(guid);
        if (p.Length == 0) p = "not found";
        return p;
    }

    static GuidDatabase CreateNewDatabase(string path){
        GuidDatabase nGuidDb = ScriptableObject.CreateInstance<GuidDatabase>();
        AssetDatabase.CreateAsset(nGuidDb, path);
        return nGuidDb;
    }
    public static (List<int> indexesL, string fixedL) AllIndexesOf(string str, bool verbose = false)
    {
        string value = "guid: ";
        List<int> indexes = new List<int>();
        for (int index = 0;; index += value.Length) {
            index = str.IndexOf(value, index);
            if (index == -1){
                return (indexes, str);
            }
            indexes.Add(index);//Found an instance of "guid: ". Now we copy next 32 characters and look for them in the GuidDatabase
            string toReplace = str.Substring(index + value.Length, 32); //Handle not finding the guid in guid_DB
            if(verbose) Debug.Log(toReplace);
            try{
                int guidToReplace_Index = guidDB.guid_key.IndexOf(toReplace);
                if (guidToReplace_Index < 0) continue;
                else{
                    string newGuid = guidDB.guid_value[guidToReplace_Index];
                    str = str.Replace(toReplace, newGuid);
                }
                }catch{}
        }
    }
    public static List<string> GetUniqueGuidList(string str, bool verbose = false)
    {
        string value = "guid: ";
        List<string> indexes = new List<string>();
        for (int index = 0;; index += value.Length) {
            index = str.IndexOf(value, index);
            if (index == -1){
                return indexes;
            }
            //Found an instance of "guid: ". Now we copy next 32 characters and look for them in the GuidDatabase
            string toReplace = str.Substring(index + value.Length, 32); //Handle not finding the guid in guid_DB
            if(!indexes.Exists(x => x== toReplace))
            indexes.Add(toReplace);
            if(verbose) Debug.Log(toReplace);
            try{
                int guidToReplace_Index = guidDB.guid_key.IndexOf(toReplace);
                if (guidToReplace_Index < 0) continue;
                else{
                    string newGuid = guidDB.guid_value[guidToReplace_Index];
                    str = str.Replace(toReplace, newGuid);
                }
                }catch{}
        }
    }

    public static string FixByGuidDbV2(string str, bool verbose = false)
    {
        string value = "guid: ";
        List<int> indexes = new List<int>();
        for (int index = 0;; index += value.Length) {
            index = str.IndexOf(value, index);
            if (index == -1){
                return str;
            }
            indexes.Add(index);//Found an instance of "guid: ". Now we copy next 32 characters and look for them in the GuidDatabase
            string toReplace = str.Substring(index + value.Length, 32); //Handle not finding the guid in guid_DB
            if(verbose) Debug.Log(toReplace);
            try{
                int guidToReplace_Index = guidDB.guid_key.IndexOf(toReplace);
                if (guidToReplace_Index < 0) continue;
                else{
                    string newGuidAsset = guidDB.guid_value[guidToReplace_Index];
                    // Debug.Log(newGuidAsset +" "+ newGuidAsset.LastIndexOf(".") + " " + newGuidAsset.Length);
                    // Debug.Log("End " + newGuidAsset.Substring(newGuidAsset.LastIndexOf("."), newGuidAsset.Length - 1));
                    //Find guid by asset name
                    if(newGuidAsset.LastIndexOf(".cs") > 0){
                        Debug.Log("Script " + newGuidAsset);
                        var itemsss = AssetDatabase.FindAssets(newGuidAsset.Substring(0, newGuidAsset.LastIndexOf(".")) + " t:script");
                        if(itemsss.Length == 0){
                            Debug.Log("NOT FOUND " + newGuidAsset);
                        }
                        else
                        foreach(string item in itemsss){
                            string assetPath = GetAssetPath(item);
                            string testString = assetPath.Substring(assetPath.LastIndexOf("/") + 1, assetPath.Length - assetPath.LastIndexOf("/") - 1);
                            // Debug.Log("TESTING " + testString + " == "  + newGuidAsset);
                            if(testString == newGuidAsset){
                                Debug.Log(itemsss.Length +" "+ item + " " + assetPath);
                                str = str.Replace(toReplace, item);
                                break;
                            }
                        }
                        
                    }
                }
                }catch{}
        }
    }

    public static string FixByV3(string str, bool verbose = false)
    {
        string value = "guid: ";
        List<int> indexes = new List<int>();
        for (int index = 0;; index += value.Length) {
            index = str.IndexOf(value, index);
            if (index == -1){
                Debug.Log("No GUIDs found");
                return str;
            }
            // Debug.Log("Index " + index);
            indexes.Add(index);//Found an instance of "guid: ". Now we copy next 32 characters and look for them in the GuidDatabase
            string toReplace = str.Substring(index + value.Length, 32); //Handle not finding the guid in guid_DB
            
            if(verbose) Debug.Log("REPLACING " + toReplace);
            try{
                int guidToReplace_Index = guidDB.guid_value.IndexOf(toReplace);
                if (guidToReplace_Index < 0){
                    Debug.LogError("NOT FOUND " + toReplace);
                    continue;
                }
                if( guidDB.guid_newValues[guidToReplace_Index] == "NOT_SCRIPT") continue;
                else{
                    Debug.Log("REPLACING " + toReplace + " with " + guidDB.guid_newValues[guidToReplace_Index]);//+ guidDB.guid_newValues[guidToReplace_Index]);
                    string newGuid = guidDB.guid_newValues[guidToReplace_Index];
                    str = str.Replace(toReplace, newGuid);
                }
            }
            catch{}
        }
    }
#endif
    
}
