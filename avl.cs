using System.Xml.Linq;


/// <summary>Represents a generic node for a binary search tree with additional height property.</summary>
/// <param name="key">The key or value stored in the node.</param>
/// <param name="count">Amount of duplicates in the node.</param>
class Node<T>(T key, int count=1)
{
    T _key = key;
    int _count = count;
    public int height = 1;
    public Node<T>? left, right;
    /// <summary>Number of (duplicate) values in the node.</summary>
    public int Count { get => _count; set => _count = value; }
    public T Key { get => _key; set => _key = value; }
}

/// <summary>Represents a generic AVL tree.</summary>
/// <typeparam name="T">Must implement <see cref="IComparable{T}"/> to maintain sort order.</typeparam>
class AVLTree<T>() where T: IComparable<T> 
{
    Node<T>? _root = null;

    /// <summary>Total number of elements in the tree.</summary>
    public int Count { get; private set; } = 0;

    /// <returns>Number of elements with <paramref name="key"/>.</returns>
    public int KeyCount(T key)
    {
        var node = _root;

        while (node != null) 
        {
            int cmp = key.CompareTo(node.Key);

            if (cmp == 0) return node.Count;
            node = cmp < 0 ? node.left : node.right;
        }

        return 0;
    }

    /// <returns>Minimum key according to the <see cref="IComparable{T}"/> implementation.</returns>
    /// <exception cref="InvalidOperationException">When the tree is empty.</exception>
    public T Min() => (_root != null) ? Min(_root).Key : throw new InvalidOperationException("Tree contains no elements.");

    /// <returns>Maximum key according to the <see cref="IComparable{T}"/> implementation.</returns>
    /// <exception cref="InvalidOperationException">When the tree is empty.</exception>
    public T Max() => (_root != null) ? Max(_root).Key : throw new InvalidOperationException("Tree contains no elements.");

    /// <summary>Inserts a <paramref name="number"/> of elements with <paramref name="key"/> into the tree.</summary>
    public void Insert(T key, int number=1)
    {
        _root = Insert(_root, key, number);
        Count += number;
    }

    /// <summary>Removes all elements with <paramref name="key"/> from the tree if all=true; one otherwise.</summary>
    public void Delete(T key, bool all=false)
    {
        (_root, int diff) = Delete(_root, key, all);
        Count -= diff;
    }

    /// <summary>Checks if a tree implemented with <see cref="Node{T}"/> at <paramref name="root"/> is an AVL tree.</summary>
    public static bool IsAVL(Node<T>? root) => PropertyCheck(root, default!, false, default!, false).valid;

    /// <summary>Checks if the current instance of <see cref="AVLTree{T}"/> is a valid AVL tree.</summary>
    public bool IsAVL() => IsAVL(_root);

    // Private helper methods:

    /// <returns>The set height of a <paramref name="node"/> if not null; otherwise, 0.</returns>
    static int NodeHeight(Node<T>? node) => (node != null) ? node.height : 0;

    /// <returns>Calculated height of a <paramref name="node"/> based off its subtrees.</returns>
    static int CalculateHeight(Node<T> node) => 1 + Math.Max(NodeHeight(node.left), NodeHeight(node.right));

    /// <returns>New root of the tree at <paramref name="node"/> after left-rotation.</returns>
    /// <exception cref="InvalidDataException">When passing a <see cref="Node{T}"/> with an empty right subtree.</exception>
    static Node<T> LeftRotate(Node<T> node) 
    {
        var oldRight = node.right ?? throw new InvalidDataException("Cannot left-rotate a node with an empty right subtree.");
        var newRight = oldRight.left;

        oldRight.left = node;
        node.right = newRight;

        node.height = CalculateHeight(node);
        oldRight.height = CalculateHeight(oldRight);

        return oldRight;
    }

    /// <returns>New root of the tree at <paramref name="node"/> after right-rotation.</returns>
    /// <exception cref="InvalidDataException">When passing a <see cref="Node{T}"/> with an empty left subtree.</exception>
    static Node<T> RightRotate(Node<T> node) {
        var oldLeft = node.left ?? throw new InvalidDataException("Cannot right-rotate a node with an empty left subtree.");
        var newLeft = oldLeft.right;

        oldLeft.right = node;
        node.left = newLeft;

        node.height = CalculateHeight(node);
        oldLeft.height = CalculateHeight(oldLeft);

        return oldLeft;
    }

