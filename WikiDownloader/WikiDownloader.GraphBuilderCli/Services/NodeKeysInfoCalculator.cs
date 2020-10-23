using System;
using System.Collections.Generic;
using System.Linq;
using WikiDownloader.Abstractions.Models;
using WikiDownloader.GraphBuilderCli.Models;

namespace WikiDownloader.GraphBuilderCli.Services
{
    public static class NodeKeysInfoCalculator
    {
        public static NodeKeysInfo Build(WikiPageTitle[] titles)
        {
            var dict = titles.ToDictionary(x => x.Name);
            var result = new NodeKeysInfo
            {
                BadTargetReference = new HashSet<string>(),
                CycleReference = new HashSet<string>(),
                References = new Dictionary<string, string>(),
                Regular = new HashSet<string>(),
            };

            foreach (var title in dict.Keys)
            {
                var info = GetNodeInfo(dict, title);

                switch (info.Type)
                {
                    case NodeType.Regular:
                        result.Regular.Add(title);
                        break;
                    case NodeType.Reference:
                        result.References.Add(title, info.Reference);
                        break;
                    case NodeType.CycleReference:
                        result.CycleReference.Add(title);
                        break;
                    case NodeType.BadReference:
                        result.BadTargetReference.Add(title);
                        break;
                }
            }

            return result;
        }

        private enum NodeType
        {
            Regular,
            Reference,
            CycleReference,
            BadReference,
        }

        private class NodeInfo
        {
            public NodeType Type { get; }

            public string Reference { get; }

            public NodeInfo(NodeType type)
            {
                if (type == NodeType.Reference)
                {
                    throw new InvalidOperationException();
                }

                Type = type;
            }

            public NodeInfo(string reference)
            {
                Reference = reference;
                Type = NodeType.Reference;
            }
        }

        private static NodeInfo GetNodeInfo(IReadOnlyDictionary<string, WikiPageTitle> dict, string title)
        {
            var stack = new Stack<string>();

            if (dict[title].ReferenceName == null)
            {
                return new NodeInfo(NodeType.Regular);
            }

            stack.Push(dict[title].Name);

            while (true)
            {
                var current = stack.Peek();
                var next = dict[current].ReferenceName;

                if (!dict.TryGetValue(next, out var value))
                {
                    return new NodeInfo(NodeType.BadReference);
                }

                if (value.ReferenceName == null)
                {
                    return new NodeInfo(value.Name);
                }

                if (stack.Contains(next))
                {
                    return new NodeInfo(NodeType.CycleReference);
                }

                stack.Push(next);
            }
        }
    }
}
