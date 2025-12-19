using UnityEngine;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;
    [SerializeField] private TaskTree taskTree;
    public TaskTree TaskTree => taskTree;
    private List<TaskNode> activeTasks = new List<TaskNode>();
    private List<TaskNode> completedTasks = new List<TaskNode>();
    private List<TaskNode> deliveredTasks = new List<TaskNode>();
    private List<TaskNode> availableTasks = new List<TaskNode>();
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        ResetAllTasks();
    }

    public void CompleteTask(int nodeId, int killCount = 0, int collectCount = 0)
    {
        var node = taskTree.GetNodeById(nodeId);
        if (node.IsCompleted) return;
        node.Complete();
        if (!completedTasks.Contains(node))
        {
            completedTasks.Add(node);
        }
        activeTasks.Remove(node);
        availableTasks = GetAvailableTasks();
    }

    public void ActivateTask(int nodeId)
    {
        var node = taskTree.GetNodeById(nodeId);
        if (availableTasks.Contains(node) == false)
        {
            Debug.LogWarning($"タスク '{node.TaskName}' は現在利用できません。");
            return;
        }
        if (node.IsActive) return;
        node.Activate();
        if (!activeTasks.Contains(node))
        {
            activeTasks.Add(node);
        }
    }

    public void DeliverTask(int nodeId)
    {
        var node = taskTree.GetNodeById(nodeId);
        if (node.IsDelivered) return;
        node.Deliver();
        if (!deliveredTasks.Contains(node))
        {
            deliveredTasks.Add(node);
        }
    }

    public List<TaskNode> GetAvailableTasks(int? groupId = null, bool includeAlreadyActive = false)
    {
        if (taskTree == null) return new List<TaskNode>();
        return taskTree.GetAvailableNodes(groupId, includeAlreadyActive);
    }

    public List<TaskNode> GetCompletableTasks(int killCount = 0, int collectCount = 0, int? groupId = null)
    {
        var list = new List<TaskNode>();
        foreach (var node in activeTasks)
        {
            if (node.CheckCompletable())
            {
                if (groupId == null || node.TaskGroupID == groupId)
                {
                    list.Add(node);
                }
            }
        }
        return list;
    }

    public void ActivateAllPossible(int? groupId = null)
    {
        var list = GetAvailableTasks(groupId);
        foreach (var node in list)
            node.Activate();
    }

    private void ResetAllTasks()
    {
        foreach (var node in taskTree.Nodes)
        {
            node.ResetStatus();
        }
        activeTasks.Clear();
        completedTasks.Clear();
        deliveredTasks.Clear();
        availableTasks = GetAvailableTasks();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private class CountMemory
    {
        private Dictionary<int, List<Dictionary<int, int>>> memory = new Dictionary<int, List<Dictionary<int, int>>>();
        private Dictionary<int, List<Dictionary<int, int>>> countAfterActivate = new Dictionary<int, List<Dictionary<int, int>>>();

        public int GetCountByNodeIdAndItemId(int nodeId, int itemId)
        {
            int count = 0;
            if (memory.ContainsKey(nodeId))
            {
                foreach (var dict in memory[nodeId])
                {
                    if (dict.ContainsKey(itemId))
                    {
                        return count += dict[itemId];
                    }
                }
            }
            return count;
        }

        public void CulculateCountByNodeIdAndItemId(int nodeId)
        {
            if (!memory.ContainsKey(nodeId))
            {
                memory[nodeId] = new List<Dictionary<int, int>>();
            }
            var temp = new Dictionary<int, int>();
            foreach (var item in PlayerStatistics.instance.GetCollectedItemsDictionary())
            {
                temp[item.Key] = item.Value;
            }

            memory[nodeId].Add(temp);
        }
    }
}
