using UnityEngine;

[CreateAssetMenu(fileName = "TaskNode", menuName = "Task/TaskNode")]
public class TaskNode : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private TaskType taskType;
    [SerializeField] private string taskName;
    [SerializeField] private string description;
    [SerializeField] private TaskNode[] childTasks;
    [SerializeField] private TaskNode[] parentTasks;
    [SerializeField] public bool isCompleted;
    [SerializeField] public bool isActive;
    [SerializeField] public bool isDelivered;
    [SerializeField] private List<Detail> requiredItems;
    [SerializeField] private List<Detail> targetEnemies;
    [SerializeField] private List<Detail> rewardItems;
    public int ID => id;
    public TaskType TType => taskType;
    public string TaskName => taskName;
    public string Description => description;
    public TaskNode[] ChildTasks => childTasks;
    public TaskNode[] ParentTasks => parentTasks;
    public List<Detail> RequiredItems => requiredItems;
    public List<Detail> TargetEnemies => targetEnemies;
    public List<Detail> RewardItems => rewardItems;

    [System.Serializable]
    private struct Detail
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
}
