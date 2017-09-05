using UnityEngine;
using System.Collections;
using UnityEditor;

public class AssetBundleCreate : MonoBehaviour {

	[MenuItem("MS/AssetBundleCreate")]
	public static void Build()
	{
		//将资源打包到StreamingAssets文件夹下
		BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,BuildAssetBundleOptions.CollectDependencies,BuildTarget.Android);
	}

    [@MenuItem("MS/Build AssetBundles")]
    static void BuildABs()
    {
        // Create the array of bundle build details.
        AssetBundleBuild[] buildMap = new AssetBundleBuild[2];

        buildMap[0].assetBundleName = "resource";//打包的资源包名称 随便命名
        string[] resourcesAssets = new string[2];//此资源包下面有多少文件
        resourcesAssets[0] = Application.dataPath+"/Scenes/testScene";
        buildMap[0].assetNames = resourcesAssets;

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,buildMap,BuildAssetBundleOptions.CollectDependencies,BuildTarget.Android);
    }

}
