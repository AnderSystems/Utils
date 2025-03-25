using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeObjects : MonoBehaviour
{
    public bool Randomize;
    public int index;
    public List<GameObject> objects = new List<GameObject>();

    public void DisableAll()
    {
        foreach (var item in objects)
        {
            item.gameObject.SetActive(false);
        }
    }
    public void SelectIndex(int index)
    {
        DisableAll();
        objects[index].gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        SelectIndex(index);
    }
    private void OnValidate()
    {
        if (Randomize)
        {
            index = (Random.Range(0, objects.Count - 1));
        }
        SelectIndex(index);
    }
}
