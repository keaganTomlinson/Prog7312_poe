using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prog_p1.Data
{
    public class DeweyTreeBuilder
    {
        public TreeNode BuildTreeFromFile(string filePath)
        {
            TreeNode root = new TreeNode("Root", "Dewey Decimal System");

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(' ');
                string callNumber = parts[0];
                string description = string.Join(" ", parts, 1, parts.Length - 1);

                AddToTree(root, callNumber, description);
            }

            return root;
        }

        private void AddToTree(TreeNode parent, string callNumber, string description)
        {
            var current = parent;
            var callNumbers = callNumber.Split('.');

            foreach (var cn in callNumbers)
            {
                if (!current.Children.TryGetValue(cn, out var child))
                {
                    child = current.AddChild(cn, "");
                }

                current = child;
            }

            // Update the description at the leaf node
            current.Description = description;
        }
    }
    public class TreeNode
    {
        public string CallNumber { get; }
        public string Description { get; set; }
        public Dictionary<string, TreeNode> Children { get; }

        public TreeNode(string callNumber, string description)
        {
            CallNumber = callNumber;
            Description = description;
            Children = new Dictionary<string, TreeNode>();
        }

        public TreeNode AddChild(string callNumber, string description)
        {
            var child = new TreeNode(callNumber, description);
            Children.Add(callNumber, child);
            return child;
        }



    }


}

