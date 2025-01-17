﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.IO
{
	[CreateAssetMenu(fileName = nameof(DataFileTSV), menuName = nameof(DataFile) + "/" + nameof(DataFileTSV), order = WIDVEEditor.C_ORDER)]
	public class DataFileTSV : DataFileCSV
	{
		public override string Extension => "tsv";
		public override char Separator => '\t';

		//everything else is the same as CSV...
	}
}