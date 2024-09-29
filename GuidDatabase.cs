using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Greenf1re/Guid Database")]
public class GuidDatabase : ScriptableObject
{
    public int version;
    //public IDictionary<string, string> guids = new Dictionary<string, string>();
    [SerializeField]
    public List<string> guid_key = new List<string>();
    public List<string> guid_value = new List<string>();
    public List<string> guid_newValues = new List<string>();

    protected void SaveAssetPose(List<string> sguid_key, List<string> sguid_value, int version_s)
    {
        #if UNITY_EDITOR
            version = version_s;
            guid_key = sguid_key;
            guid_value = sguid_value; 
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    public void AddGuidPair(string guid_keyS, string guid_valueS){
        #if UNITY_EDITOR
            guid_key.Add(guid_keyS);
            guid_value.Add(guid_valueS);
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        #endif
    }
    public void AddGuidNew(string guid){
        #if UNITY_EDITOR
            guid_newValues.Add(guid);
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        #endif
    }
}
