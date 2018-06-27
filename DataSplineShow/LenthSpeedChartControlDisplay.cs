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
    class LenthSpeedChartControlDisplay
    {
        public static ChartControl LenthSpeedChartControl;

        public LenthSpeedChartControlDisplay()
        {
            LenthSpeedChartControlInit();
        }


        #region LenthSpeedChartControlInit
        /// <summary>
        /// Initializes the LenthSpeedChartControlInit.
        /// </summary>
        private void LenthSpeedChartControlInit()
        {
            LenthSpeedChartControlDisplay.LenthSpeedChartControl = new ChartControl();
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.Name = "LenthSpeedChartControl";
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.Location = new System.Drawing.Point(10, 15);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.Size = new System.Drawing.Size(700, 500);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.MinimumSize = new System.Drawing.Size(480, 320);

            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthSpeedChartControlDisplay.LenthSpeedChartControl.Padding = new System.Windows.Forms.Padding(5);

            LenthSpeedChartXyInit();
        }
        #endregion

        #region LenthSpeedChartXyInit

        /// <summary>
        /// Adds a new chart to the ChartControl
        /// </summary>
        private void LenthSpeedChartXyInit()
        {
            ChartXy LenthSpeedChartXy = new ChartXy();

            LenthSpeedChartXy.Name = "LenthSpeedChart";
            LenthSpeedChartXy.MinContentSize = new Size(480, 320);
            LenthSpeedChartXy.ChartLineDisplayMode = ChartLineDisplayMode.DisplaySpline;

            // Setup our Crosshair display.
            LenthSpeedChartXy.ChartCrosshair.HighlightPoints = true;
            LenthSpeedChartXy.ChartCrosshair.AxisOrientation = AxisOrientation.X;
            LenthSpeedChartXy.ChartCrosshair.ShowValueXLine = true;
            LenthSpeedChartXy.ChartCrosshair.ShowValueYLine = true;
            LenthSpeedChartXy.ChartCrosshair.ShowCrosshairLabels = true;

            // Let's only display the point nearest to the mouse cursor
            // that intersects with the crosshair line.
            LenthSpeedChartXy.ChartCrosshair.CrosshairLabelMode = CrosshairLabelMode.NearestSeries;
            LenthSpeedChartXy.ChartCrosshair.CrosshairVisualStyle.Background = new Background(Color.White);

            // Setup various styles for the chart...
            SetupChartStyle(LenthSpeedChartXy);
            SetupContainerStyle(LenthSpeedChartXy);
            SetupChartAxes(LenthSpeedChartXy);
            SetupChartLegend(LenthSpeedChartXy);

            // Add a chart title and associated series.

            AddChartTitle(LenthSpeedChartXy);
            InitChartSeries(LenthSpeedChartXy);

            // And finally, add the chart to the ChartContainers
            // collection of chart elements.

            LenthSpeedChartControlDisplay.LenthSpeedChartControl.ChartPanel.ChartContainers.Add(LenthSpeedChartXy);
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

            axis.Title.Text = "距离(m)";
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

            axis.Title.Text = "速度（m/s）";
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

            ChartTitle title = new ChartTitle("距离速度曲线");
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
            ChartSeries DistanceSpeedChartSeries = new ChartSeries("DistanceSpeed", SeriesType.Line);
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            DistanceSpeedChartSeries.SeriesPoints.Clear();
            DistanceSpeedChartSeries.SeriesPoints.Add(new SeriesPoint(0, 65535));
            DistanceSpeedChartSeries.SeriesPoints.Add(new SeriesPoint(65535/2, 65535/2));
            DistanceSpeedChartSeries.SeriesPoints.Add(new SeriesPoint(65535, 0));
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            DistanceSpeedChartSeries.ShowCheckBoxInLegend = true;
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Green;
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            DistanceSpeedChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            chartXy.ChartSeries.Add(DistanceSpeedChartSeries);
        }

        #endregion
    }
}
