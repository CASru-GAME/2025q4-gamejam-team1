using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class TaskTreeTab : MonoBehaviour
{
    private static TaskTreeTab instance;
    public static TaskTreeTab Instance => instance;
    [SerializeField] private GameObject taskTreeTabObject;
    [SerializeField] private GameObject taskTreeSelectButtonObject;
    [SerializeField] private TaskTreeUIGroup[] taskTreeUIGroupsPrefab;
    [SerializeField][Header("自動割当・書き込み禁止")] private List<InfoPanelFunctionWrapperPair> infoPanelFunctionWrappers = new List<InfoPanelFunctionWrapperPair>();
    private TaskTreeUIGroup[] taskTreeUIGroups;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        if (!IsGroupCountMatchingPrefabCount())
        {
            Debug.LogError("TaskTreeTab: タスクグループに対応するPrefabが不足しています。");
        }
        if (!AreGroupIDsMatchingPrefabs(out string details))
        {
            Debug.LogError($"TaskTreeTab: {details}");
        }
        GenerateTaskTreeUIGroups();
        GenerateTaskTreeSelectButtons();
        InitializeInfoPanel();
    }
    private void Update()
    {
        var keyboard = Keyboard.current;
        ToggleTaskTreeTab(keyboard);
    }

    private void ToggleTaskTreeTab(Keyboard keyboard)
    {
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            taskTreeTabObject.GetComponent<Canvas>().enabled = !taskTreeTabObject.GetComponent<Canvas>().enabled;
        }
    }

    private void GenerateTaskTreeSelectButtons()
    {
        int totalGroups = TaskManager.instance.TaskTree.GetTotalGroupsNumber();
        int[] groupIDs = TaskManager.instance.TaskTree.GetAllGroupIDs().ToArray();
        for (int i = 0; i < totalGroups; i++)
        {
            var buttonObject = Instantiate(taskTreeSelectButtonObject, taskTreeTabObject.transform);

            // 配置調整: 2個目以降はYを-50ずつ下げる
            var rect = buttonObject.GetComponent<RectTransform>();
            var offsetY = -50f * (i - 1); // 1個目=0, 2個目=-50, 3個目=-100...
            rect.anchoredPosition += new Vector2(0f, offsetY);

            int groupId = groupIDs[i];
            buttonObject.GetComponentInChildren<Text>().text = $"{groupId}";
            buttonObject.GetComponent<Button>().onClick.AddListener(() => SwitchTaskTreeGroup(groupId));
        }
    }

    private void GenerateTaskTreeUIGroups()
    {
        int totalGroups = TaskManager.instance.TaskTree.GetTotalGroupsNumber();
        taskTreeUIGroups = new TaskTreeUIGroup[totalGroups];
        int[] groupIDs = TaskManager.instance.TaskTree.GetAllGroupIDs().ToArray();
        for (int i = 0; i < totalGroups; i++)
        {
            // taskTreeUIGroupsPrefabから対応するgroupIDのPrefabを探してインスタンス化
            // taskTreeUIGroupsPrefabのidから探したい
            var targetTaskTreeUIGroupsPrefab = System.Array.Find(taskTreeUIGroupsPrefab, prefab => prefab.groupID == groupIDs[i]);
            if (targetTaskTreeUIGroupsPrefab.groupID == 0) // 修正: nullチェック
            {
                Debug.LogError($"TaskTreeUIGroup Prefab with groupID {groupIDs[i]} not found!");
                continue;
            }

            var groupObject = Instantiate(targetTaskTreeUIGroupsPrefab.groupObject, taskTreeTabObject.transform);
            var functionWrapper = groupObject.GetComponent<InfoPanelFunctionWrapper>();
            infoPanelFunctionWrappers.Add(new InfoPanelFunctionWrapperPair
            {
                groupID = groupIDs[i],
                functionWrapper = functionWrapper
            });
            taskTreeUIGroups[i] = new TaskTreeUIGroup
            {
                groupID = groupIDs[i],
                groupObject = groupObject
            };
            // 最初のグループだけ表示、他は非表示にする
            if (i == 0)
            {
                groupObject.GetComponent<Canvas>().enabled = true;
            }
            else
            {
                groupObject.GetComponent<Canvas>().enabled = false;
            }
        }
    }

    private void InitializeInfoPanel()
    {
        var tree = TaskManager.instance?.TaskTree;
        if (tree == null) return;

        foreach (var wrapperPair in infoPanelFunctionWrappers)
        {
            var wrapper = wrapperPair.functionWrapper;
            if (wrapper == null) continue;

            var pairs = wrapper.GetInfoPanelPairs();
            foreach (var pair in pairs)
            {
                var node = tree.GetNodeById(pair.taskID);
                if (node == null) continue;

                var status = GetStatusText(node);
                var isAlt = IsAlternativeChild(node) ? "代替" : "";
                var title = node.TaskName;
                var type = BuildTaskTypeText(node);
                var description = node.Description;
                var need = BuildNeedsText(node);
                var reward = BuildRewardsText(node);

                wrapper.SetInfoPanelTexts(pair.taskID, status, isAlt, title, type, description, need, reward);
                SetInfoPanelButtonFunctions(pair.taskID);
            }
        }
    }

    private void SetInfoPanelButtonFunctions(int taskID)
    {
        var tree = TaskManager.instance?.TaskTree;
        if (tree == null) return;

        var node = tree.GetNodeById(taskID);
        if (node == null) return;

        var activateActions = new List<UnityEngine.Events.UnityAction>
        {
            () => TaskManager.instance.ActivateTask(taskID),
            () => UpdateAllInfoPanelStatus()
        };
        var completeActions = new List<UnityEngine.Events.UnityAction>
        {
            () => TaskManager.instance.CompleteTask(taskID),
            () => UpdateAllInfoPanelStatus()
        };
        var deliverActions = new List<UnityEngine.Events.UnityAction>
        {
            () => TaskManager.instance.DeliverTask(taskID),
            () => UpdateAllInfoPanelStatus()
        };

        foreach (var wrapperPair in infoPanelFunctionWrappers)
        {
            var wrapper = wrapperPair.functionWrapper;
            if (wrapper == null) continue;

            var pairs = wrapper.GetInfoPanelPairs();
            foreach (var pair in pairs)
            {
                if (pair.taskID == taskID)
                {
                    wrapper.SetFunctionToActivateButton(taskID, activateActions);
                    wrapper.SetFunctionToCompleteButton(taskID, completeActions);
                    wrapper.SetFunctionToDeliverButton(taskID, deliverActions);
                    return;
                }
            }
        }
    }

    private void UpdateAllInfoPanelStatus()
    {
        foreach (var wrapperPair in infoPanelFunctionWrappers)
        {
            var wrapper = wrapperPair.functionWrapper;
            if (wrapper == null) continue;

            var pairs = wrapper.GetInfoPanelPairs();
            foreach (var pair in pairs)
            {
                wrapper.UpdateInfoPanelStatus(pair.taskID, GetStatusText(TaskManager.instance.TaskTree.GetNodeById(pair.taskID)));
            }
        }
    }

    private string BuildTaskTypeText(TaskNode node)
    {
        if (node.TType == null)
            return "なし";

        var typeNames = node.TType.Select(t => t.ToString());
        return string.Join(" / ", typeNames);
    }

    // 状態テキスト
    private string GetStatusText(TaskNode node)
    {
        var status = "";

        // メイン状態
        if (node.IsCompleted)
            status = "完了";
        else if (TaskManager.instance.IsCompletableTask(node.ID))
            status = "完了可能";
        else if (node.IsActive)
            status = "進行中";
        else if (TaskManager.instance.IsAvailableTask(node.ID))
            status = "受注可能";
        else
            status = "未開始";

        // 配達済みを追加
        if (node.IsDelivered)
            status += " / 配達済み";
        else if (TaskManager.instance.IsDeliverableTask(node.ID))
            status += " / 配達可能";
        else if (TaskManager.instance.IsDeliverTypeTask(node.ID))
            status += " / 未配達";

        return status;
    }

    // 代替子かどうか
    private bool IsAlternativeChild(TaskNode node)
    {
        var parents = node.ParentTasks ?? System.Array.Empty<TaskNode>();
        foreach (var p in parents)
        {
            if (p == null) continue;
            if (p.IsHavingAlternativeChildren &&
                p.AlternativeChildTasks != null &&
                p.AlternativeChildTasks.Contains(node))
            {
                return true;
            }
        }
        return false;
    }

    // 必要条件（収集・討伐）
    private string BuildNeedsText(TaskNode node)
    {
        var parts = new List<string>();
        var reqItems = node.RequiredItems ?? new List<TaskNode.CountById>();
        var targets = node.TargetEnemies ?? new List<TaskNode.CountById>();

        foreach (var i in reqItems) parts.Add($"収集: {i.id} x{i.count}");
        foreach (var e in targets) parts.Add($"討伐: {e.id} x{e.count}");

        return parts.Count > 0 ? string.Join(" / ", parts) : "なし";
    }

    // 報酬
    private string BuildRewardsText(TaskNode node)
    {
        var rewards = node.RewardItems ?? new List<TaskNode.CountById>();
        if (rewards.Count == 0) return "なし";
        var parts = new List<string>();
        foreach (var r in rewards) parts.Add($"報酬: {r.id} x{r.count}");
        return string.Join(" / ", parts);
    }

    private void SwitchTaskTreeGroup(int groupID)
    {
        foreach (var group in taskTreeUIGroups)
        {
            if (group.groupID == groupID)
            {
                group.groupObject.GetComponent<Canvas>().enabled = true;
            }
            else
            {
                group.groupObject.GetComponent<Canvas>().enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private bool IsGroupCountMatchingPrefabCount()
    {
        int groupCount = TaskManager.instance.TaskTree.GetAllGroupIDs().Count();
        int prefabCount = taskTreeUIGroupsPrefab?.Length ?? 0;
        return groupCount <= prefabCount;
    }

    private bool AreGroupIDsMatchingPrefabs(out string details)
    {
        int[] groupIDs = TaskManager.instance.TaskTree.GetAllGroupIDs().ToArray();
        int[] prefabIDs = taskTreeUIGroupsPrefab?.Select(p => p.groupID).ToArray() ?? System.Array.Empty<int>();

        var missing = groupIDs.Except(prefabIDs).ToArray(); // グループにあるがPrefabにないID

        if (missing.Length > 0)
        {
            details = $"タスクグループのIDがPrefabに見つかりません。欠損ID: {string.Join(",", missing)}";
            return false;
        }
        details = "";
        return true;
    }

    [System.Serializable]
    private struct TaskTreeUIGroup
    {
        public int groupID;
        public GameObject groupObject;
    }

    private struct InfoPanelFunctionWrapperPair
    {
        public int groupID;
        public InfoPanelFunctionWrapper functionWrapper;
    }
}
