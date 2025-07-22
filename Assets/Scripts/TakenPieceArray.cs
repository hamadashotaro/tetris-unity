using UnityEngine;

public class TakenPieceArray
{
    private int[] arr;
    private int size;
    private int currentIndex;

    public TakenPieceArray()
    {
        arr = new int[7];
        size = 0;
        currentIndex = 0;
    }

    public void Add(int value)
    {
        arr[currentIndex] = value;
        currentIndex++;
        size++;
    }

    public bool Contains(int value)
    {
        for (int i = 0; i < size; i++)
        {
            if (arr[i] == value)
                return true;
        }

        return false;
    }

    public void Clear()
    {
        arr = new int[7];
        size = 0;
        currentIndex = 0;
    }

    public int Count()
    {
        return size;
    }
    
    public override string ToString()
    {
        string result = "{";
        for (int i = 0; i < size; i++)
        {
            result += arr[i] + ", ";
        }
        result += "}";
        return result;
    }
}
