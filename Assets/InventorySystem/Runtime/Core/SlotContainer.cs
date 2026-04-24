using System;
using UnityEngine;

public class SlotContainer<T> : MonoBehaviour
{
    protected Slot<T>[] slots;
    protected int containerSize;

    protected virtual void InitializeContainer(Func<Slot<T>> slotFactory)
    {
        slots = new Slot<T>[containerSize];

        for (int i = 0; i < containerSize; i++)
        {
            slots[i] = slotFactory();
        }
    }

    protected Slot<T> GetFreeSlot()
    {
        foreach (Slot<T> slot in slots)
        {
            if (slot.IsEmpty)
            {
                return slot;
            }
        }

        return null;
    }
}
