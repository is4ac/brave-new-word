/**
 * A Trie data structure for quickly searching and validating words in a dictionary.
 * 
 * Code taken in part from https://visualstudiomagazine.com/articles/2015/10/20/text-pattern-search-trie-class-net.aspx
 */

using System;
using System.Collections.Generic;

public class Trie
{
	public Node _root;

	public Trie()
	{
		_root = new Node('^', 0, null);
	}

	// TODO: use Prefix and then subsequent .FindChildNode() calls to step through a Trie
	public Node Prefix(string s)
	{
		var currentNode = _root;
		var result = currentNode;

		foreach (var c in s)
		{
			currentNode = currentNode.FindChildNode(c);
			if (currentNode == null)
				break;
			result = currentNode;
		}

		return result;
	}
		
	public bool Search(string s)
	{
		var prefix = Prefix(s);
		return prefix.Depth == s.Length && prefix.FindChildNode('$') != null;
	}

	public void InsertRange(List<string> items)
	{
		for (int i = 0; i < items.Count; i++)
			Insert(items[i]);
	}

	public void Insert(string s)
	{
		var commonPrefix = Prefix(s);
		var current = commonPrefix;

		for (var i = current.Depth; i < s.Length; i++)
		{
			var newNode = new Node(s[i], current.Depth + 1, current);
			current.Children.Add(newNode);
			current = newNode;
		}

		current.Children.Add(new Node('$', current.Depth + 1, current));
	}

	public void Delete(string s)
	{
		if (Search(s))
		{
			var node = Prefix(s).FindChildNode('$');

			while (node.IsLeaf())
			{
				var parent = node.Parent;
				parent.DeleteChildNode(node.Value);
				node = parent;
			} 
		}
	}
}


