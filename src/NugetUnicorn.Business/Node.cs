using System;
using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Dto;
using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business
{
    public class Node<T>
        where T : class
    {
        public T Value { get; protected set; }

        private IList<Node<T>> Childs { get; }

        public Node<T> Parent { get; protected set; }

        protected Node()
            : this(null, null)
        {
        }

        public Node(T value)
            : this(value, null)
        {
        }

        public Node(T value, Node<T> parentNode)
        {
            Value = value;
            Parent = parentNode;
            Childs = new List<Node<T>>();
        }

        public Node<T> AddAll(IEnumerable<Node<T>> values)
        {
            values.ForEachItem(
                x =>
                    {
                        x.Parent = this;
                        Childs.Add(x);
                    });
            return this;
        }

        public void ForEachItem(Action<Node<T>> nodeItemAction)
        {
            nodeItemAction(this);
            Childs.ForEachItem(x => x.ForEachItem(nodeItemAction));
        }

        public TV Filter<TV>(Func<T, bool> filterFunc) where TV : Node<T>, new()
        {
            var thisSatisfies = filterFunc(Value);
            if (!thisSatisfies)
            {
                return null;
            }

            var filteredChildren = Childs.Select(x => x.Filter<TV>(filterFunc)).Where(x => x != null);
            var v = new TV
                        {
                            Value = Value,
                            Parent = Parent
                        };
            v.AddAll(filteredChildren);
            return v;
        }
    }

    public class PackageNode : Node<PackageDto>
    {
        public PackageNode()
        {
        }

        public PackageNode(PackageDto value)
            : base(value)
        {
        }

        public PackageNode(PackageDto value, Node<PackageDto> parentNode)
            : base(value, parentNode)
        {
        }
    }
}