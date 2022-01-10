
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Configuration;


namespace Braincase.GanttChart
{
   

    public interface Imyinterface
    {
        void SetTask(MyTask myTask);
    }

    public partial class BM_Project : Form
    {
        public event EventHandler<TaskMouseEventArgs> TaskMouseClick = null;
        public string targetPath = ConfigurationManager.AppSettings["DBPath"];
        public string photo1 = ConfigurationManager.AppSettings["Photo1"];
        public string photo2 = ConfigurationManager.AppSettings["Photo2"];
        public string site1 = ConfigurationManager.AppSettings["Site1"];
        public string site2 = ConfigurationManager.AppSettings["Site2"];
        
        //OverlayPainter _mOverlay = new OverlayPainter();

        public ProjectManager _mManager = null;
        public ProjectManager _mManager2 = null;
        public DateTime startTime = DateTime.Today;

        public BM_Project()
        {
            InitializeComponent();

            Image img1 = Image.FromFile(@photo1);
            Image img2 = Image.FromFile(@photo2);

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            pictureBox1.Image = img1;
            pictureBox2.Image = img2;

            _mManager = new ProjectManager();
            
            
            var program = new MyTask(_mManager) { Name = "ManagerProgram" };
            var plan = new MyTask(_mManager) { Name = "Plan" };
            var make = new MyTask(_mManager) { Name = "Coding" };
            var test = new MyTask(_mManager) { Name = "Program Test" };

            _mManager.Add(program);
            _mManager.Add(plan);
            _mManager.Add(make);
            _mManager.Add(test);

            // Create another 1000 tasks for stress testing
            /*
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                 var task = new MyTask(_mManager) { Name = string.Format("New Task {0}", i.ToString()) };
                 _mManager.Add(task);
                 //_mManager.SetStart(task, TimeSpan.FromDays(rand.Next(300)));
                 _mManager.SetDuration(task, TimeSpan.FromDays(rand.Next(50)));
            }
            */

            _mManager.SetStart(make, TimeSpan.FromDays(3));
            _mManager.SetStart(test, TimeSpan.FromDays(8));

            _mManager.SetDuration(plan, TimeSpan.FromDays(4));
            _mManager.SetDuration(make, TimeSpan.FromDays(6));
            _mManager.SetDuration(test, TimeSpan.FromDays(5));

            _mManager.Group(program, plan);
            _mManager.Group(program, make);
            _mManager.Group(program, test);

            var test2 = new MyResource() { Name = "김병수" };
            _mManager.Assign(plan, test2);
            
            
            _mChart.CreateTaskDelegate = delegate () { return new MyTask(_mManager); };

            _mChart.TaskMouseOver += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOver);
            _mChart.TaskMouseOut += new EventHandler<TaskMouseEventArgs>(_mChart_TaskMouseOut);
            _mChart.TaskSelected += new EventHandler<TaskMouseEventArgs>(_mChart_TaskSelected);
            //_mChart.PaintOverlay += _mOverlay.ChartOverlayPainter;
            _mChart.AllowTaskDragDrop = true;

            // Set Time information
            var span = DateTime.Today - _mManager.Start;
            _mManager.Now = span; // set the "Now" marker at the correct date
            _mChart.TimeResolution = TimeResolution.Day; // Set the chart to display in days in header
            
            // Init the rest of the UI
            _InitProjectUI();
        }

