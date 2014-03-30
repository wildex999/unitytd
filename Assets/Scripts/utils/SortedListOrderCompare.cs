using System;
using System.Collections.Generic;

//Compares the order of two nodes

public class SortedListOrderCompare : IComparer<GUIWidget>
{
    public int Compare(GUIWidget w1, GUIWidget w2)
    {
        if (w1.getOrder() < w2.getOrder())
            return -1;
        else if (w1.getOrder() == w2.getOrder())
        {
            if (w1 == w2)
                return 0;
            //Can not return 0 for SortedList, as that would cause an exception
            //So we must sort them somehow
            if (w1.GetInstanceID() > w2.GetInstanceID())
                return 1;
            else return -1;
        }
        else
            return 1;

    }
}