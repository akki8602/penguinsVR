﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public abstract class CommandLineArgument : ScriptableObject
	{
		public bool Applied { get; protected set; } = false;

		public abstract void Apply();
	}
}