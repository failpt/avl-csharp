# Generic AVL-Based BST implementation with allowed duplicates
Written in C#

## Implementation
### Methods
- `public int KeyCount(T key)` – returns the number of elements with a fixed `key`.
- `public T Min()`, `public T Max()` – return the minimum, maximum keys according to the `IComparable<T>` implementation.
- `public void Insert(T key, int number=1)` – adds a `number` of identical `key`'s (stored in one node) to the tree, singular `key` if not specified *(there is an option to pass `0` into the `number`, in that case the function will run in **O(1)** and will not modify the tree in any way)*.
- `public void Delete(T key, bool all=false)` – deletes one `key` from a tree by default, if `all=true` fully deletes the node with all duplicates.

– All run in **O(log(n))**

- `public static bool IsAVL(Node<T>? root)` – Checks if a tree implemented with `Node<T>` at `root` is an AVL tree.
- `public bool IsAVL()` – Checks if the current instance of the class is a valid AVL tree.

– Both run in **O(n)**

### Properties
- `Count` – total number of elements (counting duplicates) in the tree.

## Testing
- **A 100 randomized, property based tests with the help of a dictionary:** each test consists of a random sequence of deletions & insertions and a check. 
Only string and integer lists are generated for randomized testing, but the `static bool RandomizedTest<T>(List<T> input) where T : IComparable<T>` itself allows other types.
- **An `AllCasesTest` test:** tests individual insertion, deletion and rotation cases with predefined integer values.

### Running the tests
Make sure you have the .NET SDK installed, then from the project folder:
```
dotnet run
```
