using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace LineChart
{
    public partial class Form1 : Form
    {
        private string csvPath = @"C:\Users\trade\Documents\_Send_To_Mac\VwapStats.csv";
        private string daysWonMessage = "No Message";
        private bool DailyGoal = false;
        private bool MultiDailyGoal = false;
        private int[] sumDay = new int[] { 0, 0, 0, 0, 0 };

        struct Trade
        {
            public DateTime DateValue;
            public bool IsLong;
            public bool InsideIB;
            public int Gain;
           
            public Trade(DateTime dateValue, bool isLong, bool insideIB, int gain)
            {
                DateValue = dateValue;
                IsLong = isLong;
                InsideIB = insideIB;
                Gain = gain;
            }
        }

        List<Trade> tradeList = new List<Trade>();

        public Form1()
        {
            InitializeComponent();
            ReadCVS();
            
            ConvertDataToChart(dailyGoal: DailyGoal);
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 7, end1: 9);
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 9, end1: 12);
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 12, end1: 15);

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void ReadCVS()
        { 
            StreamReader reader = new StreamReader(File.OpenRead(csvPath));
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split(',');
                    if (values.Length >= 3)
                    { 
                        // 1/10/2020 2:52:26 PM,  Long,  75
                        DateTime dateValue = ConvertStringFrom(date: values[0], debug: false);
                        bool IsLong = false;
                        bool InsideIB = false;
                        if (values[1] == " Long") { IsLong = true; }
                        int gain = 0;
                        Int32.TryParse(values[2], out gain);
                        if (values[3] == " True")
                        {
                            InsideIB = true;
                        } 
                        Trade trade = new Trade(dateValue, IsLong, InsideIB, gain);
                        tradeList.Add(trade);
                    }
                }
            }
        }

        private void ConvertDataToChart(bool dailyGoal)
        {
            List<int> profitList = new List<int>();
            List<DateTime> dateList = new List<DateTime>();
            int total = 0;
            DateTime lastDate = DateTime.Today;
            int lastProfit = 0;
            int i = 0;
            int dailyWin = 0;
            int daysWon = 0;
            int dayCount = 0;
            int sumLong = 0;
            int sumShort = 0;
            int sumInsideIB = 0;
            int sumOutsideIB = 0;

            Array.Clear(sumDay, 0, 4);

            foreach (var trade in tradeList)
            {                 
                //Console.WriteLine(trade.DateValue + ", IsLong: " + trade.IsLong + ", Inside: " + trade.InsideIB + ", " + trade.Gain);
                // only add if new date
                if (!IsTheSameDay(date1: trade.DateValue, date2: lastDate ) && i > 0) {
                    dayCount += 1;
                    profitList.Add(lastProfit);
                    dateList.Add(lastDate);
                    if (dailyWin >= 0 )
                    {
                        daysWon += 1;
                    }
                    dailyWin = 0;
                }

                if ( dailyGoal && dailyWin >= 75 )
                {
                    // we halt logging trades for the day
                } else {
                    dailyWin += trade.Gain;
                    total += trade.Gain;
                    lastDate = trade.DateValue;
                    lastProfit = total;
                    if (trade.IsLong) {
                        sumLong += trade.Gain; } else {
                        sumShort += trade.Gain;
                    }
                    SumDaysOfWeek(fromDate: trade.DateValue, gain: trade.Gain);
                    if (trade.InsideIB) {
                        sumInsideIB += trade.Gain;
                    } else {
                        sumOutsideIB += trade.Gain; 
                    }
                } 
                i++;
            }
            double pctWIn = ((double)daysWon / (double)dayCount) * 100;
            var pctWInStr = String.Format("{0:0.0}", pctWIn);
            daysWonMessage = pctWInStr + "% winning days";
            SingleLineChart(name: "All Trades", dates: dateList, entries: profitList);

            string days3 =  String.Join(", ", sumDay);
            string textBoxMessage = daysWonMessage + "\nSum Long " + sumLong + ", Sum Short: " 
                + sumShort + "\nDays " + days3 + "\nSum Inside: " + sumInsideIB + " Sum Out: " + sumOutsideIB;
            richTextBox1.Text = textBoxMessage;
        }

        private void SumDaysOfWeek(DateTime fromDate, int gain)
        {
            
            switch (fromDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    
                    break;
                case DayOfWeek.Monday:
                    sumDay[0] += gain;
                    break;
                case DayOfWeek.Tuesday:
                    sumDay[1] += gain;
                    break;
                case DayOfWeek.Wednesday:
                    sumDay[2] += gain;
                    break;
                case DayOfWeek.Thursday:
                    sumDay[3] += gain;
                    break;
                case DayOfWeek.Friday:
                    sumDay[4] += gain;
                    break;
                case DayOfWeek.Saturday:
                    break;
                default:
                    break;
            }
        }

        private void ConvertHoursToChart(bool dailyGoal, int start1, int end1)
        {

            List<int> profitList = new List<int>();
            List<DateTime> dateList = new List<DateTime>();
            int total = 0;
            DateTime lastDate = DateTime.Today;
            int lastProfit = 0;
            int i = 0;
            int dailyWin = 0;
            int daysWon = 0;
            int dayCount = 0;
            int sumLong = 0;
            int sumShort = 0;
            int sumInsideIB = 0;
            int sumOutsideIB = 0;
            Array.Clear(sumDay, 0, 4);

            foreach (var trade in tradeList)
            {
                //Console.WriteLine(trade.DateValue + ", IsLong: " + trade.IsLong + ", " + trade.Gain);
                // only add if new date
                if (trade.DateValue.TimeOfDay > new TimeSpan(start1, 00, 00) 
                    && trade.DateValue.TimeOfDay < new TimeSpan(end1, 00, 00))
                {
                    //match found
                    if (!IsTheSameDay(date1: trade.DateValue, date2: lastDate) && i > 0)
                    {
                        dayCount += 1;
                        profitList.Add(lastProfit);
                        dateList.Add(lastDate);
                        if (dailyWin >= 0)
                        {
                            daysWon += 1;
                        }
                        dailyWin = 0;
                    }

                    if (dailyGoal && dailyWin >= 75)
                    {
                        // we halt logging trades for the day
                    }
                    else
                    {
                        dailyWin += trade.Gain;
                        total += trade.Gain;
                        lastDate = trade.DateValue;
                        lastProfit = total;
                        if (trade.IsLong)
                        {
                            sumLong += trade.Gain;
                        }
                        else
                        {
                            sumShort += trade.Gain;
                        }

                        SumDaysOfWeek(fromDate: trade.DateValue, gain: trade.Gain);
                        if (trade.InsideIB)
                        {
                            sumInsideIB += trade.Gain;
                        }
                        else
                        {
                            sumOutsideIB += trade.Gain;
                        }
                    }
                    
                }
                i++;
            }
            double pctWIn = ((double)daysWon / (double)dayCount) * 100;
            var pctWInStr = String.Format("{0:0.0}", pctWIn);
            daysWonMessage = pctWInStr + "% winning days";
            string thisName = start1 + "-" + end1;
            MultiLineChart(name: thisName, dates: dateList, entries: profitList);
            if (thisName == "9-12" )
            {
                string days3 = String.Join(", ", sumDay);
                string textBoxMessage = daysWonMessage + "\nSum Long " + sumLong + ", Sum Short: " 
                    + sumShort + "\nDays " + days3 + "\nSum Inside: " + sumInsideIB + " Sum Out: " + sumOutsideIB;
                richTextBox2.Text = textBoxMessage;
            }
            
        }

        private bool IsTheSameDay(DateTime date1, DateTime date2)
        {
            return (date1.Year == date2.Year && date1.DayOfYear == date2.DayOfYear);
        }

        private DateTime ConvertStringFrom(string date, bool debug)
        {
            DateTime dateValue;
            if (DateTime.TryParse(date, out dateValue))
                if (debug ) Console.WriteLine("Converted '{0}' to {1}.", date, dateValue);
            else
                if (debug) Console.WriteLine("Unable to convert '{0}' to a date.", date);
            return dateValue;

        }

        private void SingleLineChart(string name, List<DateTime> dates, List<int> entries)
        {
            this.chart1.Series.Clear();
            this.chart1.Titles.Clear();
            //this.chart1.Titles.Add(daysWonMessage); 
            this.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{$0,000}";
            Series series = this.chart1.Series.Add(name);
            series.ChartType = SeriesChartType.Line;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            this.chart1.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.White;
            this.chart1.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.White;
            this.chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            this.chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            this.chart1.ChartAreas[0].AxisY.LineColor = Color.White;
            this.chart1.ChartAreas[0].AxisX.LineColor = Color.White;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            this.chart1.ChartAreas[0].BackColor = Color.DimGray;
            this.chart1.Series[name].BorderWidth = 3;
            int i = -1;
            foreach (var entry in entries)
            {
                i++;
                series.Points.AddXY(dates[i], entry);
            }
        }

        private void MultiLineChart(string name, List<DateTime> dates, List<int> entries)
        { 
            Series series1 = new Series(); 
            series1.ChartType = SeriesChartType.Line;
            series1.Name = name;
            chart2.Series.Add(series1);
            chart2.ChartAreas[0].AxisY.LabelStyle.Format = "{$0,000}";
            this.chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false; 
            this.chart2.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            this.chart2.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            this.chart2.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.White;
            this.chart2.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.White;
            this.chart2.ChartAreas[0].AxisY.LineColor = Color.White;
            this.chart2.ChartAreas[0].AxisX.LineColor = Color.White;
            this.chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            this.chart2.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash; 
            this.chart2.ChartAreas[0].BackColor = Color.DimGray;
            this.chart2.Series[name].BorderWidth = 3;
            int i = -1;
            foreach (var entry in entries)
            {
                i++;
                chart2.Series[name].Points.AddXY(dates[i], entry); 
            }

        }
         
        public void BarExample()
        {
            this.chart1.Series.Clear();

            // Data arrays
            string[] seriesArray = { "Cat", "Dog", "Bird", "Monkey" };
            int[] pointsArray = { 2, 1, 7, 5 };

            // Set palette
            this.chart1.Palette = ChartColorPalette.EarthTones;

            // Set title
            this.chart1.Titles.Add("Animals");

            // Add series.
            for (int i = 0; i < seriesArray.Length; i++)
            {
                Series series = this.chart1.Series.Add(seriesArray[i]);
                series.Points.Add(pointsArray[i]);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DailyGoal = !DailyGoal;
            Console.WriteLine("DailyGoal is " + DailyGoal);
            ConvertDataToChart(dailyGoal: DailyGoal);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            MultiDailyGoal = !MultiDailyGoal;
            this.chart2.Series.Clear();
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 7, end1: 9);
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 9, end1: 12);
            ConvertHoursToChart(dailyGoal: MultiDailyGoal, start1: 12, end1: 15);
        }
    }
}
