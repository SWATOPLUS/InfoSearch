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
            var current = dict[title];

            if (current.ReferenceName == null)
            {
                return new NodeInfo(NodeType.Regular);
            }

            while (true)
            {
                if (dict.TryGetValue(current.Name, out var value))
                {
                    if (value.ReferenceName != null)
                    {
                        return new NodeInfo(value.Name);
                    }

                    if (stack.Contains(current.Name))
                    {
                        return new NodeInfo(NodeType.CycleReference);
                    }

                    stack.Push(current.Name);
                }
                else
                {
                    return new NodeInfo(NodeType.BadReference);
                }
            }
        }
    }
}
