﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
    public class ReorderableListTest : MonoBehaviour
    {
        [SerializeField]
        [ReorderableList("int array")]
        int[] Array = new int[0];

        [SerializeField]
        [ReorderableList("int list")]
        List<int> List = new List<int>();
    }
}