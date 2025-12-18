using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskNode", menuName = "Task/TaskNode")]
public class TaskNode : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private TaskType taskType;
    [SerializeField] private NeededCompletedParentTasks neededCompletedParentTasks;
    [SerializeField] private string taskName;
    [SerializeField] private string description;
    [SerializeField] private TaskNode[] childTasks;
    [SerializeField] private TaskNode[] parentTasks;
    [SerializeField] private List<Detail> requiredItems;
    [SerializeField] private List<Detail> targetEnemies;
    [SerializeField] private List<Detail> rewardItems;
    public int ID => id;
    public TaskType TType => taskType;
    public NeededCompletedParentTasks NeededCompletedParents => neededCompletedParentTasks;
    public string TaskName => taskName;
    public string Description => description;
    public TaskNode[] ChildTasks => childTasks;
    public TaskNode[] ParentTasks => parentTasks;
    public List<Detail> RequiredItems => requiredItems;
    public List<Detail> TargetEnemies => targetEnemies;
    public List<Detail> RewardItems => rewardItems;
    public bool isCompleted;
    public bool isActive;
    public bool isDelivered;
    
    [System.Serializable] 
    public struct Detail
    {
        public int id;
        public int number;
    }

    public enum TaskType
    {
        Collect,
        Hunt,
        Deliver
    }
    public enum NeededCompletedParentTasks
    {
        None,
        Any,
        All
    }
}
