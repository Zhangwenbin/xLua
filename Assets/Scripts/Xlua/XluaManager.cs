using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System.Text;

namespace EG
{
    public class XluaManager
    {
        private LuaEnv luaenv = null;
        public LuaEnv LuaEnv{ get { return luaenv; } }
        public static XluaManager Instance = new XluaManager();
        public  string LuaPath = Application.dataPath.Replace("Assets", "Lua");
        // Use this for initialization
        public void Start()
            {
            BuildLuaFileMap();
            luaenv = new LuaEnv();
            luaenv.AddLoader((ref string filename) =>
            {
                if (filename.Contains("emmy_core"))
                {
                    return null;
                }
                if (mLuaFileMap.ContainsKey(filename))
                {
                    string fullPath = mLuaFileMap[filename];
                    return File.ReadAllBytes(fullPath);
                }
                var file = Path.Combine(LuaPath, filename);
                return File.ReadAllBytes(file);
            });
        }

        // Update is called once per frame
        public void Update()
            {
                if (luaenv != null)
                {
                    luaenv.Tick();
                }
            }

        public void OnDestroy()
            {
                luaenv.Dispose();
            }

        public void DoString(string luaStr)
        {
            if (luaenv != null)
            {
                luaenv.DoString(luaStr);
            }
        }



#if UNITY_EDITOR || USE_EDITOR_LUA
        private Dictionary<string, string> mLuaFileMap = new Dictionary<string, string>();
        private void BuildLuaFileMap()
        {
            StringBuilder sb = new StringBuilder();
            WalkLuaFiles(LuaPath, delegate (string filename, string fullname) {
                //删除filename的.lua后缀
                filename = filename.Substring(0, filename.Length - ".lua".Length);
                fullname = fullname.Replace("\\", "/");
                if (mLuaFileMap.ContainsKey(filename))
                {
                    //已经记录过这个文件名了，说明文件名有重名，报出来
                    string orig_filepath = mLuaFileMap[filename];
                    sb.Clear();
                    sb.Append("lua文件名重名：\n");
                    sb.Append(orig_filepath);
                    sb.Append(fullname);
                    Debug.LogError(sb.ToString());
                }
                else
                {
                    mLuaFileMap[filename] = fullname;
                }
            });
        }

        private void WalkLuaFiles(string parent_dir, System.Action<string, string> callback)
        {
            DirectoryInfo dir = new DirectoryInfo(parent_dir);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo finfo in files)
            {
                if (finfo.Name.EndsWith(".lua"))
                {
                    //是lua文件
                    if (callback != null)
                    {
                        callback(finfo.Name, finfo.FullName);
                    }
                }
            }
            DirectoryInfo[] sub_dirs = dir.GetDirectories();
            foreach (DirectoryInfo sub_dir in sub_dirs)
            {
                WalkLuaFiles(sub_dir.FullName, callback);
            }

        }
#endif
    }
}