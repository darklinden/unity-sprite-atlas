using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class LoadUtil
{
    const int MAX_ATTEMPTS = 3;

    private static readonly Lazy<LoadUtil> lazy = new Lazy<LoadUtil>(() => new LoadUtil());
    public static LoadUtil Instances => lazy.Value;


    private Dictionary<string, int> FailedPaths = new Dictionary<string, int>();
    private Dictionary<string, UnityEngine.Object> LoadedPrototypes = new Dictionary<string, UnityEngine.Object>(128);
    private UnityEngine.Object getPrototype(string path)
    {
        if (LoadedPrototypes.TryGetValue(path, out UnityEngine.Object obj))
        {
            return obj;
        }
        if (FailedPaths.TryGetValue(path, out var failedCount))
        {
            if (failedCount > MAX_ATTEMPTS)
            {
                return null;
            }
        }
        return obj;
    }

    public static T GetPrototype<T>(string path) where T : UnityEngine.Object
    {
        return (T)Instances.getPrototype(path);
    }

    private Dictionary<string, List<string>> LoadedPathsByLabel = new Dictionary<string, List<string>>(64);
    private List<string> getPathsByLabel(string label)
    {
        if (LoadedPathsByLabel.TryGetValue(label, out List<string> paths))
        {
            return paths;
        }
        return null;
    }

    public static List<string> GetPathsByLabel(string label)
    {
        return Instances.getPathsByLabel(label);
    }

    private IEnumerator loadPathsByLabel(string label)
    {
        // if cached already, return
        if (LoadedPathsByLabel.ContainsKey(label)) yield break;
        // if failed too many times, return
        if (FailedPaths.TryGetValue(label, out var failedCount))
        {
            if (failedCount > MAX_ATTEMPTS)
            {
                yield break;
            }
        }

        var handle = Addressables.LoadResourceLocationsAsync(label);
        yield return handle;

        var result = new List<string>();
        if (handle.Result != null && handle.Result.Count > 0)
        {
            foreach (var loc in handle.Result)
                result.Add(loc.PrimaryKey);
        }
        LoadedPathsByLabel[label] = result;
        Addressables.Release(handle);
    }

    public static IEnumerator LoadPathsByLabel(string label)
    {
        return Instances.loadPathsByLabel(label);
    }

    private IEnumerator load<T>(string path)
    {
        // if cached already, return
        if (LoadedPathsByLabel.ContainsKey(path)) yield break;
        // if failed too many times, return
        if (FailedPaths.TryGetValue(path, out var failedCount))
        {
            if (failedCount > MAX_ATTEMPTS)
            {
#if UNITY_EDITOR && DEBUG
                Log.E("LoadUtil.load too many times", path);
#else
                yield break;
#endif
            }
        }

        var handle = Addressables.LoadAssetAsync<T>(path);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            LoadedPrototypes.Add(path, handle.Result as UnityEngine.Object);
        }
        else
        {
            int count = 0;
            if (!FailedPaths.TryGetValue(path, out count))
            {
                count = 0;
            }
            count++;
            FailedPaths[path] = count;
            Log.E("LoadUtil.load Failed", path, count);
        }
    }

    public static IEnumerator Load<T>(string path)
    {
        return Instances.load<T>(path);
    }

    private void release(string pathOrLabel)
    {
        if (LoadedPathsByLabel.TryGetValue(pathOrLabel, out List<string> paths))
        {
            // release all paths by label
            foreach (var path in paths)
            {
                if (LoadedPrototypes.TryGetValue(path, out UnityEngine.Object obj))
                {
                    if (obj != null) Addressables.Release(obj);
                    LoadedPrototypes.Remove(path);
                }
            }
            LoadedPathsByLabel.Remove(pathOrLabel);
        }
        else
        {
            // release single path
            if (LoadedPrototypes.TryGetValue(pathOrLabel, out UnityEngine.Object obj))
            {
                if (obj != null) Addressables.Release(obj);
                LoadedPrototypes.Remove(pathOrLabel);
            }
        }
    }
    public static void Release(string pathOrLabel)
    {
        Instances.release(pathOrLabel);
    }

    private IEnumerator loadScene(string path)
    {
        var handle = Addressables.LoadSceneAsync(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
        yield return handle;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Log.E("LoadUtil.loadScene Failed ", path);
        }
    }

    public static IEnumerator LoadScene(string path)
    {
        return Instances.loadScene(path);
    }
}