        private void BM_Project_Load(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            timer1.Start();
        }
        private void _InitProjectUI()
        {
            _mChart.Init(_mManager);
            int binddata = _mManager.Tasks.Count();
            if (binddata <= 0)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Columns[3].Visible = false;
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;
            }
            else
            {
                dataGridView1.DataSource = new BindingSource(_mManager.Tasks, null);
                dataGridView1.Columns[3].Visible = false;
                dataGridView1.Columns[4].Visible = false;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[7].Visible = false;
                dataGridView1.Columns[8].Visible = false;
            }
            dataGridView1.Refresh();
        }
        private void _InitResourceUI()
        {
            _mChart.Init(_mManager2);
            int binddata = _mManager2.Tasks.Count();
            if (binddata <= 0)
            {
                dataGridView3.Rows.Clear();
                dataGridView3.Columns[3].Visible = false;
                dataGridView3.Columns[4].Visible = false;
                dataGridView3.Columns[5].Visible = false;
                dataGridView3.Columns[7].Visible = false;
                dataGridView3.Columns[8].Visible = false;
            }
            else
            {
                dataGridView3.DataSource = new BindingSource(_mManager2.Tasks, null);
                dataGridView3.Columns[3].Visible = false;
                dataGridView3.Columns[4].Visible = false;
                dataGridView3.Columns[5].Visible = false;
                dataGridView3.Columns[7].Visible = false;
                dataGridView3.Columns[8].Visible = false;
            }
            dataGridView3.Refresh();
        }

        private void _ClearTimeResolutionMenu()
        {
            mnuViewDays.Checked = false;
            mnuViewWeeks.Checked = false;
        }

        public void addProject(MyTask myTask)
        {
            _mManager.Add(myTask);
        }

        private void Project_Add_btn_Click(object sender, EventArgs e)
        {
            Project_Add project_Add = new Project_Add(this);
            project_Add.ShowDialog();
            _mChart.Invalidate();
            _InitProjectUI();
        }

        private void resource_Mana_Click(object sender, EventArgs e)
        {
            ResourceManagement resourceManagement = new ResourceManagement();
            resourceManagement.ShowDialog();
        }

        private void Schedule_Add_btn_Click(object sender, EventArgs e)
        {
            if(e2 != null) { 
                Schedule_Add schedule_Add = new Schedule_Add(this, e2);
                schedule_Add.ShowDialog();
                _mChart.Invalidate();
            }
            _InitProjectUI();
        }

        private void Resource_Add_btn_Click(object sender, EventArgs e)
        {
            if(e2 != null) { 
                Resource_Add resource_Add = new Resource_Add(this, _mChart ,e2);
                resource_Add.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _mManager = new ProjectManager();
            dataGridView1.Rows.Clear();
            _InitProjectUI();
            _mChart.Init(_mManager);
            _mChart.Invalidate();
        }

        private void instructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_mOverlay.PrintMode = !(instructionsToolStripMenuItem.Checked = !instructionsToolStripMenuItem.Checked);
            _mChart.Invalidate();
        }
        

        
        private void _mTaskGridView_SelectionChanged(object sender, EventArgs e)
        {

        }

        TaskMouseEventArgs e2;

        void _mChart_TaskSelected(object sender, TaskMouseEventArgs e)
        {
            TaskMouseClick?.Invoke(e2, e);
            e2 = e;
            //bool ch1 = _mManager.IsGroup(e.Task);
            //bool ch2 = _mManager.IsMember(e.Task);
            //Console.WriteLine("Group : " + ch1);
            //Console.WriteLine("Member : " + ch2);
        }

        private void mHour_btn_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Hour;
            _ClearTimeResolutionMenu();
            _mChart.Invalidate();
        }

        private void mDay_btn_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Day;
            _ClearTimeResolutionMenu();
            mnuViewDays.Checked = true;
            _mChart.Invalidate();
        }

        private void mWeek_btn_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Week;
            _ClearTimeResolutionMenu();
            mnuViewWeeks.Checked = true;
            _mChart.Invalidate();
        }

        private void mnuViewHours_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Hour;
            _ClearTimeResolutionMenu();
            _mChart.Invalidate();
        }

        private void mnuViewDays_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Day;
            _ClearTimeResolutionMenu();
            mnuViewDays.Checked = true;
            _mChart.Invalidate();
        }

        private void mnuViewWeeks_Click(object sender, EventArgs e)
        {
            _mChart.TimeResolution = TimeResolution.Week;
            _ClearTimeResolutionMenu();
            mnuViewWeeks.Checked = true;
            _mChart.Invalidate();
        }

        private void Project_Del_btn_Click(object sender, EventArgs e)
        {
            if(e2 != null) _mManager.Delete(e2.Task);
            _mChart.Invalidate();
            _InitProjectUI();
        }

        void _mChart_TaskMouseOut(object sender, TaskMouseEventArgs e)
        {
            _mChart.Invalidate();
        }

        void _mChart_TaskMouseOver(object sender, TaskMouseEventArgs e)
        {
            _mChart.Invalidate();
        }

        private void Schedule_Del_btn_Click(object sender, EventArgs e)
        {
            if(e2 != null) _mManager.Delete(e2.Task);
            _mChart.Invalidate();
            _InitProjectUI();
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fs = System.IO.File.OpenRead(dialog.FileName))
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        _mManager = bf.Deserialize(fs) as ProjectManager;
                        if (_mManager == null)
                        {
                            MessageBox.Show("Unable to load ProjectManager. Data structure might have changed from previous verions", "Gantt Chart", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            _mChart.Init(_mManager);
                            _mChart.Invalidate();
                        }
                    }
                }
            }
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fs = System.IO.File.OpenWrite(dialog.FileName))
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        bf.Serialize(fs, _mManager);
                    }
                }
            }
        }

        private void Resource_Del_btn_Click(object sender, EventArgs e)
        {
            _mManager.Unassign(e2.Task);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Visible == true)
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = true;
            }

            else if (pictureBox2.Visible == true)
            {
                pictureBox2.Visible = false;
                pictureBox1.Visible = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(site1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(site2);
        }

        private void ReSource_View()
        {
            _mChart.Visible = false;
            _mChart2.Visible = true;

            _mManager2 = new ProjectManager();

            DataConn conn = new DataConn();
            DataSet ds;
            string DB_path = targetPath;

            string sql = @"SELECT manager_name from manager";

            ds = conn.GetDataset(sql, DB_path);

            if (ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    string st = r["manager_name"].ToString();
                    Console.WriteLine(st);
                    var newP = new MyTask(_mManager2) { Name = st };
                    _mManager2.Add(newP);
                    string sss = null;

                    for (int i = 0; i < _mManager.Tasks.Count(); i++)
                    {

                        var t = _mManager.PopTask(i);
                        string aaa = newP.Name;

                        sss = string.Join(", ", _mManager.ResourcesOf(t).Select(x => (x as MyResource).Name));
                        Console.WriteLine(sss);
                        if (aaa.Equals(sss))
                        {
                            _mManager2.Add(t);
                            _mManager2.Group(newP, t);
                        }
                    }
                }
            }
            _mChart2.Init(_mManager2);
            _InitResourceUI();
        }

        private void Project_View()
        {
            _mChart.Visible = true;
            _mChart2.Visible = false;
            _InitProjectUI();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(tabControl1.SelectedIndex);
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                {
                        Project_View();
                        break;
                }
                case 1:
                {
                        ReSource_View();
                        break;
                }
            }
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            _mChart.MoveScrollY(e.NewValue);
        }

        private void _mChart_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void dataGridView3_Scroll(object sender, ScrollEventArgs e)
        {
            _mChart2.MoveScrollY(e.NewValue);
        }

        private void _mChart2_Scroll(object sender, ScrollEventArgs e)
        {
            dataGridView3.HorizontalScrollingOffset = e.NewValue;
            _mChart2.MoveScrollY(e.NewValue);
            Console.WriteLine((int)_mChart2.MoveScrolY2());
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var task = dataGridView1.SelectedRows[0].DataBoundItem as Task;
                _mChart.Invalidate();
            }
        }
    }

    class DataConn
    {
        public DataSet GetDataset(string sql, string DB_path)
        {
            string connStr = @"Provider = Microsoft.Ace.OleDb.12.0; Data Source =" + DB_path + ";";

            OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connStr);
            DataSet ds = new DataSet();
            OleDbDataAdapter adp = new OleDbDataAdapter(sql, conn);
            adp.Fill(ds);
            return ds;

        }
    }
   

}
