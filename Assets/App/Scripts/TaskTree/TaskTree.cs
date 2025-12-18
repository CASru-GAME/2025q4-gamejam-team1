using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

[CreateAssetMenu(fileName = "TaskTree", menuName = "Task/TaskTree")]
public class TaskTree : ScriptableObject
{
    [SerializeField] private TaskNode[] nodes;

    public TaskNode[] Nodes => nodes;
    public TaskNode GetNodeByID(int id)
    {
        foreach (var node in nodes)
        {
            if (node.ID == id)
                return node;
        }
        return null;
    }
    public List<TaskNode> GetActiveNodes()
    {
        List<TaskNode> activeNodes = new List<TaskNode>();
        foreach (var node in nodes)
        {
            if (node.IsActive)
                activeNodes.Add(node);
        }
        return activeNodes;
    }

    public List<TaskNode> GetCompletedNodes()
    {
        List<TaskNode> completedNodes = new List<TaskNode>();
        foreach (var node in nodes)
        {
            if (node.IsCompleted)
                completedNodes.Add(node);
        }
        return completedNodes;
    }

    public int GetTotalNodes()
    {
        return nodes.Length;
    }
    public struct ValidationResult
    {
        public bool IsValid;
        public List<string> Errors;
        public List<string> Warnings;

        // 各木（連結成分）ごとのトポロジカル順
        public List<List<TaskNode>> ComponentTopologicalOrders;

        public int TreeCount => ComponentTopologicalOrders != null ? ComponentTopologicalOrders.Count : 0;
        public int NodeCount => ComponentTopologicalOrders != null ? ComponentTopologicalOrders.Sum(l => l.Count) : 0;
    }

    /// <summary>
    /// TaskTree の整合性チェック（森対応）。
    /// - 参照のnull/外部参照/自己参照
    /// - 親子の相互参照整合性
    /// - NeededCompletedParentTasksと親配列の整合
    /// - ID重複/0
    /// - DAG性（各連結成分でサイクル検出）
    /// - ルート存在（各成分で入次数0ノードが少なくとも1つ）
    /// - 各成分のトポロジカル順（決定的: ID昇順）
    /// </summary>
    public ValidationResult Validate(bool considerAlternativeChildren = true)
    {
        var result = new ValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            Warnings = new List<string>(),
            ComponentTopologicalOrders = new List<List<TaskNode>>()
        };

        if (nodes == null || nodes.Length == 0)
        {
            result.Errors.Add("nodes が空です。TaskTree に1つ以上の TaskNode を設定してください。");
            result.IsValid = false;
            return result;
        }

        var nodeList = new List<TaskNode>();
        var nodeSet = new HashSet<TaskNode>();
        var idBuckets = new Dictionary<int, List<TaskNode>>();

        for (int i = 0; i < nodes.Length; i++)
        {
            var n = nodes[i];
            if (n == null)
            {
                result.Errors.Add($"nodes[{i}] が null です。");
                continue;
            }
            if (!nodeSet.Add(n))
            {
                result.Errors.Add($"nodes に同一参照の TaskNode が重複しています: {n.name} (ID:{n.ID})");
            }
            nodeList.Add(n);

            if (n.ID <= 0)
            {
                result.Errors.Add($"TaskNode '{n.name}' の ID が 0 または負の値です。");
            }
            if (!idBuckets.TryGetValue(n.ID, out var list))
            {
                list = new List<TaskNode>();
                idBuckets[n.ID] = list;
            }
            list.Add(n);

            var parents = n.ParentTasks ?? System.Array.Empty<TaskNode>();
            switch (n.NeededCompletedParents)
            {
                case TaskNode.NeededCompletedParentTasks.None:
                    if (parents.Length > 0)
                        result.Errors.Add($"'{n.name}' は NeededCompletedParentTasks=None ですが親が {parents.Length} 存在します。");
                    break;
                case TaskNode.NeededCompletedParentTasks.Any:
                case TaskNode.NeededCompletedParentTasks.All:
                    if (parents.Length == 0)
                        result.Errors.Add($"'{n.name}' は NeededCompletedParentTasks={n.NeededCompletedParents} ですが親が存在しません。");
                    break;
            }

            var alt = n.AlternativeChildTasks;
            if (n.IsHavingAlternativeChildren)
            {
                if (alt == null || alt.Count == 0)
                    result.Errors.Add($"'{n.name}' は代替子タスクフラグが true ですが AlternativeChildTasks が空です。");
            }
            else
            {
                if (alt != null && alt.Count > 0)
                    result.Warnings.Add($"'{n.name}' は代替子タスクフラグが false ですが AlternativeChildTasks に要素があります。");
            }
        }

