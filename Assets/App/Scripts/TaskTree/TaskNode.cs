using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskNode", menuName = "Task/TaskNode")]
public class TaskNode : ScriptableObject
{
    [SerializeField][Header("自動割当のため書込み禁止")][Tooltip("タスクの一意識別子")] private int id;
    [SerializeField][Header("以降書込み可")][Tooltip("タスクの種類")] private TaskType[] taskType;
    [SerializeField][Tooltip("タスクグループID")] private int taskGroupID;
    [SerializeField][Tooltip("必要な親タスクの完了条件")] private NeededCompletedParentTasks neededCompletedParentTasks;
    [SerializeField][Tooltip("タスク名")] private string taskName;
    [SerializeField][TextArea(3, 10)][Tooltip("タスクの説明")] private string description;
    [SerializeField][Tooltip("代替子タスクを持つかどうか")] private bool isHavingAlternativeChildren;
    [SerializeField][Tooltip("代替子タスクのリスト")] private List<TaskNode> alternativeChildTasks;
    [SerializeField][Tooltip("子タスクのリスト")] private TaskNode[] childTasks;
    [SerializeField][Tooltip("親タスクのリスト")] private TaskNode[] parentTasks;
    [SerializeField][Tooltip("必要なアイテムのリスト")] private List<Detail> requiredItems;
    [SerializeField][Tooltip("対象の敵のリスト")] private List<Detail> targetEnemies;
    [SerializeField][Tooltip("報酬アイテムのリスト")] private List<Detail> rewardItems;
    public int ID => id;
    public TaskType[] TType => taskType;
    public int TaskGroupID => taskGroupID;
    public NeededCompletedParentTasks NeededCompletedParents => neededCompletedParentTasks;
    public string TaskName => taskName;
    public string Description => description;
    public bool IsHavingAlternativeChildren => isHavingAlternativeChildren;
    public List<TaskNode> AlternativeChildTasks => alternativeChildTasks;
    public TaskNode[] ChildTasks => childTasks;
    public TaskNode[] ParentTasks => parentTasks;
    public List<Detail> RequiredItems => requiredItems;
    public List<Detail> TargetEnemies => targetEnemies;
    public List<Detail> RewardItems => rewardItems;
    [Header("状態管理・デバッグ用")]
    [SerializeField] private bool isCompleted = false;
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isDelivered = false;
    public bool IsCompleted => isCompleted;
    public bool IsActive => isActive;
    public bool IsDelivered => isDelivered;

    public void Complete(int killCount = 0, int collectCount = 0)
    {
        if (!CheckCompletable(killCount, collectCount))
        {
            Debug.LogWarning($"タスク '{taskName}' を完了できません。");
            return;
        }
        isCompleted = true;
    }
    public void Activate()
    {
        isActive = true;
    }
    public void Deliver()
    {
        isDelivered = true;
    }
    public void ResetStatus()
    {
        isActive = false;
        isCompleted = false;
        isDelivered = false;
    }

    public bool CheckCompletable(int killCount = 0, int collectCount = 0)
    {
        if (!isActive)
        {
            Debug.LogWarning($"タスク '{taskName}' はアクティブではないため、完了できません。");
            return false;
        }
        if (taskType != null)
        {
            foreach (var t in taskType)
            {
                if (t == TaskType.Collect)
                {
                    foreach (var item in requiredItems)
                    {
                        if ((collectCount < item.count) && (item.count > 0))
                        {
                            Debug.LogWarning($"タスク '{taskName}' の収集アイテムが不足しています。必要数: {item.count}, 現在の数: {collectCount}");
                            return false;
                        }
                    }
                }
                else if (t == TaskType.Hunt)
                {
                    foreach (var enemy in targetEnemies)
                    {
                        if ((killCount < enemy.count) && (enemy.count > 0))
                        {
                            Debug.LogWarning($"タスク '{taskName}' の対象敵の討伐数が不足しています。必要数: {enemy.count}, 現在の数: {killCount}");
                            return false;
                        }
                    }
                }
                else if (t == TaskType.Deliver)
                {
                    if (!isDelivered)
                    {
                        Debug.LogWarning($"タスク '{taskName}' はまだ配達されていません。");
                        return false;
                    }
                }
            }
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // idが0か、既存のすべてのTaskNodeで重複している場合は新しいIDを生成
        if (id == 0 || IsDuplicateID())
        {
            id = GenerateUniqueID();
        }

        // タスク名を基にScriptableObjectの名前を変更
        if (!string.IsNullOrEmpty(taskName))
        {
            name = taskName; // ScriptableObjectの名前をタスク名に設定
        }
    }

    private bool IsDuplicateID()
    {
        var allTaskNodes = UnityEditor.AssetDatabase.FindAssets("t:TaskNode");
        int duplicateCount = 0;

        foreach (var guid in allTaskNodes)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TaskNode node = UnityEditor.AssetDatabase.LoadAssetAtPath<TaskNode>(path);
            if (node != null && node.id == this.id)
            {
                duplicateCount++;
            }
        }

        // 自分自身を含めて2つ以上存在する場合は重複
        return duplicateCount > 1;
    }

    private int GenerateUniqueID()
    {
        var allTaskNodes = UnityEditor.AssetDatabase.FindAssets("t:TaskNode");
        HashSet<int> usedIDs = new HashSet<int>();

        foreach (var guid in allTaskNodes)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TaskNode node = UnityEditor.AssetDatabase.LoadAssetAtPath<TaskNode>(path);
            if (node != null && node != this && node.id != 0)
            {
                usedIDs.Add(node.id);
            }
        }

        int newID = 1;
        while (usedIDs.Contains(newID))
        {
            newID++;
        }

        return newID;
    }
#endif

    [System.Serializable]
    public struct Detail
    {
        public int id;
        public int count;
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
