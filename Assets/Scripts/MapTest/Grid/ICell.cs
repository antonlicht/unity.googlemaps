using UnityEngine;
using System.Collections;

public interface ICell<TContent>
{
    TContent GetContent();
    void SetContent(TContent content);
    void Clear();
}
