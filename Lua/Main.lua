
package.cpath = package.cpath .. ';C:/Users/Admin/AppData/Roaming/JetBrains/Rider2021.1/plugins/EmmyLua/classes/debugger/emmy/windows/x64/?.dll'
local dbg = require('emmy_core')
dbg.tcpListen('localhost', 9966)
dbg.waitIDE()
local started=false
function LuaStart() 
    print("start")
    local tree=require("TestBuildTree")
    BehTree.BehaviorTreeManager.RunTree(tree)
    started=true
end



function LuaUpdate( )
    if started then
        BehTree.BehaviorTreeManager.OnUpdate()
    end
end

function LuaOnDestroy()
   print("ondestroy")   
end

