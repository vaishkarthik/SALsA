﻿using System;
using System.Collections.Generic;
using System.Drawing;
using ZedGraph;

namespace LivesiteAutomation.Commons
{
    public class ResourceUsage
    {
        private ZedGraphControl control = new ZedGraphControl();
        public ResourceUsage()
        {
            var area = control.GraphPane;
            area.Title.Text = "Resource Usage";
            area.XAxis.Type = AxisType.DateAsOrdinal;
            area.XAxis.MajorGrid.IsVisible = true;
            area.YAxis.MajorGrid.IsVisible = true;
            area.XAxis.Scale.Format = "yyyy-MM-dd hh:mm";
            area.XAxis.Title.Text = "Time";
            area.YAxis.Title.Text = "%";
        }

        public void AddArea(string name, List<KeyValuePair<DateTime, double>> kvps, Color color)
        {
            var ppl = new PointPairList();
            foreach (KeyValuePair<DateTime, double> kvp in kvps)
            {
                ppl.Add(kvp.Key.ToOADate(), kvp.Value);
            }
            control.GraphPane.AddCurve(name, ppl, color);
        }

        public Bitmap GenerateGraph(int width = 768, int heigth = 576, float dpi = 120F)
        {
            control.AxisChange();
            control.Invalidate();
            return control.GraphPane.GetImage(width, heigth, dpi, true);
        }
    }
}