        foreach (var kv in idBuckets)
        {
            if (kv.Value.Count > 1)
            {
                var names = string.Join(", ", kv.Value.Select(x => x.name));
                result.Errors.Add($"同一 ID({kv.Key}) の TaskNode が複数含まれています: {names}");
            }
        }

        // 有向辺（子方向）と入次数
        var edges = new Dictionary<TaskNode, HashSet<TaskNode>>();
        var indeg = new Dictionary<TaskNode, int>();
        foreach (var n in nodeList)
        {
            edges[n] = new HashSet<TaskNode>();
            indeg[n] = 0;
        }

        // 無向隣接（連結成分抽出用）：親子・代替子も双方向に張る
        var undirected = new Dictionary<TaskNode, HashSet<TaskNode>>();
        foreach (var n in nodeList)
            undirected[n] = new HashSet<TaskNode>();

        void CheckRefs(TaskNode n, IEnumerable<TaskNode> refs, string label)
        {
            int idx = 0;
            foreach (var r in refs)
            {
                if (r == null)
                {
                    result.Errors.Add($"'{n.name}' の {label}[{idx}] が null です。");
                }
                else
                {
                    if (ReferenceEquals(r, n))
                        result.Errors.Add($"'{n.name}' の {label}[{idx}] が自己参照になっています。");
                    if (!nodeSet.Contains(r))
                        result.Errors.Add($"'{n.name}' の {label}[{idx}] が TaskTree 外の TaskNode '{r.name}' を参照しています。");
                }
                idx++;
            }
        }

        foreach (var n in nodeList)
        {
            var children = new List<TaskNode>();
            if (n.ChildTasks != null) children.AddRange(n.ChildTasks);
            if (considerAlternativeChildren && n.IsHavingAlternativeChildren && n.AlternativeChildTasks != null)
                children.AddRange(n.AlternativeChildTasks);

            var parents = n.ParentTasks ?? System.Array.Empty<TaskNode>();

            CheckRefs(n, n.ChildTasks ?? System.Array.Empty<TaskNode>(), "ChildTasks");
            CheckRefs(n, parents, "ParentTasks");
            if (considerAlternativeChildren)
                CheckRefs(n, n.AlternativeChildTasks ?? new List<TaskNode>(), "AlternativeChildTasks");

            // 親子の相互参照整合
            if (n.ChildTasks != null)
            {
                foreach (var c in n.ChildTasks)
                {
                    if (c == null) continue;
                    var cps = c.ParentTasks ?? System.Array.Empty<TaskNode>();
                    if (!cps.Contains(n))
                        result.Errors.Add($"親子不整合: '{n.name}' → '{c.name}' は ChildTasks ですが、'{c.name}' の ParentTasks に '{n.name}' がありません。");
                }
            }
            if (parents != null)
            {
                foreach (var p in parents)
                {
                    if (p == null) continue;
                    var pcs = p.ChildTasks ?? System.Array.Empty<TaskNode>();
                    if (!pcs.Contains(n))
                        result.Errors.Add($"親子不整合: '{n.name}' は '{p.name}' を ParentTasks に持ちますが、'{p.name}' の ChildTasks に '{n.name}' がありません。");
                }
            }

            // 有向辺（子方向）
            foreach (var c in children)
            {
                if (c == null || !nodeSet.Contains(c) || ReferenceEquals(c, n)) continue;
                if (edges[n].Add(c))
                    indeg[c] = indeg[c] + 1;
            }

            // 無向隣接（親・子・代替子すべて）
            void LinkUndirected(TaskNode a, TaskNode b)
            {
                if (a == null || b == null) return;
                if (!nodeSet.Contains(a) || !nodeSet.Contains(b)) return;
                if (ReferenceEquals(a, b)) return;
                undirected[a].Add(b);
                undirected[b].Add(a);
            }

            foreach (var c in n.ChildTasks ?? System.Array.Empty<TaskNode>()) LinkUndirected(n, c);
            foreach (var p in parents) LinkUndirected(n, p);
            if (considerAlternativeChildren)
                foreach (var ac in n.AlternativeChildTasks ?? new List<TaskNode>()) LinkUndirected(n, ac);
        }

