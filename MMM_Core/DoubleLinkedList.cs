public class DoubleLinkedList<T> : IEnumerable<T>
{
	public class ListNode
	{
		public T Data;
		public ListNode? Next { get; set; }
		public ListNode? Prev { get; set; }

		public ListNode(T data)
		{
			Data = data;
			Next = null;
			Prev = null;
		}
	}

	protected ListNode? head;
	protected ListNode? tail;
	protected int count;

	public int Count => count;

	public void AddFirst(T data)
	{
		ListNode newNode = new ListNode(data);
		if (head == null)
		{
			head = tail = newNode;
			newNode.Next = newNode;
			newNode.Prev = newNode;
		}
		else
		{
			newNode.Next = head;
			newNode.Prev = tail;
			head.Prev = newNode;
			tail.Next = newNode;
			head = newNode;
		}
		count++;
	}

	public void AddLast(T data)
	{
		ListNode newNode = new ListNode(data);
		if (tail == null)
		{
			head = tail = newNode;
			newNode.Next = newNode;
			newNode.Prev = newNode;
		}
		else
		{
			newNode.Next = head;
			newNode.Prev = tail;
			head.Prev = newNode;
			tail.Next = newNode;
			tail = newNode;
		}
		count++;
	}



	public bool Remove(T data)
	{
		ListNode? current = head;
		while (current != null)
		{
			if (current.Data!.Equals(data))
			{
				if (current.Prev != null)
				{
					current.Prev.Next = current.Next;
				}
				else
				{
					head = current.Next;
				}

				if (current.Next != null)
				{
					current.Next.Prev = current.Prev;
				}
				else
				{
					tail = current.Prev;
				}

				count--;
				return true;
			}
			current = current.Next;
		}
		return false;
	}

	public bool Contains(T data)
	{
		ListNode? current = head;
		while (current != null)
		{
			if (current.Data!.Equals(data))
			{
				return true;
			}
			current = current.Next;
		}
		return false;
	}

	public void Clear()
	{
		head = tail = null;
		count = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		ListNode? current = head;
		while (current != null)
		{
			yield return current.Data;
			current = current?.Next;
			if (current == tail) break; // Prevent infinite loop in case of circular list
		}
	}

	public IEnumerable<T> GetReverseEnumerator()
	{
		ListNode? current = tail;
		while (current != null)
		{
			yield return current.Data;
			current = current.Prev;
			if (current == head) break; // Prevent infinite loop in case of circular list
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	public void InsertAfter(ListNode node, T data)
	{
		if (node == null)
		{
			throw new ArgumentNullException(nameof(node));
		}

		ListNode newNode = new ListNode(data);
		newNode.Prev = node;
		newNode.Next = node.Next;

		if (node.Next != null)
		{
			node.Next.Prev = newNode;
		}
		else
		{
			tail = newNode;
		}

		node.Next = newNode;
		count++;
	}
}
