#[Unity]AssetBundle资源更新以及多线程下载


## 前言
![这里写图片描述](https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1494902993&di=76ef0051c42c8c9cdfcebf3cad9604e0&imgtype=jpg&er=1&src=http://u.candou.com/2014/1010/1412905945671.jpg)
此文章适合不太了解资源加载的萌新，有了入门基础之后再去github上搜大牛写的专业的资源加载方案才能得心应手，不然的话会看的很吃力或者说一脸懵逼。Unity里面关于资源加载我们都知道是下载更新AssetBundle，关于[AssetBundle](http://blog.csdn.net/dingxiaowei2013/article/details/71479045)我之前的文章已经详细介绍过，没看过的朋友可以在看一下。下面介绍的资源加载的Demo有以下几点：
1.WWW下载图片资源
2.HTTP下载apk文件，并且支持断点续传，并且显示加载进度条
3.HTTP多线程下载文件


## 部分核心代码和讲解

### WWW下载
#### 思路：
WWW是Unity给我们封装的一个基于HTTP的简单类库，如果我们做很简单的下载，或者网络请求可以用这个类库，个人觉得这个封装的并不是很好，所以一般商业项目开发都不会使用这个，宁可自己去封装一个HTTP请求和下载的类库，可控性更好。仅仅是个人观点，不喜勿喷。
#### 代码:

```
using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class WWWLoad
{
    private WWW www = null;
    static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    /// <summary>
    /// 下载文件
    /// </summary>
    public IEnumerator DownFile(string url, string savePath, Action<WWW> process)
    {
        FileInfo file = new FileInfo(savePath);
        stopWatch.Start();
        UnityEngine.Debug.Log("Start:" + Time.realtimeSinceStartup);
        www = new WWW(url);
        while (!www.isDone)
        {
            yield return 0;
            if (process != null)
                process(www);
        }
        yield return www;
        if (www.isDone)
        {
            byte[] bytes = www.bytes;
            CreatFile(savePath, bytes);
        }
    }

    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="bytes"></param>
    public void CreatFile(string savePath, byte[] bytes)
    {
        FileStream fs = new FileStream(savePath, FileMode.Append);
        BinaryWriter bw = new BinaryWriter(fs);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();     //流会缓冲，此行代码指示流不要缓冲数据，立即写入到文件。
        fs.Close();     //关闭流并释放所有资源，同时将缓冲区的没有写入的数据，写入然后再关闭。
        fs.Dispose();   //释放流
        www.Dispose();

        stopWatch.Stop();
        Debug.Log("下载完成,耗时:" + stopWatch.ElapsedMilliseconds);
        UnityEngine.Debug.Log("End:" + Time.realtimeSinceStartup);
    }

}

```

### HTTP下载并加载AB资源
#### 思路：
主要用的核心类是HttpWebRequest，用这个类创建的对象可以申请下载的文件的大小以及下载的进度。移动上可读写的目录是PersidentDataPath，并且各个移动设备的路径不同，这点要注意，所以我们下载的AB资源就会下载到这个目录。

#### 效果图：
![这里写图片描述](http://img.blog.csdn.net/20170903085252084?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGluZ3hpYW93ZWkyMDEz/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)

#### 核心代码：

```
using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO;
using System.Net;
using System;

/// <summary>
/// 通过http下载资源
/// </summary>
public class HttpDownLoad {
	//下载进度
	public float progress{get; private set;}
	//涉及子线程要注意,Unity关闭的时候子线程不会关闭，所以要有一个标识
	private bool isStop;
	//子线程负责下载，否则会阻塞主线程，Unity界面会卡主
	private Thread thread;
	//表示下载是否完成
	public bool isDone{get; private set;}
    const int ReadWriteTimeOut = 2 * 1000;//超时等待时间
    const int TimeOutWait = 5 * 1000;//超时等待时间


    /// <summary>
    /// 下载方法(断点续传)
    /// </summary>
    /// <param name="url">URL下载地址</param>
    /// <param name="savePath">Save path保存路径</param>
    /// <param name="callBack">Call back回调函数</param>
    public void DownLoad(string url, string savePath,string fileName, Action callBack, System.Threading.ThreadPriority threadPriority = System.Threading.ThreadPriority.Normal)
	{
		isStop = false;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //开启子线程下载,使用匿名方法
        thread = new Thread(delegate() {
            stopWatch.Start();
            //判断保存路径是否存在
            if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}
			//这是要下载的文件名，比如从服务器下载a.zip到D盘，保存的文件名是test
			string filePath = savePath + "/"+ fileName;
			
			//使用流操作文件
			FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
			//获取文件现在的长度
			long fileLength = fs.Length;
			//获取下载文件的总长度
			UnityEngine.Debug.Log(url+" "+fileName);
			long totalLength = GetLength(url);
            Debug.LogFormat("<color=red>文件:{0} 已下载{1}M，剩余{2}M</color>",fileName,fileLength/1024/1024,(totalLength- fileLength)/ 1024/1024);			
			
			//如果没下载完
			if(fileLength < totalLength)
			{
				
				//断点续传核心，设置本地文件流的起始位置
				fs.Seek(fileLength, SeekOrigin.Begin);

				HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                
                request.ReadWriteTimeout = ReadWriteTimeOut;
                request.Timeout = TimeOutWait;

                //断点续传核心，设置远程访问文件流的起始位置
                request.AddRange((int)fileLength);

                Stream  stream = request.GetResponse().GetResponseStream();
				byte[] buffer = new byte[1024];
				//使用流读取内容到buffer中
				//注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
				int length = stream.Read(buffer, 0, buffer.Length);
                //Debug.LogFormat("<color=red>length:{0}</color>" + length);
                while (length > 0)
				{
					//如果Unity客户端关闭，停止下载
					if(isStop) break;
					//将内容再写入本地文件中
					fs.Write(buffer, 0, length);
					//计算进度
					fileLength += length;
					progress = (float)fileLength / (float)totalLength;
					//UnityEngine.Debug.Log(progress);
					//类似尾递归
					length = stream.Read(buffer, 0, buffer.Length);

				}
				stream.Close();
				stream.Dispose();

            }
            else
			{
				progress = 1;
            }
            stopWatch.Stop();
            Debug.Log("耗时: " + stopWatch.ElapsedMilliseconds);
            fs.Close();
			fs.Dispose();
			//如果下载完毕，执行回调
			if(progress == 1)
			{
                isDone = true;
                if (callBack != null) callBack();
                thread.Abort();
            }
            UnityEngine.Debug.Log ("download finished");	
		});
		//开启子线程
		thread.IsBackground = true;
        thread.Priority = threadPriority;
		thread.Start();
    }


	/// <summary>
	/// 获取下载文件的大小
	/// </summary>
	/// <returns>The length.</returns>
	/// <param name="url">URL.</param>
	long GetLength(string url)
	{
		UnityEngine.Debug.Log(url);
		
		HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
		requet.Method = "HEAD";
		HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
		return response.ContentLength;
	}

	public void Close()
	{
		isStop = true;
	}

}

```

### 多线程下载文件
#### 思路：
多线程下载思路是计算一个文件包大小，然后创建几个线程，计算每一个线程下载的始末下载的位置，最后是合并成一个整体的文件包写入到本地。
#### 效果图：
![这里写图片描述](http://img.blog.csdn.net/20170903084653808?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGluZ3hpYW93ZWkyMDEz/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)
#### 核心代码：
```
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class MultiHttpDownLoad : MonoBehaviour
{
	string savePath = string.Empty;

	string resourceURL = @"http://www.dingxiaowei.cn/birdlogo.png";
	string saveFile = string.Empty;
	public int ThreadNum { get; set; }
	public bool[] ThreadStatus { get; set; }
	public string[] FileNames { get; set; }
	public int[] StartPos { get; set; }
	public int[] FileSize { get; set; }
	public string Url { get; set; }
	public bool IsMerge { get; set; }

	DateTime beginTime;

	void Start()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		savePath = Application.streamingAssetsPath;
#elif UNITY_ANDROID
          savePath = Application.persistentDataPath;;
#endif
		saveFile = Path.Combine(savePath, "birdlogo.png");

		DownDoad();
	}

	void Init(long fileSize)
	{
		if (ThreadNum == 0)
			ThreadNum = 5;

		ThreadStatus = new bool[ThreadNum];
		FileNames = new string[ThreadNum];
		StartPos = new int[ThreadNum];
		FileSize = new int[ThreadNum];
		int fileThread = (int)fileSize / ThreadNum;
		int fileThreade = fileThread + (int)fileSize % ThreadNum;
		for (int i = 0; i < ThreadNum; i++)
		{
			ThreadStatus[i] = false;
			FileNames[i] = i.ToString() + ".dat";
			if (i < ThreadNum - 1)
			{
				StartPos[i] = fileThread * i;
				FileSize[i] = fileThread - 1;
			}
			else
			{
				StartPos[i] = fileThread * i;
				FileSize[i] = fileThreade - 1;
			}
		}
	}

	void DownDoad()
	{
		UnityEngine.Debug.Log("开始下载 时间:" + System.DateTime.Now.ToString());
		beginTime = System.DateTime.Now;
		Url = resourceURL;
		long fileSizeAll = 0;
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
		fileSizeAll = request.GetResponse().ContentLength;
		request.Abort();
		Init(fileSizeAll);

		System.Threading.Thread[] threads = new System.Threading.Thread[ThreadNum];
		HttpMultiThreadDownload[] httpDownloads = new HttpMultiThreadDownload[ThreadNum];
		for (int i = 0; i < ThreadNum; i++)
		{
			httpDownloads[i] = new HttpMultiThreadDownload(this, i);
			threads[i] = new System.Threading.Thread(new System.Threading.ThreadStart(httpDownloads[i].Receive));
			threads[i].Start();
		}
		StartCoroutine(MergeFile());
	}

	IEnumerator MergeFile()
	{
		while (true)
		{
			IsMerge = true;
			for (int i = 0; i < ThreadNum; i++)
			{
				if (ThreadStatus[i] == false)
				{
					IsMerge = false;
					System.Threading.Thread.Sleep(100);
					break;
				}
			}
			if (IsMerge)
				break;
		}

		int bufferSize = 512;
		int readSize;
		string downFileNamePath = saveFile;
		byte[] bytes = new byte[bufferSize];
		FileStream fs = new FileStream(downFileNamePath, FileMode.Create);
		FileStream fsTemp = null;

		for (int i = 0; i < ThreadNum; i++)
		{
			fsTemp = new FileStream(FileNames[i], FileMode.Open);
			while (true)
			{
				readSize = fsTemp.Read(bytes, 0, bufferSize);
				if (readSize > 0)
					fs.Write(bytes, 0, readSize);
				else
					break;
			}
			fsTemp.Close();
		}
		fs.Close();
		Debug.Log("接受完毕!!!结束时间:" + System.DateTime.Now.ToString());
		Debug.LogError("下载耗时:" + (System.DateTime.Now - beginTime).TotalSeconds.ToString());
		yield return null;
	}
}

public class HttpMultiThreadDownload
{
	const int bufferSize = 512;
	private int threadId;
	private string url;
	MultiHttpDownLoad downLoadObj;

	public HttpMultiThreadDownload(MultiHttpDownLoad downLoadObj, int threadId)
	{
		this.threadId = threadId;
		this.url = downLoadObj.Url;
		this.downLoadObj = downLoadObj;
	}

	public void Receive()
	{
		string fileName = downLoadObj.FileNames[threadId];
		var buffer = new byte[bufferSize];
		int readSize = 0;
		FileStream fs = new FileStream(fileName, System.IO.FileMode.Create);
		Stream ns = null;

		try
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AddRange(downLoadObj.StartPos[threadId], downLoadObj.StartPos[threadId] + downLoadObj.FileSize[threadId]);
			ns = request.GetResponse().GetResponseStream();
			readSize = ns.Read(buffer, 0, bufferSize);
			showLog("线程[" + threadId.ToString() + "] 正在接收 " + readSize);
			while (readSize > 0)
			{
				fs.Write(buffer, 0, readSize);
				readSize = ns.Read(buffer, 0, bufferSize);
				showLog("线程[" + threadId.ToString() + "] 正在接收 " + readSize);
			}
			fs.Close();
			ns.Close();
		}
		catch (Exception er)
		{
			Debug.LogError(er.Message);
			fs.Close();
		}
		showLog("线程[" + threadId.ToString() + "] 结束!");
		downLoadObj.ThreadStatus[threadId] = true;
	}

	private void showLog(string processing)
	{
		Debug.Log(processing);
	}
}

```


线程下载速度跟线程的关系呈钟罩式关系，也就是说适量的线程数量会提高下载速度，但并不是说线程数越多就越好，因为线程的切换和资源的整合也是需要时间的。下面就列举下载单个文件，创建的线程数和对应的下载时间：

 - 单线程
![这里写图片描述](http://img.blog.csdn.net/20170903082911107?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGluZ3hpYW93ZWkyMDEz/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)
 - 5个线程
![这里写图片描述](http://img.blog.csdn.net/20170903082922718?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGluZ3hpYW93ZWkyMDEz/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)
 - 15个线程
![这里写图片描述](http://img.blog.csdn.net/20170903082930979?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGluZ3hpYW93ZWkyMDEz/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)

这里我是1M的带宽，下载的是一个300KB左右的资源，一般不会做多线程下载单一资源，多线程下载一般用于下载多个资源，除非单一资源真的很大才有多线程下载，然后做合包操作。

## Demo下载
https://git.oschina.net/dingxiaowei/UnityResourceDownLoad.git
关注后续更新请点start或者fork，感谢！

## 开发交流
1群
![QQ群](http://img.blog.csdn.net/20161128123546291)
<a target="_blank" href="//shang.qq.com/wpa/qunwpa?idkey=e3e3e79643dedbe3ad8b25c448338f0be9fa23526a28a6f8a9f2389081e1eda0"><img border="0" src="//pub.idqqimg.com/wpa/images/group.png" alt="unity3d unity 游戏开发" title="unity3d unity 游戏开发"></a>

1群如果已经满员，请加2群
159875734

## 后续计划
写一个实际商业项目中用到的资源更新案例。