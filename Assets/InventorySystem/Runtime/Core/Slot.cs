using System;

public class Slot<T>
{
    public virtual T Item => item;
    public virtual bool IsEmpty => item == null;

    protected T item = default;
    protected Predicate<T> filterPredicate;

    public Slot(Predicate<T> filterPredicate = null)
    {
        this.filterPredicate = filterPredicate ?? (_ => true);
    }

    public virtual bool CanAcceptItem(T candidate)
    {
        return IsItemAllowed(candidate) && (IsEmpty || Equals(item, candidate));
    }

    public virtual bool IsItemAllowed(T candidate)
    {
        return filterPredicate(candidate);
    }

    public virtual void UpdateSlot(T newItem)
    {
        item = newItem;
    }
}
