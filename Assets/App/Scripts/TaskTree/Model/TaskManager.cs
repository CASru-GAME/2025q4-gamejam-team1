using UnityEngine;
using System.Collections.Generic;
using System;
using NUnit.Framework;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;
    [SerializeField] private TaskTree taskTree;
    public TaskTree TaskTree => taskTree;
    private List<TaskNode> activeTasks = new List<TaskNode>();
    private List<TaskNode> completedTasks = new List<TaskNode>();
    private List<TaskNode> deliveredTasks = new List<TaskNode>();
    private List<TaskNode> availableTasks = new List<TaskNode>();
    private List<ActivatedTaskProgressInfo> activatedTaskProgressInfos = new List<ActivatedTaskProgressInfo>();
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
        if (!IsCompletableTask(node, activatedTaskProgressInfos.Find(info => info.nodeId == nodeId)))
        {
            Debug.LogWarning($"タスク '{node.TaskName}' を完了できません。");
            return;
        }
        node.Complete();
        if (!completedTasks.Contains(node))
        {
            completedTasks.Add(node);
        }
        activeTasks.Remove(node);
        activatedTaskProgressInfos.RemoveAll(info => info.nodeId == nodeId);
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
            activatedTaskProgressInfos.Add(new ActivatedTaskProgressInfo(nodeId));
        }
    }

    public void DeliverTask(int nodeId)
    {
        var node = taskTree.GetNodeById(nodeId);
        if (IsDeliverableTask(nodeId) == false) return;
        node.Deliver();
        foreach (var item in node.RequiredItems)
        {
            Inventory.Instance.RemoveItem(item.id, item.count);
        }
        if (!deliveredTasks.Contains(node))
        {
            deliveredTasks.Add(node);
        }
    }
    public bool IsDeliverableTask(int nodeID)
    {
        var node = taskTree.GetNodeById(nodeID);
        if (node.TType == null || !Array.Exists(node.TType, t => t == TaskNode.TaskType.Deliver))
        {
            return false;
        }
        foreach (var item in node.RequiredItems)
        {
            if (Inventory.Instance.CanRemoveItem(item.id, item.count) == false)
            {
                return false;
            }
        }
        return true;
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

    private bool IsCompletableTask(TaskNode node, ActivatedTaskProgressInfo progressInfo)
    {
        if (node.TType != null)
        {
            foreach (var t in node.TType)
            {
                if (t == TaskNode.TaskType.Collect)
                {
                    foreach (var item in node.RequiredItems)
                    {
                        var collectedItem = progressInfo.collectedItems.Find(i => i.id == item.id);
                        if (collectedItem == null || collectedItem.count < item.count)
                        {
                            return false;
                        }
                    }
                }
                else if (t == TaskNode.TaskType.Hunt)
                {
                    foreach (var enemy in node.TargetEnemies)
                    {
                        var defeatedEnemy = progressInfo.defeatedEnemies.Find(e => e.id == enemy.id);
                        if (defeatedEnemy == null || defeatedEnemy.count < enemy.count)
                        {
                            return false;
                        }
                    }
                }
                else if (t == TaskNode.TaskType.Deliver)
                {
                    if (!node.IsDelivered)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
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

    private class ActivatedTaskProgressInfo
    {
        public int nodeId;
        public List<CountInfo> collectedItems = new List<CountInfo>();
        public List<CountInfo> defeatedEnemies = new List<CountInfo>();
        private void UpdateCollectedItem(int itemId, int count)
        {
            foreach (var item in collectedItems)
            {
                if (item.id == itemId)
                {
                    item.count += count;
                    return;
                }
            }
        }

        private void UpdateDefeatedEnemy(int enemyId, int count)
        {
            foreach (var enemy in defeatedEnemies)
            {
                if (enemy.id == enemyId)
                {
                    enemy.count += count;
                    return;
                }
            }
        }

        private void InitializeCounts()
        {
            foreach (var requiredItem in instance.TaskTree.GetNodeById(nodeId).RequiredItems)
            {
                collectedItems.Add(new CountInfo { id = requiredItem.id, count = 0 });
            }
            foreach (var targetEnemy in instance.TaskTree.GetNodeById(nodeId).TargetEnemies)
            {
                defeatedEnemies.Add(new CountInfo { id = targetEnemy.id, count = 0 });
            }
        }
        public ActivatedTaskProgressInfo(int nodeId)
        {
            this.nodeId = nodeId;
            InitializeCounts();
            PlayerStatistics.instance.SubscribeToItemCollected(UpdateCollectedItem);
            PlayerStatistics.instance.SubscribeToEnemyDefeated(UpdateDefeatedEnemy);
        }
    }

    private class CountInfo
    {
        public int id;
        public int count;
    }
}

