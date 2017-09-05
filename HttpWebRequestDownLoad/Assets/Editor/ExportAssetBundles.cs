using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ExportAssetBundles
{
    private static List<Object> prefabList = new List<Object>();
    //在Unity编辑器中添加菜单
    [MenuItem("Assets/Build AssetBundle From Selection")]
    static void ExportResourceRGB2()
    {
        // 打开保存面板，获得用户选择的路径
        string path = EditorUtility.SaveFilePanel(Application.streamingAssetsPath, "", "test", "assetbundle");

        if (path.Length != 0)
        {
            // 选择的要保存的对象
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            //打包
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows);
        }
    }

    [MenuItem("AssetBundle/Build Scene(请选择工程下AssetBundle文件夹)")]
    static void ExportScene()
    {
          // 打开保存面板，获得用户选择的路径
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "test", "");

        if (path.Length != 0)
        {
            // 选择的要保存的对象
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            string[] scenes = { "Assets/Scenes/testScene.unity" };
            //打包
            BuildPipeline.BuildPlayer(scenes,path,BuildTarget.StandaloneWindows,BuildOptions.BuildAdditionalStreamedScenes);
        }
    }

    /// <summary>
    /// 获取resources目录下的Prefabs
    /// </summary>
    [@MenuItem("AssetBundle/Resource/BuildAssetBundle")]
    static void Test_Compoent()
    {
        //路径  
        string fullPath = "Assets/Resurces/";
        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            Debug.Log(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".prefab"))
                {
                    prefabList.Add((GameObject)AssetDatabase.LoadAssetAtPath(fullPath + files[i].Name, typeof(UnityEngine.Object)));
                    Debug.Log("path:" + fullPath + files[i].Name);
                }
                //Debug.Log( "FullName:" + files[i].FullName );  
                //Debug.Log( "DirectoryName:" + files[i].DirectoryName );  
            }
            // 打开保存面板，获得用户选择的路径
            string path = EditorUtility.SaveFilePanel("Save Resource", "", "test1", "assetbundle");

            if (path.Length != 0)
            {
                BuildPipeline.BuildAssetBundle(null, prefabList.ToArray(), path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows);
            }
            AssetDatabase.Refresh();
        }
    }
}