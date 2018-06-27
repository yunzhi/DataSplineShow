using DevComponents.DotNetBar.Charts;
using DevComponents.DotNetBar.Charts.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataSplineShow
{
    class LenthPosChartControlDisplay
    {
        public static ChartControl LenthPosChartControl;

        public LenthPosChartControlDisplay()
        {
            LenthPosChartControlInit();
        }

        #region LenthPosChartControlInit
        /// <summary>
        /// Initializes the LenthPosChartControlInit.
        /// </summary>
        private void LenthPosChartControlInit()
        {
            LenthPosChartControlDisplay.LenthPosChartControl = new ChartControl();
            LenthPosChartControlDisplay.LenthPosChartControl.Name = "LenthPosChartControl";
            LenthPosChartControlDisplay.LenthPosChartControl.Location = new System.Drawing.Point(10, 15);
            LenthPosChartControlDisplay.LenthPosChartControl.Size = new System.Drawing.Size(680, 500);
            LenthPosChartControlDisplay.LenthPosChartControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LenthPosChartControlDisplay.LenthPosChartControl.MinimumSize = new System.Drawing.Size(480, 320);

            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthPosChartControlDisplay.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthPosChartControlDisplay.LenthPosChartControl.Padding = new System.Windows.Forms.Padding(5);

            LenthPosChartXyInit();
        }
        #endregion

        #region LenthPosChartXyInit

        /// <summary>
        /// Adds a new chart to the ChartControl
        /// </summary>
        private void LenthPosChartXyInit()
        {
            ChartXy LenthPosChartXy = new ChartXy("LenthPosChart");
            LenthPosChartXy.MinContentSize = new Size(480, 320);
            LenthPosChartXy.ChartLineDisplayMode = ChartLineDisplayMode.DisplaySpline;

            // Setup our Crosshair display.
            LenthPosChartXy.ChartCrosshair.HighlightPoints = true;
            LenthPosChartXy.ChartCrosshair.AxisOrientation = AxisOrientation.X;
            LenthPosChartXy.ChartCrosshair.ShowValueXLine = true;
            LenthPosChartXy.ChartCrosshair.ShowValueYLine = true;
            LenthPosChartXy.ChartCrosshair.ShowCrosshairLabels = true;

            // Let's only display the point nearest to the mouse cursor
            // that intersects with the crosshair line.
            LenthPosChartXy.ChartCrosshair.CrosshairLabelMode = CrosshairLabelMode.NearestSeries;
            LenthPosChartXy.ChartCrosshair.CrosshairVisualStyle.Background = new Background(Color.White);

            // Setup various styles for the chart...
            SetupChartStyle(LenthPosChartXy);
            SetupContainerStyle(LenthPosChartXy);
            SetupChartAxes(LenthPosChartXy);
            SetupChartLegend(LenthPosChartXy);

            // Add a chart title and associated series.

            AddChartTitle(LenthPosChartXy);
            InitChartSeries(LenthPosChartXy);

            // And finally, add the chart to the ChartContainers
            // collection of chart elements.

            LenthPosChartControlDisplay.LenthPosChartControl.ChartPanel.ChartContainers.Add(LenthPosChartXy);
        }

        #endregion

        #region SetupChartAxes

        /// <summary>
        /// Sets up the chart axes.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartAxes(ChartXy chartXy)
        {
            // X Axis

            ChartAxis axis = chartXy.AxisX;

            //axis.GridSpacing = 10;
            axis.MinGridInterval = 20;

            axis.AxisMargins = 5;
            axis.AxisFarMargin = 5;
            axis.AxisNearMargin = 5;

            // Set our axis title appropriately.

            axis.Title.Text = "距离（m）";
            axis.Title.ChartTitleVisualStyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4, 0, 4, 0);
            axis.Title.ChartTitleVisualStyle.Font = new Font("微软雅黑", 10);
            axis.Title.ChartTitleVisualStyle.TextColor = Color.Navy;
            axis.Title.ChartTitleVisualStyle.Alignment = Alignment.BottomCenter;

            axis.MinorTickmarks.TickmarkCount = 0;
            axis.MajorTickmarks.StaggerLabels = true;
            axis.MajorTickmarks.LabelSkip = 2;

            axis.MajorGridLines.GridLinesVisualStyle.LineColor = Color.Gainsboro;
            axis.MinorGridLines.GridLinesVisualStyle.LineColor = Color.WhiteSmoke;

            // Set our alternate background to a nice MidnightBlue.

            axis.ChartAxisVisualStyle.AlternateBackground = new Background(Color.FromArgb(20, Color.MidnightBlue));

            axis.UseAlternateBackground = true;

            // Y Axis

            axis = chartXy.AxisY;

            //axis.AxisMargins = 5;
            axis.AxisFarMargin = 30;
            axis.AxisNearMargin = 10;

            axis.GridSpacing = 20;
            axis.MinGridInterval = 50;

            axis.AxisAlignment = AxisAlignment.Far;
            axis.MinorTickmarks.TickmarkCount = 0;

            // Set our axis title appropriately.

            axis.Title.Text = "方位（°）";
            axis.Title.ChartTitleVisualStyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4, 0, 4, 0);
            axis.Title.ChartTitleVisualStyle.Font = new Font("微软雅黑", 10);
            axis.Title.ChartTitleVisualStyle.TextColor = Color.Navy;
            axis.Title.ChartTitleVisualStyle.Alignment = Alignment.MiddleCenter;

            axis.MajorGridLines.GridLinesVisualStyle.LineColor = Color.Gainsboro;
            axis.MinorGridLines.GridLinesVisualStyle.LineColor = Color.WhiteSmoke;

            axis.ChartAxisVisualStyle.AlternateBackground = new Background(Color.FromArgb(30, Color.DarkKhaki));
        }

        #endregion

        #region SetupChartStyle

        /// <summary>
        /// Sets up the chart style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartStyle(ChartXy chartXy)
        {
            ChartXyVisualStyle cstyle = chartXy.ChartVisualStyle;

            cstyle.Background = new Background(Color.White);
            cstyle.BorderThickness = new Thickness(1);
            cstyle.BorderColor = new BorderColor(Color.Navy);

            cstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(6);

            ChartSeriesVisualStyle sstyle = chartXy.ChartSeriesVisualStyle;
            PointMarkerVisualStyle pstyle = sstyle.MarkerHighlightVisualStyle;

            pstyle.Background = new Background(Color.Yellow);
            pstyle.Type = PointMarkerType.Ellipse;
            pstyle.Size = new Size(15, 15);

            CrosshairVisualStyle chstyle = chartXy.ChartCrosshair.CrosshairVisualStyle;

            chstyle.ValueXLineStyle.LineColor = Color.Navy;
            chstyle.ValueXLineStyle.LinePattern = LinePattern.Dot;

            chstyle.ValueYLineStyle.LineColor = Color.Navy;
            chstyle.ValueYLineStyle.LinePattern = LinePattern.Dot;
        }

        #endregion

        #region SetupContainerStyle

        /// <summary>
        /// Sets up the chart's container style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupContainerStyle(ChartXy chartXy)
        {
            ContainerVisualStyle dstyle = chartXy.ContainerVisualStyles.Default;

            dstyle.Background = new Background(Color.White);
            dstyle.BorderColor = new BorderColor(Color.DimGray);
            dstyle.BorderThickness = new Thickness(1);

            dstyle.DropShadow.Enabled = Tbool.True;
            dstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(1);
        }

        #endregion

        #region SetupChartLegend

        /// <summary>
        /// Sets up the Legend style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartLegend(ChartXy chartXy)
        {
            ChartLegend legend = chartXy.Legend;

            legend.ShowCheckBoxes = true;

            legend.Placement = Placement.Inside;
            legend.Alignment = Alignment.TopRight;
            legend.Direction = Direction.LeftToRight;

            // Align vertical items, and permit the legend to only use
            // up to 50% of the available chart width;

            legend.AlignVerticalItems = true;
            legend.MaxHorizontalPct = 50;

            ChartLegendVisualStyle lstyle = legend.ChartLegendVisualStyles.Default;

            lstyle.BorderThickness = new Thickness(0);
            lstyle.BorderColor = new BorderColor(Color.White);

            lstyle.Margin = new DevComponents.DotNetBar.Charts.Style.Padding(8);
            lstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4);

            //lstyle.Background = new Background(Color.FromArgb(200, Color.White));
        }

        #endregion

        #region AddChartTitle

        /// <summary>
        /// Sets up the chart title style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void AddChartTitle(ChartXy chartXy)
        {
            // Add 2 titles for the chart.  They will both be centered and
            // set to automatically wrap if needed.

            // Title 1.

            ChartTitle title = new ChartTitle("距离方位曲线");

            title.XyAlignment = XyAlignment.Top;

            ChartTitleVisualStyle tstyle = title.ChartTitleVisualStyle;

            tstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(8, 8, 8, 0);
            tstyle.Font = new Font("微软雅黑", 16);
            tstyle.TextColor = Color.Navy;
            tstyle.Alignment = Alignment.MiddleCenter;

            chartXy.Titles.Add(title);
        }

        #endregion

        #region InitChartSeries

        /// <summary>
        /// Adds 4 food series to the given chart.
        /// </summary>
        /// <param name="chartXy"></param>
        private void InitChartSeries(ChartXy chartXy)
        {
            ChartSeries DistanceAzimuthChartSeries = new ChartSeries("DistanceAzimuth", SeriesType.Line);
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            DistanceAzimuthChartSeries.SeriesPoints.Clear();
            DistanceAzimuthChartSeries.SeriesPoints.Add(new SeriesPoint(0, 0));
            DistanceAzimuthChartSeries.SeriesPoints.Add(new SeriesPoint(65535/2, 180));
            DistanceAzimuthChartSeries.SeriesPoints.Add(new SeriesPoint(65535, 360));
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            DistanceAzimuthChartSeries.ShowCheckBoxInLegend = true;
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Green;
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            DistanceAzimuthChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            chartXy.ChartSeries.Add(DistanceAzimuthChartSeries);
        }

        #endregion
    }
}
