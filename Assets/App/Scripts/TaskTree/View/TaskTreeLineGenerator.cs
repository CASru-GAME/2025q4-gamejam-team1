using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskTreeLineGenerator : MonoBehaviour
{
    [SerializeField] private int groupID;
    [SerializeField] private NodeObjectPair[] nodeObjectPairs;
    [SerializeField] private TaskTree taskTreeModel;

    [Header("Auto node instantiate")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float autoLayoutRadius = 3f;

    [Header("Edge settings")]
    [SerializeField] private GameObject edgePrefab;   // LineRenderer を持つプレハブ
    [SerializeField] private Color edgeFromColor = Color.white;
    [SerializeField] private Color edgeToColor = Color.white;
    [SerializeField] private float canvasLineWidth = 2f;  // 追加：Canvas 線幅

    [Header("Links (from -> to)")]
    [SerializeField] private List<LinkPair> links = new();

    private readonly List<EdgeRecord> _edges = new();

    [System.Serializable]
    public struct NodeObjectPair
    {
        public int nodeID;
        public GameObject nodeObject;
    }

    [System.Serializable]
    public struct LinkPair
    {
        public int fromId;
        public int toId;
    }

    private class EdgeRecord
    {
        public LineRenderer line;
        public Image image;  // 追加：Canvas UI Image
        public Transform from;
        public Transform to;
    }

    public List<InfoPanelPair> GetInfoPanelPairs()
    {
        var result = new List<InfoPanelPair>();

        foreach (var pair in nodeObjectPairs)
        {
            if (pair.nodeObject == null) continue;

            var infoPanelManager = pair.nodeObject.GetComponent<InfoPanelManager>();
            if (infoPanelManager != null)
            {
                result.Add(new InfoPanelPair(infoPanelManager, pair.nodeID));
            }
        }

        return result;
    }

    public void ClearEdges()
    {
        foreach (var e in _edges)
        {
            if (e.line) SafeDestroy(e.line.gameObject);
        }
        _edges.Clear();

        // 追跡外の孤児 LineRenderer も掃除
        CleanupOrphanEdges();
    }

    // エディタでは即時破棄
    private void SafeDestroy(GameObject go)
    {
        if (go == null) return;
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    // 子階層に残っている LineRenderer を一掃
    private void CleanupOrphanEdges()
    {
        // 追加：再コンパイル直後など _edges が空のときは掃除しない
        if (_edges.Count == 0) return;

        var tracked = new HashSet<Image>(_edges.Where(x => x.image).Select(x => x.image));
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.name.StartsWith("Edge_") && !tracked.Contains(img))
                SafeDestroy(img.gameObject);
        }
    }

    [ContextMenu("Build Or Update Edges")]
    private void BuildOrUpdateEdges()
    {
        RefreshLinksFromTaskTree();
        CleanupOrphanEdges();

        if (_edges.Count == 0)
        {
            BuildCanvasEdges(links.Select(l => (l.fromId, l.toId)));
        }
        else
        {
            ApplyAllEdgePositions();
        }
    }

    private void ApplyAllEdgePositions()
    {
        foreach (var e in _edges)
        {
            if (e.image && e.from && e.to)
            {
                var fromRect = e.from.GetComponent<RectTransform>();
                var toRect = e.to.GetComponent<RectTransform>();
                if (fromRect && toRect)
                    ApplyCanvasLinePositions(e.image.rectTransform, fromRect, toRect);
            }
        }
    }

    /// <summary>
    /// Canvas UI (RectTransform) 用のエッジ生成
    /// </summary>
    private void BuildCanvasEdges(IEnumerable<(int fromId, int toId)> links)
    {
        ClearEdges();
        var map = BuildNodeMap();

        int created = 0;
        int skipped = 0;

        foreach (var (fromId, toId) in links)
        {
            if (!map.TryGetValue(fromId, out var fromTf) || !map.TryGetValue(toId, out var toTf))
            {
                Debug.LogWarning($"リンクをスキップ: Transform 未検出 fromId={fromId}, toId={toId}");
                skipped++;
                continue;
            }

            var fromRect = fromTf.GetComponent<RectTransform>();
            var toRect = toTf.GetComponent<RectTransform>();
            if (!fromRect || !toRect)
            {
                Debug.LogWarning($"RectTransform がありません: fromId={fromId}, toId={toId}");
                skipped++;
                continue;
            }

            var lineGO = new GameObject($"Edge_{fromId}_to_{toId}");
            lineGO.transform.SetParent(transform, false);
            lineGO.transform.localScale = Vector3.one;

            var image = lineGO.AddComponent<Image>();
            image.raycastTarget = false;
            image.type = Image.Type.Sliced;

            var fromColor = edgeFromColor;
            var toColor = edgeToColor;
            image.color = fromColor;

            var rectTf = lineGO.GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            var scale = canvas ? canvas.scaleFactor : 1f;
            ApplyCanvasLinePositions(rectTf, fromRect, toRect, scale);

            _edges.Add(new EdgeRecord { image = image, from = fromTf, to = toTf });
            created++;
        }

        Debug.Log($"Canvas Edge 生成: 作成={created}, スキップ={skipped}");
    }


    /// <summary>
    /// Canvas 線の位置・角度・サイズを更新
    /// </summary>
    private void ApplyCanvasLinePositions(RectTransform lineRect, RectTransform fromRect, RectTransform toRect, float scale = 1f)
    {
        var fromPos = fromRect.anchoredPosition;
        var toPos = toRect.anchoredPosition;
        var direction = toPos - fromPos;
        var distance = direction.magnitude;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.sizeDelta = new Vector2(distance, canvasLineWidth * scale); // 幅にスケールを反映
        lineRect.anchoredPosition = (fromPos + toPos) / 2f;
        lineRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private void RefreshLinksFromTaskTree()
    {
        if (taskTreeModel == null)
        {
            Debug.LogWarning("TaskTreeModel が未設定です。Inspector の links を使用します。");
            // links を上書きしない
            return;
        }

        var newLinks = BuildLinkPairsFromTaskTree(groupID);
        if (newLinks == null || newLinks.Count == 0)
        {
            Debug.LogWarning($"TaskTree からリンクが取得できませんでした (groupID={groupID})。Inspector の links を使用します。");
            // links を上書きしない
            return;
        }

        links = newLinks;
        Debug.Log($"TaskTree から {links.Count} 件のリンクを取得しました。");
    }

    private List<LinkPair> BuildLinkPairsFromTaskTree(int targetGroupId)
    {
        var result = new List<LinkPair>();

        var groupNodes = taskTreeModel?.GetNodesByGroupID(targetGroupId);
        if (groupNodes == null || groupNodes.Count == 0) return result;

        var visited = new HashSet<TaskNode>();

        void Traverse(TaskNode node)
        {
            if (node == null) return;
            if (!visited.Add(node)) return;

            var children = node.ChildTasks; // 例: node.Children
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child == null) continue;
                    result.Add(new LinkPair
                    {
                        fromId = node.ID,   // 例: node.Id
                        toId = child.ID     // 例: child.Id
                    });
                    Traverse(child);
                }
            }
        }

        foreach (var n in groupNodes)
        {
            Traverse(n);
        }

        return result;
    }

    [ContextMenu("Auto Build Nodes (Instantiate from TaskTree)")]
    private void AutoBuildNodesFromTaskTree()
    {
        var tree = taskTreeModel;
        if (tree == null)
        {
            Debug.LogWarning("TaskTreeModel が未設定です。");
            return;
        }

        var groupNodes = tree.GetNodesByGroupID(groupID);
        if (groupNodes == null || groupNodes.Count == 0)
        {
            Debug.LogWarning($"GroupID {groupID} の TaskNode がありません。");
            return;
        }

        // 既存の子オブジェクトをクリア
        void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
        var toDelete = new List<GameObject>();
        foreach (Transform child in transform) toDelete.Add(child.gameObject);
        foreach (var go in toDelete) SafeDestroy(go);

        nodeObjectPairs = new NodeObjectPair[groupNodes.Count];

        for (int i = 0; i < groupNodes.Count; i++)
        {
            var node = groupNodes[i];
            GameObject go;
            if (nodePrefab != null)
            {
                go = Instantiate(nodePrefab, transform);
                go.name = $"{nodePrefab.name}_{node.ID}";
            }
            else
            {
                go = new GameObject($"Node_{node.ID}");
                go.transform.SetParent(transform, false);
            }

            // 簡易レイアウト（円形配置）
            float angle = (Mathf.PI * 2f * i) / groupNodes.Count;
            var pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * autoLayoutRadius;
            go.transform.localPosition = pos;

            nodeObjectPairs[i] = new NodeObjectPair
            {
                nodeID = node.ID,
                nodeObject = go
            };
        }

        InitializeInfoPanelsFromNodes();   // 追加：生成後に全 InfoPanelManager を初期化

        // シリアライズ反映
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log($"GroupID {groupID} の {groupNodes.Count} ノードを自動生成し、nodeObjectPairs を更新しました。");
    }

    [ContextMenu("Apply Edge Color")]
    private void ApplyEdgeColor()
    {
        // 既存エッジにもグラデーションを反映
        foreach (var e in _edges)
        {
            if (!e.line || !e.from || !e.to) continue;

            var fromColor = edgeFromColor;
            var toColor = edgeToColor;

            e.line.startColor = fromColor;
            e.line.endColor = toColor;
        }
    }

    private Dictionary<int, Transform> BuildNodeMap()
    {
        var map = nodeObjectPairs
            .Where(p => p.nodeObject != null)
            .ToDictionary(p => p.nodeID, p => p.nodeObject.transform);

        if (map.Count == 0)
        {
            Debug.LogWarning("nodeObjectPairs が空、または nodeObject が未設定です。'Auto Build Nodes (Instantiate from TaskTree)' を実行するか、手動で割り当ててください。");
        }
        return map;
    }

    private bool ValidateEdgePrefab()
    {
        if (edgePrefab == null)
        {
            Debug.LogWarning("edgePrefab が未設定です。LineRenderer を持つプレハブを割り当ててください。");
            return false;
        }
        if (edgePrefab.GetComponent<LineRenderer>() == null)
        {
            Debug.LogWarning("edgePrefab に LineRenderer コンポーネントがありません。");
            return false;
        }
        return true;
    }

    [ContextMenu("Validate Setup")]
    private void ValidateSetup()
    {
        var hasModel = taskTreeModel != null;
        var map = BuildNodeMap();

        Debug.Log($"検証: TaskTreeModel={(hasModel ? "OK" : "None")}, groupID={groupID}, nodeMapCount={map.Count}, linksCount={links?.Count ?? 0}, edgePrefabHasLR={ValidateEdgePrefab()}");
    }

    // 追加：生成されたノード上の InfoPanelManager を一括初期化
    private void InitializeInfoPanelsFromNodes()
    {
        var managers = new List<InfoPanelManager>();

        foreach (var pair in nodeObjectPairs)
        {
            if (pair.nodeObject == null) continue;
            var mgr = pair.nodeObject.GetComponent<InfoPanelManager>();
            if (mgr != null) managers.Add(mgr);
        }

        // 取得できた全パネルへ同一リストを渡す
        foreach (var mgr in managers)
        {
            mgr.Initialize(managers);
        }
    }
}
