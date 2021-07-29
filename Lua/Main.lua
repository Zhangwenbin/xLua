
function LuaStart() 
    print("start")
    local tree=require("Test")
    BehTree.BehaviorTreeManager.RunTree(tree)
end



function LuaUpdate( )
    BehTree.BehaviorTreeManager.OnUpdate()
end

function LuaOnDestroy()
   print("ondestroy")   
end

