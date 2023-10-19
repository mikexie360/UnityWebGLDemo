public class DoublyLinkedList<T>
{
    private T _node;
    private DoublyLinkedList<T> _next;
    private DoublyLinkedList<T> _prev;

    public DoublyLinkedList(T node)
    {
        _node = node;
    }

    public DoublyLinkedList(T node, DoublyLinkedList<T> prev)
    {
        _node = node;
        _prev = prev;
    }

    public DoublyLinkedList(T node, DoublyLinkedList<T> prev, DoublyLinkedList<T> next) 
    {
        _node = node;
        _prev = prev;
        _next = next;
    }

    public T GetNode()
    {
        return _node;
    }

    public DoublyLinkedList<T> GetNext()
    {
        return _next;
    }

    public DoublyLinkedList<T> GetPrev()
    {
        return _prev;
    }

    public void SetNext(DoublyLinkedList<T> next)
    {
        _next = next;
    }

    public void SetPrev(DoublyLinkedList<T> prev)
    {
        _prev = prev;
    }
}