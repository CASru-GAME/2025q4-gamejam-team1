using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TaskNodePostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var assetPath in importedAssets)
        {
            if (!assetPath.EndsWith(".asset")) continue;

            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset is TaskNode taskNode)
            {
                // TaskNodeが生成された
                AutoAddToTaskTree(taskNode, assetPath);
            }
        }

        // TaskNodeが削除された
        foreach (var deletedPath in deletedAssets)
        {
            if (!deletedPath.EndsWith(".asset")) continue;

            // 削除されたアセット名からTaskNodeの可能性を判断
            RemoveNodeFromAllTaskTrees(deletedPath);
        }
    }

    private static void AutoAddToTaskTree(TaskNode taskNode, string taskNodePath)
    {
        // TaskNodeのディレクトリを取得
        var taskNodeDir = System.IO.Path.GetDirectoryName(taskNodePath);

        // 同じディレクトリ以上の階層でTaskTreeを検索
        var taskTreeAssets = FindTaskTreeInHierarchy(taskNodeDir);

        if (taskTreeAssets.Count == 0)
        {
            Debug.LogWarning($"TaskNode '{taskNode.name}' の親階層にTaskTreeが見つかりません。", taskNode);
            return;
        }

        // 最も近いTaskTreeを選択（ディレクトリ階層が浅い順）
        var closestTaskTree = taskTreeAssets.OrderBy(x => x.path.Length).First();
        var taskTree = AssetDatabase.LoadAssetAtPath<TaskTree>(closestTaskTree.path);

        if (taskTree != null)
        {
            AddNodeToTaskTree(taskTree, taskNode);
        }
    }

    private static void RemoveNodeFromAllTaskTrees(string deletedAssetPath)
    {
        var allTaskTrees = AssetDatabase.FindAssets("t:TaskTree");

        foreach (var guid in allTaskTrees)
        {
            var taskTreePath = AssetDatabase.GUIDToAssetPath(guid);
            var taskTree = AssetDatabase.LoadAssetAtPath<TaskTree>(taskTreePath);

            if (taskTree == null) continue;

            RemoveNullNodesFromTaskTree(taskTree);
        }
    }

    private static void RemoveNullNodesFromTaskTree(TaskTree taskTree)
    {
        var nodes = taskTree.Nodes;
        var so = new SerializedObject(taskTree);
        var nodesProperty = so.FindProperty("nodes");

        // null参照とMissing参照（削除されたTaskNode）を除外して新しい配列を作成
        var validNodes = new List<TaskNode>();
        int removedCount = 0;

        for (int i = 0; i < nodesProperty.arraySize; i++)
        {
            var elementProperty = nodesProperty.GetArrayElementAtIndex(i);
            var nodeRef = elementProperty.objectReferenceValue as TaskNode;

            // null参照またはMissing参照をチェック
            if (nodeRef != null && elementProperty.objectReferenceValue != null)
            {
                validNodes.Add(nodeRef);
            }
            else
            {
                removedCount++;
            }
        }

        if (removedCount == 0)
        {
            return; // 変更がない場合はスキップ
        }

        // SerializedObjectで更新
        nodesProperty.arraySize = validNodes.Count;

        for (int i = 0; i < validNodes.Count; i++)
        {
            nodesProperty.GetArrayElementAtIndex(i).objectReferenceValue = validNodes[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(taskTree);
        AssetDatabase.SaveAssets();

        Debug.Log($"TaskTree '{taskTree.name}' から null参照・Missing参照のTaskNodeを {removedCount} 個除去しました。", taskTree);
    }

    private static List<(string path, int depth)> FindTaskTreeInHierarchy(string startDir)
    {
        var results = new List<(string, int)>();
        var normalizedStart = startDir.Replace("\\", "/");

        var allTaskTrees = AssetDatabase.FindAssets("t:TaskTree");
        foreach (var guid in allTaskTrees)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var assetDir = System.IO.Path.GetDirectoryName(assetPath).Replace("\\", "/");

            // TaskNodeのディレクトリがTaskTreeのディレクトリ以下か確認
            if (normalizedStart.StartsWith(assetDir))
            {
                int depth = assetDir.Split('/').Length;
                results.Add((assetPath, depth));
            }
        }

        return results;
    }

    private static void AddNodeToTaskTree(TaskTree taskTree, TaskNode taskNode)
    {
        var nodes = taskTree.Nodes;

        // 既に追加されているか確認
        if (System.Array.Exists(nodes, x => ReferenceEquals(x, taskNode)))
        {
            return;
        }

        // SerializedObjectで追加
        var so = new SerializedObject(taskTree);
        var nodesProperty = so.FindProperty("nodes");
        int newIndex = nodesProperty.arraySize;
        nodesProperty.arraySize = newIndex + 1;
        nodesProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = taskNode;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(taskTree);
        AssetDatabase.SaveAssets();

        Debug.Log($"TaskNode '{taskNode.name}' を TaskTree '{taskTree.name}' に追加しました。", taskTree);
    }
}
