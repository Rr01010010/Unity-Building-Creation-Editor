using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassNode : MonoBehaviour
{
    public int Index;
    public List<int> Indexes { get => _indexes; set => _indexes = value; }
    [SerializeField]private List<int> _indexes = new List<int>();

}