    /// <returns>The balance factor of a <paramref name="node"/> if not null; otherwise, 0.</returns>
    static int BalanceFactor(Node<T>? node) => (node != null) ? (NodeHeight(node.left) - NodeHeight(node.right)) : 0;

    /// <returns>New root of the tree at <paramref name="node"/> after balancing.</returns>
    static Node<T> BalanceNode(Node<T> node)
    {
        int bf = BalanceFactor(node);

        if (bf > 1)
        {
            if (BalanceFactor(node.left) < 0) node.left = LeftRotate(node.left!);
            return RightRotate(node);
        }
        else if (bf < -1)
        {
            if (BalanceFactor(node.right) > 0) node.right = RightRotate(node.right!);
            return LeftRotate(node);
        }
        else return node;
    }

    /// <returns>New root of the tree at <paramref name="node"/> after inserting a <paramref name="key"/> with <paramref name="count"/>.</returns>
    /// <exception cref="InvalidDataException">When passing a negative number into <paramref name="count"/>.</exception>
    static Node<T>? Insert(Node<T>? node, T key, int count)
    {
        if (count == 0) return node;
        if (count < 0) throw new InvalidDataException("Cannot add a negative number of elements to the tree.");
        if (node == null) return new Node<T>(key, count);

        int cmp = key.CompareTo(node.Key);

        if (cmp < 0) node.left = Insert(node.left, key, count);
        else if (cmp > 0) node.right = Insert(node.right, key, count);
        else
        {
            node.Count += count;
            return node;
        }

        node.height = CalculateHeight(node);
        return BalanceNode(node);
    }

    /// <returns>New root of the tree at <paramref name="node"/> after deleting a <paramref name="key"/> and the amount of removed (duplicate) keys; 0 if nothing was removed.</returns>
    /// <param name="all">Deletes all occurrences of the <paramref name="key"/> (the entire node) if true; one otherwise.</param>
    static (Node<T>? root, int diff) Delete(Node<T>? node, T key, bool all=false)
    {
        int diff = 0;
        if (node == null) return (node, diff);

        int cmp = key.CompareTo(node.Key);

        if (cmp < 0) (node.left, diff) = Delete(node.left, key, all);
        else if (cmp > 0) (node.right, diff) = Delete(node.right, key, all);
        else
        {
            if (node.Count > 1 && !all)
            {
                node.Count--;
                return (node, 1);
            }

            diff = node.Count;

            if (node.left == null) return (node.right, diff);
            else if (node.right == null) return (node.left, diff);

            var replacement = Min(node.right);
            node.Key = replacement.Key;
            node.Count = replacement.Count;
            (node.right, _) = Delete(node.right, replacement.Key, true);
        }

        node.height = CalculateHeight(node);
        return (BalanceNode(node), diff);
    }

    /// <returns>Leftmost leaf of the tree at <paramref name="node"/>.</returns>
    static Node<T> Min(Node<T> node) => (node.left != null) ? Min(node.left) : node;

    /// <returns>Rightmost leaf of the tree at <paramref name="node"/>.</returns>
    static Node<T> Max(Node<T> node) => (node.right != null) ? Max(node.right) : node;

    /// <summary>Recursively checks AVL properties of the tree at <paramref name="node"/> and its subtrees.</summary>
    /// <returns>Whether the data structure is a valid AVL tree and the height of the tree if valid (-1 otherwise) in a tuple.</returns>
    static (bool valid, int height) PropertyCheck(Node<T>? node, T? min, bool hasMin, T? max, bool hasMax)
    { // had to add hasMin, hasMax because of generics (can't always pass null into min,max)
        if (node == null) return (true, 0);

        bool orderViolation = (hasMin && min!.CompareTo(node.Key) >= 0) || (hasMax && max!.CompareTo(node.Key) <= 0);
        if (orderViolation) return (false, -1);

        var (lValid, lHeight) = PropertyCheck(node.left, min, hasMin, node.Key, true);
        if (!lValid) return (false, -1);

        var (rValid, rHeight) = PropertyCheck(node.right, node.Key, true, max, hasMax);
        if (!rValid) return (false, -1);

        bool heightViolation = (node.left != null && node.left.height != lHeight) || (node.right != null && node.right.height != rHeight);
        if (heightViolation) return (false, -1);

        if (Math.Abs(lHeight - rHeight) > 1) return (false, -1);

        if (node.height != CalculateHeight(node)) return (false, -1);

        return (true, node.height);
    }
}