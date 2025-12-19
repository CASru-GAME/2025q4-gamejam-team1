using UnityEngine;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private TaskTree taskTree;
    public TaskTree TaskTree => taskTree;
    private List<TaskNode> activeTasks = new List<TaskNode>();
    private List<TaskNode> completedTasks = new List<TaskNode>();
    private List<TaskNode> deliveredTasks = new List<TaskNode>();
    private List<TaskNode> availableTasks = new List<TaskNode>();

    public void Awake()
    {
        ResetAllTasks();
    }

    private void CompleteTask(TaskNode node)
    {
        if (node.IsCompleted) return;
        node.Complete();
        if (!completedTasks.Contains(node))
        {
            completedTasks.Add(node);
        }
        activeTasks.Remove(node);
        availableTasks = GetAvailableTasks();
    }

    private void ActivateTask(TaskNode node)
    {
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

    private void DeliverTask(TaskNode node)
    {
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
}
