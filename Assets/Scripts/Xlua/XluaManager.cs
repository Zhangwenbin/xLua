using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;

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
                luaenv = new LuaEnv();
            luaenv.AddLoader((ref string filename) =>
            {
                if (!filename.EndsWith(".lua"))
                {
                    filename = filename + ".lua";
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

    }
}