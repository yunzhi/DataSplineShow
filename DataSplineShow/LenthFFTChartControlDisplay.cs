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
    class LenthFFTChartControlDisplay
    {
        public static ChartControl LenthFFTChartControl;

        public LenthFFTChartControlDisplay()
        {
            LenthFFTChartControlInit();
        }


        #region LenthFFTChartControlInit
        /// <summary>
        /// Initializes the LenthFFTChartControlInit.
        /// </summary>
        private void LenthFFTChartControlInit()
        {
            LenthFFTChartControlDisplay.LenthFFTChartControl = new ChartControl();
            LenthFFTChartControlDisplay.LenthFFTChartControl.Name = "LenthFFTChartControl";
            LenthFFTChartControlDisplay.LenthFFTChartControl.Location = new System.Drawing.Point(10, 15);
            LenthFFTChartControlDisplay.LenthFFTChartControl.Size = new System.Drawing.Size(750, 500);
            LenthFFTChartControlDisplay.LenthFFTChartControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LenthFFTChartControlDisplay.LenthFFTChartControl.MinimumSize = new System.Drawing.Size(480, 320);

            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            LenthFFTChartControlDisplay.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            LenthFFTChartControlDisplay.LenthFFTChartControl.Padding = new System.Windows.Forms.Padding(5);

            LenthFFTChartXyInit();
        }
        #endregion

        #region LenthFFTChartXyInit

        /// <summary>
        /// Adds a new chart to the ChartControl
        /// </summary>
        private void LenthFFTChartXyInit()
        {
            ChartXy LenthFFTChartXy = new ChartXy("LenthFFTChart");
            LenthFFTChartXy.MinContentSize = new Size(480, 320);
            LenthFFTChartXy.ChartLineDisplayMode = ChartLineDisplayMode.DisplaySpline;

            // Setup our Crosshair display.
            LenthFFTChartXy.ChartCrosshair.HighlightPoints = true;
            LenthFFTChartXy.ChartCrosshair.AxisOrientation = AxisOrientation.X;
            LenthFFTChartXy.ChartCrosshair.ShowValueXLine = true;
            LenthFFTChartXy.ChartCrosshair.ShowValueYLine = true;
            LenthFFTChartXy.ChartCrosshair.ShowCrosshairLabels = true;

            // Let's only display the point nearest to the mouse cursor
            // that intersects with the crosshair line.
            LenthFFTChartXy.ChartCrosshair.CrosshairLabelMode = CrosshairLabelMode.NearestSeries;
            LenthFFTChartXy.ChartCrosshair.CrosshairVisualStyle.Background = new Background(Color.White);

            // Setup various styles for the chart...
            SetupChartStyle(LenthFFTChartXy);
            SetupContainerStyle(LenthFFTChartXy);
            SetupChartAxes(LenthFFTChartXy);
            SetupChartLegend(LenthFFTChartXy);

            // Add a chart title and associated series.

            AddChartTitle(LenthFFTChartXy);
            InitChartSeries(LenthFFTChartXy);

            // And finally, add the chart to the ChartContainers
            // collection of chart elements.

            LenthFFTChartControlDisplay.LenthFFTChartControl.ChartPanel.ChartContainers.Add(LenthFFTChartXy);
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

            axis.Title.Text = "频率/256（频率=300K）";
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

            axis.Title.Text = "距离FFT值";
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

            ChartTitle title = new ChartTitle("距离FFT曲线");
            
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
            ChartSeries lenthFFT1ChartSeries = new ChartSeries("lenthFFT1", SeriesType.Line);
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT1ChartSeries.SeriesPoints.Clear();
            lenthFFT1ChartSeries.SeriesPoints.Add(new SeriesPoint(0, 0));
            lenthFFT1ChartSeries.SeriesPoints.Add(new SeriesPoint(150000, 65535));
            lenthFFT1ChartSeries.SeriesPoints.Add(new SeriesPoint(300000, 0));
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            lenthFFT1ChartSeries.ShowCheckBoxInLegend = true;
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Green;
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            lenthFFT1ChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            lenthFFT1ChartSeries.PointLabelDisplayMode = PointLabelDisplayMode.DataLabels;
            chartXy.ChartSeries.Add(lenthFFT1ChartSeries);

            ChartSeries lenthFFT1AverageChartSeries = new ChartSeries("lenthFFT1Average", SeriesType.Line);
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT1AverageChartSeries.SeriesPoints.Clear();
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            lenthFFT1AverageChartSeries.ShowCheckBoxInLegend = true;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Red;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Red);
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            chartXy.ChartSeries.Add(lenthFFT1AverageChartSeries);

            ChartSeries lenthFFT2ChartSeries = new ChartSeries("lenthFFT2", SeriesType.Line);
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT2ChartSeries.ShowCheckBoxInLegend = true;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerEmptyVisualStyle.Type = PointMarkerType.Star;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Orange;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Orange;
            lenthFFT2ChartSeries.PointLabelDisplayMode = PointLabelDisplayMode.DataLabels;
            chartXy.ChartSeries.Add(lenthFFT2ChartSeries);

            ChartSeries lenthFFT2AverageChartSeries = new ChartSeries("lenthFFT2Average", SeriesType.Line);
            lenthFFT2AverageChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT2AverageChartSeries.ShowCheckBoxInLegend = true;
            lenthFFT2AverageChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Orange;
            chartXy.ChartSeries.Add(lenthFFT2AverageChartSeries);

        }

        #endregion
    }
}
