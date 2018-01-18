using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace KFrameWork
{
#if UNITY_EDITOR
    public class EditorTools :BaseAttributeRegister {

        private static EditorTools mIns;

        private Dictionary<int,EditorAssetImportAttribute> importCaches =new Dictionary<int, EditorAssetImportAttribute>();

        public EditorTools()
        {
            mIns =this;
            if (!EditorApplication.isPlaying)
                this.Init();
        }

        public static EditorTools getInstance()
        {
            return mIns;
        }

        private void Init()
        {
            try
            {
                //属性检查编辑器类
                EditorAttRegister.Register(this);

                var asmes = AppDomain.CurrentDomain.GetAssemblies();
                for(int i =0; i < asmes.Length;++i)
                {
                    var data = asmes[i];
                    if(data.FullName.StartsWith("Assembly"))
                    {
                        this.Start(data);
                    }
                }

                this.End();

                ////其他插件
                SceneListCheck.Generate();
            }
            catch(Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }


        public static string GetRelativeAssetPath(string _fullPath)
        {
            _fullPath = GetRightFormatPath(_fullPath);
            int idx = _fullPath.IndexOf("Assets");
            string assetRelativePath = _fullPath.Substring(idx);
            return assetRelativePath;
        }

        public static string GetRightFormatPath(string _path)
        {
            return _path.Replace("\\", "/");
        }

        public static bool HasSelected()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        public static string ToSelectPath()
        {
            if (HasSelected())
            {
                return AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            }
            return null;
        }

        public static FileInfo[] GetAllFilesInSelectDir(string path,string pattern)
        {
            string fullpath = Path.GetFullPath(path);
            DirectoryInfo dir = null;
            if (File.Exists(fullpath))
            {
                dir = new DirectoryInfo(Path.GetDirectoryName(fullpath));
            }
            else
            {
                dir = new DirectoryInfo(fullpath);
            }
            

            return dir.GetFiles(pattern, SearchOption.AllDirectories);
        }

        public static bool MatchActionOrFunc(MethodInfo method,Type delegateType)
        {
            MethodInfo delegateSignature = delegateType.GetMethod("Invoke");

            bool parametersEqual = delegateSignature
                .GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters()
                    .Select(x => x.ParameterType));

            return delegateSignature.ReturnType == method.ReturnType &&
                parametersEqual;
        }

        public void PushImportAtt(AssetImportDefine df,EditorAssetImportAttribute att)
        {
            int value = (int)df;
            if(!this.importCaches.ContainsKey(value))
            {
                this.importCaches[value] =att;
            }
            else
            {
                LogMgr.LogError("Duplicate Insert");
            }

        }

        public static string GetUnityAssetPath(string path)
        {
            int index = path.IndexOf("Assets/");
            if (index != -1)
                path = path.Substring(index);

            index = path.IndexOf("Assets\\");
            if (index == -1)
                return path;
            return path.Substring(index);

        }

        public static GameObject CreatePrefab(string path, GameObject target)
        {
            string dirname = Path.GetDirectoryName(path);
            if (Directory.Exists(dirname) == false)
                Directory.CreateDirectory(dirname);

            if (File.Exists(path))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return PrefabUtility.ReplacePrefab(target, prefab, ReplacePrefabOptions.ReplaceNameBased | ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                return PrefabUtility.CreatePrefab(path,target , ReplacePrefabOptions.ReplaceNameBased | ReplacePrefabOptions.ConnectToPrefab);
            }
        }

        public static T NewScriptAsset<T>(string path) where T : ScriptableObject
        {
            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            else
            {
                return ScriptableObject.CreateInstance<T>();
            }
        }

        public static T LoadGUIDAsset<T>(string guid) where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void CreateAsset<T>(string path,T o) where T:UnityEngine.Object
        {
            if (File.Exists(path))
            {
                EditorUtility.SetDirty(o);
            }
            else
            {
                AssetDatabase.CreateAsset(o, path);
            }
        }

        public void DynamicInvokeAtt(AssetImportDefine df,params object[] objs)
        {
            try
            {
                int value = (int)df;
                if(this.importCaches.ContainsKey(value))
                {
                    EditorAssetImportAttribute att = this.importCaches[value];

                    att.callback.DynamicInvoke(objs);
                }
            }catch(Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }

        public static void DirectoryCopy(string sourceDirName, string destDirName,string fileApend,string ignorename, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (temppath.EndsWith(ignorename))
                {
                    continue;
                }
                file.CopyTo(temppath+ fileApend, true);
            }
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    DirectoryCopy(subdir.FullName, temppath, fileApend, ignorename, copySubDirs);
                }
            }
        }

        public static void DirectoryDelete(string sourceDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                DirectoryDelete(subdir.FullName);
            }
        }

        public static void DirectoryDeleteFull(string sourceDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                subdir.Delete(true);
            }

        }

        public static void DirectoryCopyEndWith(string sourceDirName, string destDirName,string endwith, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!string.IsNullOrEmpty(endwith) && temppath.EndsWith(endwith, StringComparison.Ordinal))
                {
                    file.CopyTo(temppath, true);
                }
                else if (string.IsNullOrEmpty(endwith))
                {
                    file.CopyTo(temppath, true);
                }
            }

            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    DirectoryCopyEndWith(subdir.FullName, temppath, endwith, copySubDirs);
                }
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    DirectoryCopy(subdir.FullName, temppath,  copySubDirs);
                }
            }
        }

    }
#endif
}


