using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EG
{
    public class GameStart : MonoBehaviour
    {
        private Action LuaStart, LuaUpdate, LuaOnDestroy;
        // Start is called before the first frame update
        void Start()
        {
            XluaManager.Instance.Start();
            XluaManager.Instance.DoString("require 'Main'");
            LuaStart = XluaManager.Instance.LuaEnv.Global.Get<Action>("LuaStart");
            LuaUpdate = XluaManager.Instance.LuaEnv.Global.Get<Action>("LuaUpdate");
            LuaOnDestroy = XluaManager.Instance.LuaEnv.Global.Get<Action>("LuaOnDestroy");
            LuaStart();
        }

        // Update is called once per frame
        void Update()
        {
            LuaUpdate();
            XluaManager.Instance.Update();
        }

        private void OnDestroy()
        {
            LuaOnDestroy();
            XluaManager.Instance.OnDestroy();
        }
    }
}