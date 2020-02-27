using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

public class SaveAndLoadAssetWithMenu : EditorWindow
{
    struct DeserializeTransform
    {
        [JsonProperty("pos")]   public Vector3 P;
        [JsonProperty("rot")]   public Vector3 R;
        [JsonProperty("scale")] public Vector3 S;
    }
    
    [MenuItem("Plugins/BuildingCreationEditor")]
    static void OpenMenu() 
    {
        SaveAndLoadAssetWithMenu contextMenu = (SaveAndLoadAssetWithMenu)GetWindow(typeof(SaveAndLoadAssetWithMenu));
        contextMenu.Show();
    }

    Object wallPrefab;
    Object wallContainer;
    private void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        wallPrefab = EditorGUILayout.ObjectField(wallPrefab, typeof(MeshFilter), true);
        wallContainer = EditorGUILayout.ObjectField(wallContainer, typeof(Transform), true);

        if (GUILayout.Button("InstantinateRooms")) LoadMeshes_andInstantinateRooms((MeshFilter)wallPrefab,(Transform)wallContainer);
    }

    public static void DownloadMeshes_andSaveRooms(RoomCreator manager)
    {
        string path = NodeCreator.ReturnPathToJsonFile(manager.PathToAssetBundle, manager.LevelName, "");

        Transform[] childs = new Transform[manager.WallContainer.childCount];
        for (int i = 0; i < manager.WallContainer.childCount; i++)
        {
            childs[i] = manager.WallContainer.GetChild(i);

            string transforms = $"{{\"pos\":{JsonConvert.SerializeObject(childs[i].transform.position)},\"rot\":{JsonConvert.SerializeObject(childs[i].transform.eulerAngles)},\"scale\":{JsonConvert.SerializeObject(childs[i].transform.lossyScale)}}}";
            
            AssetDatabase.CreateAsset(childs[i].GetComponent<MeshFilter>().mesh, $"{manager.PathToAssetBundle}/{manager.LevelName}/{i}.asset");
            AssetDatabase.SaveAssets();
            NodeCreator.WriteJsonAtJsonsFolder(transforms, manager.PathToAssetBundle, manager.LevelName, $"{i}.txt");
        }
    }
    public static void LoadMeshes_andInstantinateRooms(MeshFilter pref,Transform container)
    {
        string transformsStr;
        DeserializeTransform transforms;
        MeshFilter instaniatedObj;
        Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/ThirdParty/BuildingCreationEditor/AssetBundle/Test/0.asset",typeof(Mesh));
        for (int i = 0; mesh != null; i++)
        {
            if (i != 0) mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/ThirdParty/BuildingCreationEditor/AssetBundle/Test/{i}.asset");
            if (mesh != null)
            {
                transformsStr = NodeCreator.ReadFile("Assets/ThirdParty/BuildingCreationEditor/AssetBundle", "Test", $"{i}.txt");
                transforms = JsonConvert.DeserializeObject<DeserializeTransform>(transformsStr);
                instaniatedObj = Instantiate(pref, transforms.P, Quaternion.Euler(transforms.R), container);
                instaniatedObj.transform.localScale = transforms.S;
                instaniatedObj.mesh = mesh;
                instaniatedObj.gameObject.name = i.ToString();
                instaniatedObj.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }            
        }
    }
}