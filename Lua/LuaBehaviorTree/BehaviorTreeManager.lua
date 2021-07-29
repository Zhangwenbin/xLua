BehTree={}
require 'Enum'
require 'StackList'
require 'TaskRoot'
require 'ITask'
require 'IParent'
require 'IAction'
require 'IComposite'
require 'IConditional'
require 'IDecorator'
--复合节点（）
require 'Selector'
require 'Sequence'
--修饰节点
require 'Repeater'
require 'ReturnFailure'
require 'ReturnSuccess'
require 'UntilFailure'
require 'Inverter'
--Action节点
require 'Wait'


BehTree.BehaviorTreeManager={}
local this = BehTree.BehaviorTreeManager
function this.Init()
end
--从这里开始启动一颗行为树的入口跟节点
function this.RunTree(enter)
	this.bhTree =enter
end

--重置树下所有Action
function this.ResetTreeActions()
	local treeRoot = this.GetCurTreeRoot()
	treeRoot:ResetAllActionsState()
end

function this.OnUpdate() 
	this.UpdateTask()
end
function this.UpdateTask()
	local status = this.bhTree:OnUpdate()
	print(status)
	if status ~= BehTree.TaskStatus.Running then
		
	end
end