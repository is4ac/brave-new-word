/**
 * Node object for Trie class.
 * 
 * Code retrieved from https://visualstudiomagazine.com/articles/2015/10/20/text-pattern-search-trie-class-net.aspx
 */

/*
using System;
using System.Collections.Generic;

public class Node
{
	public char Value { get; set; }
	public List<Node> Children { get; set; }
	public Node Parent { get; set; }
	public int Depth { get; set; }

	public Node(char value, int depth, Node parent)
	{
		Value = value;
		Children = new List<Node>();
		Depth = depth;
		Parent = parent;
	}

	public bool IsLeaf()
	{
		return Children.Count == 0;
	}

	public Node FindChildNode(char c)
	{
		// check to see if any character matches, case insensitive
		foreach (var child in Children)
			if (char.ToUpper(child.Value) == char.ToUpper(c))
				return child;

		return null;
	}

	public void DeleteChildNode(char c)
	{
		for (var i = 0; i < Children.Count; i++)
			if (Children[i].Value == c)
				Children.RemoveAt(i);
	}
}

*/