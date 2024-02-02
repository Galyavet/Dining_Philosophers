using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_3
{
    public partial class Form1 : Form
    {
        private const int PHILOSOPHER_COUNT = 5;
        private List<Philosopher> philosophers;
        private List<Thread> philosopherThreads;

        public Form1()
        {
            InitializeComponent();
            philosophers = InitializePhilosophers();
            philosopherThreads = new List<Thread>();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            listBox1.Items.Clear();
            listBox1.Items.Add("Dinner is starting!");

            listView1.Items.Clear();
            for (int i = 0; i < 5; i++)
            {
                ListViewItem listViewItem = new ListViewItem();
                listView1.Items.Add(listViewItem);
                listViewItem.Text = $"{i + 1}";
                listViewItem.SubItems.Add("Thinking");
                listViewItem.SubItems.Add("-");
                listViewItem.SubItems.Add("-");
                listViewItem.SubItems.Add("0");
            }
            foreach (var philosopher in philosophers)
            {
                var philosopherThread = new Thread(new ThreadStart(philosopher.EatAll));
                philosopherThreads.Add(philosopherThread);
                philosopherThread.Start();
            }
        }

        private List<Philosopher> InitializePhilosophers()
        {
            var philosophers = new List<Philosopher>(PHILOSOPHER_COUNT);
            for (int i = 0; i < PHILOSOPHER_COUNT; i++)
            {
                philosophers.Add(new Philosopher(philosophers, i, this));
            }
            foreach (var philosopher in philosophers)
            {
                philosopher.LeftFork = philosopher.LeftPhilosopher.RightFork ?? new Fork();

                philosopher.RightFork = philosopher.RightPhilosopher.LeftFork ?? new Fork();
            }

            return philosophers;
        }
        public void UpdateData(string data)
        {
            listBox1.Items.Add(data);
        }
        public void UpdateListView(string data, int i, int j)
        {
            listView1.Items[i].SubItems[j].Text = data;
        }
        public void UpdateLeftForks(string data, int i)
        {
            listView1.Items[i].SubItems[2].Text = data;
        }
        public void UpdateRightForks(string data, int i)
        {
            listView1.Items[i].SubItems[3].Text = data;
        }

        public void UpdateCount(string data, int i)
        {
            listView1.Items[i].SubItems[4].Text = data;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit(); 
        }
    }

    public class Philosopher
    {
        private Form1 form;
        private int _timesEaten = 0;
        private readonly List<Philosopher> _allPhilosophers;
        private readonly int _index;

        public Philosopher(List<Philosopher> allPhilosophers, int index, Form1 form)
        {
            _allPhilosophers = allPhilosophers;
            _index = index;
            this.Name = string.Format("Philosopher {0}", _index);
            this.State = State.Thinking;
            this.form = form;
        }

        public string Name { get; private set; }
        public State State { get; private set; }
        public Fork LeftFork { get; set; }
        public Fork RightFork { get; set; }

        public Philosopher LeftPhilosopher
        {
            get
            {
                if (_index == 0)
                    return _allPhilosophers[_allPhilosophers.Count - 1];
                else
                    return _allPhilosophers[_index - 1];
            }
        }

        public Philosopher RightPhilosopher
        {
            get
            {
                if (_index == _allPhilosophers.Count - 1)
                    return _allPhilosophers[0];
                else
                    return _allPhilosophers[_index + 1];
            }
        }

        public void EatAll()
        {
        
            while (true)
            {
                this.Think();
                if(Test()) continue;
                if (this.PickUp())
                {
                    this.Eat();

                    this.PutDownLeft();
                    this.PutDownRight();

                }

            }
        }

        private bool PickUp()
        {
            
            if (Monitor.TryEnter(this.LeftFork))
            {
                string data1 = this.Name + " picks up left fork.";
                form.Invoke((MethodInvoker)delegate{ form.UpdateData(data1); });
                LeftInfo(0);
                this.State = State.Starving;
                string[] namePh = this.Name.Split(' ');
                int numPh = int.Parse(namePh[1]);
                form.Invoke((MethodInvoker)delegate { form.UpdateListView("Starving", numPh, 1); });
                Thread.Sleep(1000);
                
                if (Monitor.TryEnter(this.RightFork))
                {
                    string data2 = this.Name + " picks up right fork.";
                    form.Invoke((MethodInvoker)delegate { form.UpdateData(data2); });
                    RightInfo(0);
                    Thread.Sleep(1000);
                    return true;
                }
                else
                {
                    this.PutDownLeft();
                }
            }
            return false;
        }

        private void Eat()
        {
            this.State = State.Eating;
            _timesEaten++;
            string data = this.Name + " eats.";
            form.Invoke((MethodInvoker)delegate { form.UpdateData(data); });
            string[] namePh = this.Name.Split(' ');
            int numPh = int.Parse(namePh[1]);
            form.Invoke((MethodInvoker)delegate { form.UpdateListView("Ate", numPh, 1); });
            form.Invoke((MethodInvoker)delegate { form.UpdateListView(_timesEaten.ToString(), numPh, 4); });
            Thread.Sleep(1000);

        }

        private void PutDownLeft()
        {
            Monitor.Exit(this.LeftFork);
            string data = this.Name + " puts down left fork.";
            form.Invoke((MethodInvoker)delegate { form.UpdateData(data); });
            string[] namePh = this.Name.Split(' ');
            int numPh = int.Parse(namePh[1]);
            form.Invoke((MethodInvoker)delegate { form.UpdateListView("Thinking", numPh, 1); });
            LeftInfo(1);
            Thread.Sleep(1000);
        }

        private void PutDownRight()
        {
            Monitor.Exit(this.RightFork);
            string data = this.Name + " puts down right fork.";
            form.Invoke((MethodInvoker)delegate { form.UpdateData(data); });
            RightInfo(1);
            Thread.Sleep(1000);
        }

        private void Think()
        {
            this.State = State.Thinking;
            string[] namePh = this.Name.Split(' ');
            int numPh = int.Parse(namePh[1]);
            form.Invoke((MethodInvoker)delegate { form.UpdateListView("Thinking", numPh, 1); });
            Thread.Sleep(1000);

        }
        
        public void LeftInfo(int i)
        {
            string[] namePh = this.Name.Split(' ');
            int numPh = int.Parse(namePh[1]);
            if (i == 0) form.Invoke((MethodInvoker)delegate { form.UpdateListView("+", numPh, 2); });
            else form.Invoke((MethodInvoker)delegate { form.UpdateListView("-", numPh, 2); });
        }
        public void RightInfo(int i)
        {
            string[] namePh = this.Name.Split(' ');
            int numPh = int.Parse(namePh[1]);
            if (i == 0) form.Invoke((MethodInvoker)delegate { form.UpdateListView("+", numPh, 3); });
            else form.Invoke((MethodInvoker)delegate { form.UpdateListView("-", numPh, 3); });
        }
        public bool Test()
        {
            foreach (var philosopher in _allPhilosophers)
            {
                if (((this._timesEaten + 1) - philosopher._timesEaten) == 2 && !philosopher.Name.Equals(this.Name))
                    return true;
            }
            return false;
        }
    }

            public class Fork
            {
                private static int _count = 1;
                public string Name { get; private set; }

                public Fork()
                {
                    this.Name = "Fork " + _count++;
                }
            }

    public enum State
    {
        Thinking,
        Eating,
        Starving
    }
}