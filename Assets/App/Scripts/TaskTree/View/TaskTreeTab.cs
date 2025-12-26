using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq; // 追加

public class TaskTreeTab : MonoBehaviour
{
    private static TaskTreeTab instance;
    public static TaskTreeTab Instance => instance;
    [SerializeField] private GameObject taskTreeTabObject;
    [SerializeField] private GameObject taskTreeSelectButtonObject;
    [SerializeField] private TaskTreeUIGroup[] taskTreeUIGroupsPrefab;
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
}
