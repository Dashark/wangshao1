using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Collections;
using Microsoft.Win32;
using OxyPlot.Annotations;
using System.Diagnostics;
namespace oxyplotdrawing
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private MainViewModel mvm = null;
    public MainWindow()
    {
      InitializeComponent();
      pressFF.IsEnabled = false;
      pressure.IsEnabled = false;
    }
    //
    private void openfile_Click(object sender, RoutedEventArgs e)//打开数据文件
    {
      OpenFileDialog op = new OpenFileDialog();
      op.InitialDirectory = System.Environment.CurrentDirectory;
      op.RestoreDirectory = true;   //TODO: 什么意思？
      op.DefaultExt = ".txt";
      op.Filter = "文本 文档(*.txt)|*.txt|所有文件(*.*)|*.*";
      if (op.ShowDialog() == false)
      {
        return;  //用户取消
      }
      PTData ptd = PTData.Load(op.FileName);
      //assert(ptd != null);
      if (ptd == null) MessageBox.Show("数据格式错误！");
      mvm = new MainViewModel(ptd);   //组装
      //初始化mvm
      this.DataContext = mvm;
      //new PTDATA
      //new MainViewModel   //add 
      //DataContext
      pressFF.IsEnabled=true;
      pressure.IsEnabled = true;
    }
    // 消息响应，显示作业曲线
    private void pressFF_Click(object sender, RoutedEventArgs e)
    {
      mvm.FFCurve();
    }
    private void pressure_Click(object sender, RoutedEventArgs e)
    {
      mvm.wholeCurve();
    }
  }
  // 窗口的数据模型
  public class MainViewModel
  {
    private PTData ptd = null;  //
    public PlotModel plotModel1 { get; private set; }
    public MainViewModel(PTData ptd)
    {
      // assert(this.ptd != null);
      this.ptd = ptd;
      plotModel1 = new PlotModel();
      plotModel1.Title = "压力曲线";
      var linearAxis1 = new LinearAxis();//横坐标
      linearAxis1.IsAxisVisible = true;
      linearAxis1.Position = AxisPosition.Bottom;
      linearAxis1.Title = "time";
      plotModel1.Axes.Add(linearAxis1);
      var linearAxis2 = new LinearAxis();//纵坐标
      linearAxis2.IsZoomEnabled = false;//纵坐标不随鼠标的变化放大缩小
      linearAxis2.Title = "pressure";
      plotModel1.Axes.Add(linearAxis2);
    }
    public void wholeCurve()
    {      
      drawline(ptd.Time,ptd.Pressure);      
    }
    // 显示高速作业曲线
    public void FFCurve()
    {
      drawline(ptd.TimeFF,ptd.PressureFF);
    }
    // 加载文件--》要数据--》构造对象
    // 参数：file 需要打开的文件
    public void markpoint(double x, double y)
    {
      var mark = new PointAnnotation();//标记点
      mark.X = x;
      mark.Y = y;
      mark.Shape = MarkerType.Square;
      mark.Stroke = OxyColors.DarkBlue;
      mark.StrokeThickness = 1;
      mark.Text = "P";   //TODO: 名称会变？
      plotModel1.Annotations.Add(mark);
    }
    public void drawline(IEnumerator time, IEnumerator pressure)
    {
      plotModel1.Series.Clear();//清空后台画线数据
      plotModel1.Annotations.Clear();//清空后台标记点数据
      var lineSeries1 = new LineSeries();
      double maxX = 0;
      double maxY = 0;
      lineSeries1.Title = "Series 1";
      if (time == null) return;
      IEnumerator tenu = time;
      IEnumerator penu = pressure;
      for (tenu.MoveNext(), penu.MoveNext(); tenu.MoveNext() && penu.MoveNext(); )
      {
        double shuzi1 = Convert.ToDouble(tenu.Current);
        double shuzi2 = Convert.ToDouble(penu.Current);
        if (shuzi2 >= maxY)
        {
          maxY = shuzi2;
          maxX = shuzi1;
        }
        lineSeries1.Points.Add(new DataPoint(shuzi1, shuzi2));
      }
      markpoint(maxX, maxY);       
      plotModel1.Series.Add(lineSeries1);
      plotModel1.InvalidatePlot(true);//刷新屏幕
    }
  }
  public class PTData
  {
    private ArrayList[] myAL = new ArrayList[3];
    private int[] key = new int[20];
    private PTData() { }
    public static PTData Load(string file)
    {
      PTData ret = new PTData();
      FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
      StreamReader m_streamReader = new StreamReader(fs);
      string strLine = m_streamReader.ReadLine();
      strLine = m_streamReader.ReadLine();  //忽略头两行
      ret.myAL[0] = new ArrayList();
      ret.myAL[1] = new ArrayList();
      ret.myAL[2] = new ArrayList();
      string[] split = null;
      int index = 0;
      while (!m_streamReader.EndOfStream)
      {
        strLine = m_streamReader.ReadLine();
        split = strLine.Split(new char[] { ',' });// 按,分割
        ret.myAL[0].Add(split[1]);//时间
        ret.myAL[1].Add(split[2]);//压力
        ret.myAL[2].Add(split[5]);//温度
        if (split[7] == "FF")
        {
          ret.key[0] = index;//'FF'步骤的尾部坐标
          ret.key[1] += 1;//“FF”步骤的长度
        }
        index += 1;  //索引右移
      }
      return ret;
    }
    public IEnumerator Time
    {
      get { return myAL[0].GetEnumerator(); }
      set { }
    }
    public IEnumerator TimeFF
    {
      get { return myAL[0].GetEnumerator(key[0] - key[1], key[1]); }
      set { }
    }
    public IEnumerator Pressure
    {
      get { return myAL[1].GetEnumerator(); }
      set { }
    }
    public IEnumerator PressureFF
    {
      get { return myAL[1].GetEnumerator(key[0] - key[1], key[1]); }
      set { }
    }
  }
}
