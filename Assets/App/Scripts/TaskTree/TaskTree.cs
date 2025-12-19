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

        // グループごとの結果
        public List<GroupValidation> Groups;

        public int GroupCount => Groups != null ? Groups.Count : 0;
        public int TreeCount => Groups != null ? Groups.Sum(g => g.TreeCount) : 0;
        public int NodeCount => Groups != null ? Groups.Sum(g => g.NodeCount) : 0;

        public struct GroupValidation
        {
            public int GroupID;
            public List<List<TaskNode>> ComponentTopologicalOrders;
            public int TreeCount => ComponentTopologicalOrders != null ? ComponentTopologicalOrders.Count : 0;
            public int NodeCount => ComponentTopologicalOrders != null ? ComponentTopologicalOrders.Sum(l => l.Count) : 0;
        }
    }

    /// <summary>
    /// TaskTree の整合性チェック（TaskGroupIDごとに森対応）。
    /// - 参照のnull/外部参照/自己参照
    /// - 親子の相互参照整合性
    /// - NeededCompletedParentTasksと親配列の整合
    /// - ID重複/0
    /// - グループ跨ぎ参照の検出（エラー）
    /// - DAG性（各グループ・各連結成分でサイクル検出）
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
            Groups = new List<ValidationResult.GroupValidation>()
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

        // まず全ノード走査（共通チェック・ID重複・グループ収集）
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

            // グループIDの軽微なチェック（0/負値は警告）
            if (n.TaskGroupID <= 0)
            {
                result.Warnings.Add($"TaskNode '{n.name}' の TaskGroupID が {n.TaskGroupID} です。正の値を推奨します。");
            }

            // NeededCompletedParentTasks と親の整合
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

            // 代替子タスクとの整合
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

        // ID重複
        foreach (var kv in idBuckets)
        {
            if (kv.Value.Count > 1)
            {
                var names = string.Join(", ", kv.Value.Select(x => x.name));
                result.Errors.Add($"同一 ID({kv.Key}) の TaskNode が複数含まれています: {names}");
            }
        }

        // 参照チェック・グラフ準備（全体で持ち、後でグループで絞る）
        var edges = new Dictionary<TaskNode, HashSet<TaskNode>>();
        var undirected = new Dictionary<TaskNode, HashSet<TaskNode>>();
        foreach (var n in nodeList)
        {
            edges[n] = new HashSet<TaskNode>();
            undirected[n] = new HashSet<TaskNode>();
        }

        void CheckRefs(TaskNode owner, IEnumerable<TaskNode> refs, string label)
        {
            int idx = 0;
            foreach (var r in refs)
            {
                if (r == null)
                {
                    result.Errors.Add($"'{owner.name}' の {label}[{idx}] が null です。");
                }
                else
                {
                    if (ReferenceEquals(r, owner))
                        result.Errors.Add($"'{owner.name}' の {label}[{idx}] が自己参照になっています。");
                    if (!nodeSet.Contains(r))
                        result.Errors.Add($"'{owner.name}' の {label}[{idx}] が TaskTree 外の TaskNode '{r.name}' を参照しています。");
                    // グループ跨ぎ参照は禁止
                    if (nodeSet.Contains(r) && r.TaskGroupID != owner.TaskGroupID)
                        result.Errors.Add($"グループ跨ぎ参照: '{owner.name}'(Group:{owner.TaskGroupID}) の {label}[{idx}] → '{r.name}'(Group:{r.TaskGroupID})。同一 TaskGroupID 内にしてください。");
                }
                idx++;
            }
        }

        // エッジ構築と相互参照整合、無向リンク（グループ内のみ）
        foreach (var n in nodeList)
        {
            var parents = n.ParentTasks ?? System.Array.Empty<TaskNode>();
            var children = new List<TaskNode>();
            if (n.ChildTasks != null) children.AddRange(n.ChildTasks);
            if (considerAlternativeChildren && n.IsHavingAlternativeChildren && n.AlternativeChildTasks != null)
                children.AddRange(n.AlternativeChildTasks);

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

            // 有向辺（同一グループのみ）
            foreach (var c in children)
            {
                if (c == null || !nodeSet.Contains(c) || ReferenceEquals(c, n)) continue;
                if (c.TaskGroupID != n.TaskGroupID) continue; // 既にエラー済み。グラフには追加しない
                edges[n].Add(c);
            }

            // 無向隣接（親・子・代替子、同一グループのみ）
            void LinkUndirected(TaskNode a, TaskNode b)
            {
                if (a == null || b == null) return;
                if (!nodeSet.Contains(a) || !nodeSet.Contains(b)) return;
                if (ReferenceEquals(a, b)) return;
                if (a.TaskGroupID != b.TaskGroupID) return;
                undirected[a].Add(b);
                undirected[b].Add(a);
            }
            foreach (var c in n.ChildTasks ?? System.Array.Empty<TaskNode>()) LinkUndirected(n, c);
            foreach (var p in parents) LinkUndirected(n, p);
            if (considerAlternativeChildren)
                foreach (var ac in n.AlternativeChildTasks ?? new List<TaskNode>()) LinkUndirected(n, ac);
        }

        // グループ分割
        var groups = nodeList.GroupBy(n => n.TaskGroupID)
                             .ToDictionary(g => g.Key, g => g.ToList());

        // 各グループ内で連結成分抽出 → 各成分ごとにトポロジカルソート
        foreach (var (groupId, gNodes) in groups)
        {
            // 連結成分（グループ内）
            var visited = new HashSet<TaskNode>();
            var components = new List<List<TaskNode>>();

            foreach (var n in gNodes)
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
                components.Add(comp);
            }

            var groupOrders = new List<List<TaskNode>>();

            foreach (var comp in components)
            {
                var compSet = new HashSet<TaskNode>(comp);

                // 成分内入次数を計算（グループ・成分内エッジのみ）
                var indegWork = new Dictionary<TaskNode, int>();
                foreach (var u in comp) indegWork[u] = 0;
                foreach (var u in comp)
                    foreach (var v in edges[u])
                        if (compSet.Contains(v))
                            indegWork[v] = indegWork[v] + 1;

                var roots = indegWork.Where(kv => kv.Value == 0)
                                     .Select(kv => kv.Key)
                                     .OrderBy(x => x.ID)
                                     .ToList();
                if (roots.Count == 0)
                {
                    result.Errors.Add($"[Group:{groupId}] 成分にルートが存在しません（サイクルの可能性）。成分ノード: {string.Join(", ", comp.Select(x => x.name))}");
                }

                // Kahn法（決定的: ID昇順）
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
                    ready.Sort((a, b) => a.ID.CompareTo(b.ID));
                    foreach (var v in ready)
                        queue.Enqueue(v);
                }

                groupOrders.Add(topo);

                if (topo.Count != comp.Count)
                {
                    var cycleCandidates = comp.Where(x => indegWork[x] > 0).Select(x => x.name);
                    result.Errors.Add($"[Group:{groupId}] グラフにサイクルが存在します（成分内）。候補ノード: {string.Join(", ", cycleCandidates)}");
                }
            }

            result.Groups.Add(new ValidationResult.GroupValidation
            {
                GroupID = groupId,
                ComponentTopologicalOrders = groupOrders
            });
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
    sb.AppendLine($"- Groups: {res.GroupCount}");
    sb.AppendLine($"- Trees: {res.TreeCount}");
    sb.AppendLine($"- Valid: {res.IsValid}");
    sb.AppendLine($"- Errors: {res.Errors.Count}");
    foreach (var e in res.Errors) sb.AppendLine($"  • {e}");
    sb.AppendLine($"- Warnings: {res.Warnings.Count}");
    foreach (var w in res.Warnings) sb.AppendLine($"  • {w}");

    // 各グループ・各成分のトポ順をアスキーアートで出力
    foreach (var g in res.Groups.OrderBy(g => g.GroupID))
    {
        sb.AppendLine($"[Group:{g.GroupID}] Trees: {g.TreeCount}, Nodes: {g.NodeCount}");
        foreach (var order in g.ComponentTopologicalOrders)
        {
            sb.AppendLine("  - ASCII Art Representation:");
            foreach (var node in order)
            {
                sb.AppendLine($"    {node.name}(ID:{node.ID})");
                // 子ノードを表示
                if (node.ChildTasks != null)
                {
                    foreach (var child in node.ChildTasks)
                    {
                        sb.AppendLine($"      └─ {child.name}(ID:{child.ID})");
                    }
                }
                // 代替子ノードを表示
                if (node.IsHavingAlternativeChildren && node.AlternativeChildTasks != null)
                {
                    foreach (var altChild in node.AlternativeChildTasks)
                    {
                        sb.AppendLine($"      ├─ [Alt] {altChild.name}(ID:{altChild.ID})");
                    }
                }
            }
        }
    }

    if (res.IsValid)
        Debug.Log(sb.ToString(), this);
    else
        Debug.LogError(sb.ToString(), this);
}
}