        // 連結成分（木）を抽出
        var visited = new HashSet<TaskNode>();
        var components = new List<List<TaskNode>>();

        foreach (var n in nodeList)
        {
            if (visited.Contains(n)) continue;
            var comp = new List<TaskNode>();
            var q = new Queue<TaskNode>();
            q.Enqueue(n);
            visited.Add(n);
            while (q.Count > 0)
            {
                var u = q.Dequeue();
                comp.Add(u);
                foreach (var v in undirected[u])
                {
                    if (visited.Add(v))
                        q.Enqueue(v);
                }
            }
            // 単独ノード（孤立）も1成分として扱う
            components.Add(comp);
        }

        // 各成分ごとにトポロジカルソート（Kahn法、ID昇順で決定化）
        foreach (var comp in components)
        {
            // 成分内の入次数と辺だけで Kahn を回す
            var compSet = new HashSet<TaskNode>(comp);
            var indegWork = new Dictionary<TaskNode, int>();
            foreach (var u in comp)
            {
                indegWork[u] = 0;
            }
            foreach (var u in comp)
            {
                foreach (var v in edges[u])
                {
                    if (compSet.Contains(v))
                        indegWork[v] = indegWork[v] + 1;
                }
            }

            var roots = indegWork.Where(kv => kv.Value == 0).Select(kv => kv.Key).OrderBy(x => x.ID).ToList();
            if (roots.Count == 0)
            {
                result.Errors.Add($"成分にルートが存在しません（サイクルの可能性）。成分ノード: {string.Join(", ", comp.Select(x => x.name))}");
            }

            var topo = new List<TaskNode>();
            var ready = new List<TaskNode>(roots);
            var queue = new Queue<TaskNode>(ready);

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                topo.Add(u);

                ready.Clear();
                foreach (var v in edges[u])
                {
                    if (!compSet.Contains(v)) continue;
                    indegWork[v] = indegWork[v] - 1;
                    if (indegWork[v] == 0)
                        ready.Add(v);
                }
                // 決定的順序（ID昇順）
                ready.Sort((a, b) => a.ID.CompareTo(b.ID));
                foreach (var v in ready)
                    queue.Enqueue(v);
            }

            result.ComponentTopologicalOrders.Add(topo);

            if (topo.Count != comp.Count)
            {
                var cycleCandidates = comp.Where(x => indegWork[x] > 0).Select(x => x.name);
                result.Errors.Add($"グラフにサイクルが存在します（成分内）。候補ノード: {string.Join(", ", cycleCandidates)}");
            }
        }

        if (result.Errors.Count > 0)
            result.IsValid = false;

        return result;
    }

    [ContextMenu("Validate TaskTree (Forest)")]
    private void ValidateFromContextMenu()
    {
        var res = Validate(true);
        var sb = new StringBuilder();
        sb.AppendLine($"[TaskTree Validation] '{name}'");
        sb.AppendLine($"- Nodes: {nodes?.Length ?? 0}");
        sb.AppendLine($"- Trees: {res.TreeCount}");
        sb.AppendLine($"- Valid: {res.IsValid}");
        sb.AppendLine($"- Errors: {res.Errors.Count}");
        foreach (var e in res.Errors) sb.AppendLine($"  • {e}");
        sb.AppendLine($"- Warnings: {res.Warnings.Count}");
        foreach (var w in res.Warnings) sb.AppendLine($"  • {w}");

        for (int i = 0; i < res.ComponentTopologicalOrders.Count; i++)
        {
            var order = res.ComponentTopologicalOrders[i];
            var line = order != null && order.Count > 0
                ? string.Join(" -> ", order.Select(n => $"{n.name}(ID:{n.ID})"))
                : "(empty)";
            sb.AppendLine($"- Topological Order [#{i + 1}]: {line}");
        }

        if (res.IsValid)
            Debug.Log(sb.ToString(), this);
        else
            Debug.LogError(sb.ToString(), this);
    }
}
