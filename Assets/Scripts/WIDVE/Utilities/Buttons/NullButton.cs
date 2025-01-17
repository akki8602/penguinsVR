﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	[CreateAssetMenu(fileName = nameof(NullButton), menuName = nameof(Button) + "/" + nameof(NullButton), order = B_ORDER)]
	public class NullButton : ButtonFloat
    {
		public override float GetRawValue()
		{
			return 0;
		}
	}
}