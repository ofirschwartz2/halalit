using Assets.Enums;
using Assets.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class JsonToInspector
{

    /*
     * WHEN YOU WANT TO SAVE TO JSON - ADD THIS TO THE CLASS YOU WANT TO SAVE


    private void Awake()
    {
        JsonToInspector.SaveToJson(this);
    }


    */

    public static void SaveToJson(object obj)
    {
        string json = JsonUtility.ToJson(obj);
        string filepath = Application.persistentDataPath + obj.GetType().Name + ".json";
        Debug.Log("Saving to " + filepath);
        System.IO.File.WriteAllText(filepath, json);
    }

    /* 
     * WHEN YOU WANT TO LOAD FROM JSON - ADD THIS TO THE CLASS YOU WANT TO LOAD FROM JSON
     *  HalalitItemRankPicker
     *  
    [InitializeOnLoadMethod]
    private static void LoadHalalitItemRankPickerFromJsonOnEditorLoad()
    {
        Debug.Log("Loading from json");
        string filepath = "C:\\Users\\Ofir\\AppData\\LocalLow\\DefaultCompany\\HalalitItemRankPicker.json";
        string json = System.IO.File.ReadAllText(filepath);

        var itemRankPicker = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription()).GetComponent<ItemRankPicker>();
        JsonUtility.FromJsonOverwrite(json, itemRankPicker);
    }


    */


}
