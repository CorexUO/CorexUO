using System.Collections;
using System.Drawing;

namespace Server.Engines.Reports
{
	// Modified from MS sample

	//*********************************************************************
	//
	// ChartItem Class
	//
	// This class represents a data point in a chart
	//
	//*********************************************************************

	public class DataItem
	{
		private string _description;
		private float _sweepSize;

		private DataItem() { }

		public DataItem(string label, string desc, float data, float start, float sweep, Color clr)
		{
			Label = label;
			_description = desc;
			Value = data;
			StartPos = start;
			_sweepSize = sweep;
			ItemColor = clr;
		}

		public string Label { get; set; }

		public string Description
		{
			get => _description;
			set => _description = value;
		}

		public float Value { get; set; }

		public Color ItemColor { get; set; }

		public float StartPos { get; set; }

		public float SweepSize
		{
			get => _sweepSize;
			set => _sweepSize = value;
		}
	}

	//*********************************************************************
	//
	// Custom Collection for ChartItems
	//
	//*********************************************************************

	public class ChartItemsCollection : CollectionBase
	{
		public DataItem this[int index]
		{
			get => (DataItem)(List[index]);
			set => List[index] = value;
		}

		public int Add(DataItem value)
		{
			return List.Add(value);
		}

		public int IndexOf(DataItem value)
		{
			return List.IndexOf(value);
		}

		public bool Contains(DataItem value)
		{
			return List.Contains(value);
		}

		public void Remove(DataItem value)
		{
			List.Remove(value);
		}
	}
}
