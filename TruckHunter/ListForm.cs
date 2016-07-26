using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.Threading;
using System.Deployment.Application;
using asa;


namespace TruckHunter
{
    public partial class ListForm : Form
    {
        public ListForm()
        {
            Navigator.NavigatorEventHandler = new Navigator.NavigatorEvent(this.ListNavigator);
            InitializeComponent();
        }

        public Form Hunt = new EventForm();

        private bool refreshaction, startup, refreshrays, wait = false;

        private SqlConnection intellectDB = new SqlConnection();

        //private OleDbConnection localDB = new OleDbConnection();

        void ListNavigator(bool listvector, int tab)
        {
                int count = listView2.SelectedIndices[0];
                if (listvector)
                {
                    listView2.Items[count].Selected = false;
                    if (count < listView2.Items.Count - 1)
                    {
                        listView2.Items[count + 1].Selected = true;
                        ChangeSelected();
                    }
                    else
                    {
                        listView2.Items[0].Selected = true;
                        ChangeSelected();
                    }
                }
                else
                {
                    listView2.Items[count].Selected = false;
                    if (count > 0)
                    {
                        listView2.Items[count - 1].Selected = true;
                        ChangeSelected();
                    }
                    else
                    {
                        listView2.Items[listView2.Items.Count - 1].Selected = true;
                        ChangeSelected();
                    }
                }
        }

        private void ChangeSelected()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\aydnep IT-consulting\\Objects\\" + comboBox1.SelectedItem.ToString());
            if (intellectDB.State == ConnectionState.Open)
            {
                intellectDB.Close();
            }
            else
            {
                try
                {
                    intellectDB.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Невозможно подключиться к БД: " + comboBox1.SelectedItem.ToString());
                    Log.Write(ex, intellectDB.ConnectionString);
                    return;
                }
            }
            SqlCommand sqlrun = new SqlCommand("SELECT [InCam], [OutCam], [UpCam] FROM [TruckHunterEntries] WHERE [Name] LIKE '" + listView2.Items[listView2.SelectedIndices[0]].Text.ToString() + "'", intellectDB);
            SqlDataReader rdr = null;
            try
            {
                rdr = sqlrun.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show("sql error");
                Log.Write(ex, sqlrun.CommandText);
                return;
            }
            string srv, ttn, nomer, kultura, mesto, entry, interv = "";
            int cam1, cam2, cam3 = 0;
            DateTime sdat, edat, updat = DateTime.Now;

            if (listView2.Items[listView2.SelectedIndices[0]].SubItems[3].Text.ToString() == "въезд") // Ставим порядок камер
            {
                if (rdr.Read())
                {
                    cam1 = Convert.ToInt32(rdr[0].ToString());
                    cam2 = Convert.ToInt32(rdr[1].ToString());
                    cam3 = Convert.ToInt32(rdr[2].ToString());
                }
                else
                {
                    cam1 = 0;
                    cam2 = 0;
                    cam3 = 0;
                }
            }
            else // Меняем местами камеры
            {
                if (rdr.Read())
                {
                    cam1 = Convert.ToInt32(rdr[1].ToString());
                    cam2 = Convert.ToInt32(rdr[0].ToString());
                    cam3 = Convert.ToInt32(rdr[2].ToString());
                }
                else
                {
                    cam1 = 0;
                    cam2 = 0;
                    cam3 = 0;
                }
            }
            srv = key.GetValue("Server").ToString();
            srv = srv.Replace("\\SQLEXPRESS", "");
            ttn = listView2.Items[listView2.SelectedIndices[0]].SubItems[4].Text.ToString();
            nomer = listView2.Items[listView2.SelectedIndices[0]].SubItems[5].Text.ToString();
            kultura = listView2.Items[listView2.SelectedIndices[0]].SubItems[6].Text.ToString();
            mesto = listView2.Items[listView2.SelectedIndices[0]].SubItems[7].Text.ToString();
            sdat = DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[1].Text.ToString());
            edat = DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[2].Text.ToString());
            TimeSpan diff = edat - sdat;
            TimeSpan half = new TimeSpan(diff.Ticks / 2);
            updat = sdat + half;
            interv = listView2.Items[listView2.SelectedIndices[0]].SubItems[1].Text.ToString() + " - " + DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[2].Text.ToString()).ToString("HH:mm:ss");
            sdat = sdat.AddSeconds(Convert.ToInt32(key.GetValue("AfterBegin").ToString()));
            edat = edat.AddSeconds(Convert.ToInt32(key.GetValue("BeforeEnd").ToString()) * (-1));
            entry = listView2.Items[listView2.SelectedIndices[0]].Text.ToString();
            if (cam3 == 0)
            {
                MessageBox.Show("sql error");
                return;
            }
            rdr.Close();
            intellectDB.Close();
            Color _brush = listView2.Items[listView2.SelectedIndices[0]].BackColor;
            CamMonitor.CamMonitorEventHandler(srv, cam1, cam2, cam3, sdat, edat, updat, ttn, nomer, kultura, mesto, entry, interv, refreshaction, _brush, trackBar4.Value);
            refreshaction = false;
            listView2.Items[listView2.SelectedIndices[0]].BackColor = Properties.Settings.Default.processedEventColor;
            listView2.Items[listView2.SelectedIndices[0]].ForeColor = Color.White;
        }

        private void ChangeSelectedR()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\aydnep IT-consulting\\Objects\\" + comboBox1.SelectedItem.ToString());
            if (intellectDB.State == ConnectionState.Open)
            {
                intellectDB.Close();
            }
            else
            {
                try
                {
                    intellectDB.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Невозможно подключиться к БД: " + comboBox1.SelectedItem.ToString());
                    Log.Write(ex, intellectDB.ConnectionString);
                    return;
                }
            }
            SqlCommand sqlrun = new SqlCommand("SELECT [InCam], [OutCam], [UpCam] FROM [TruckHunterEntries] WHERE [Name] LIKE '" + listView2.Items[listView2.SelectedIndices[0]].Text.ToString() + "'", intellectDB);
            SqlDataReader rdr = null;
            try
            {
                rdr = sqlrun.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show("sql error");
                Log.Write(ex, sqlrun.CommandText);
                return;
            }
            string srv, ttn, nomer, kultura, mesto, entry, interv = "";
            int cam1, cam2, cam3 = 0;
            DateTime sdat, edat, updat = DateTime.Now;
            if (rdr.Read())
            {
                cam1 = Convert.ToInt32(rdr[0].ToString());
                cam2 = Convert.ToInt32(rdr[1].ToString());
                cam3 = Convert.ToInt32(rdr[2].ToString());
            }
            else
            {
                cam1 = 0;
                cam2 = 0;
                cam3 = 0;
            }

            srv = key.GetValue("Server").ToString();
            srv = srv.Replace("\\SQLEXPRESS", "");
            ttn = listView2.Items[listView2.SelectedIndices[0]].SubItems[5].Text.ToString();
            nomer = listView2.Items[listView2.SelectedIndices[0]].SubItems[1].Text.ToString();
            kultura = listView2.Items[listView2.SelectedIndices[0]].SubItems[2].Text.ToString();
            mesto = "Нарушение логики маятника";
            sdat = DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[4].Text.ToString());
            edat = DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[3].Text.ToString());
            TimeSpan diff = edat - sdat;
            TimeSpan half = new TimeSpan(diff.Ticks / 2);
            updat = sdat + half;
            interv = listView2.Items[listView2.SelectedIndices[0]].SubItems[4].Text.ToString() + " - " + DateTime.Parse(listView2.Items[listView2.SelectedIndices[0]].SubItems[3].Text.ToString()).ToString("HH:mm:ss");
            entry = listView2.Items[listView2.SelectedIndices[0]].Text.ToString();
            if (cam3 == 0)
            {
                MessageBox.Show("sql error");
                return;
            }
            rdr.Close();
            intellectDB.Close();
            Color _brush = listView2.Items[listView2.SelectedIndices[0]].BackColor;
            CamMonitor.CamMonitorEventHandler(srv, cam1, cam2, cam3, sdat, edat, updat, ttn, nomer, kultura, mesto, entry, interv, refreshaction, _brush, trackBar4.Value);
            refreshaction = false;
            listView2.Items[listView2.SelectedIndices[0]].BackColor = Properties.Settings.Default.processedEventColor;
            listView2.Items[listView2.SelectedIndices[0]].ForeColor = Color.White;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Bounds = Screen.PrimaryScreen.WorkingArea;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.MaximizeBox = false;

            if (!ApplicationDeployment.IsNetworkDeployed)
            {
            #if (DEBUG)
            {
            }
            #else
            {
                DialogResult dr = MessageBox.Show("Внимание!\n\n Для корректной работы охотника ПО необходимо запускать через ярлыки в меню Пуск или на Рабочем столе.\n\n Программа будед завершена!", "Запуск", MessageBoxButtons.OK);
                if (DialogResult.OK == dr)
                {
                    Application.Exit();
                }
            }
            #endif
            }

            asaTools ayASA = new asaTools();
            if (!ayASA.asa(Application.ProductName))
            {
                Form info = new asa.Form1(Properties.Settings.Default.modules);
                info.ShowDialog();
                Application.Exit();
            }
            this.Text = Application.ProductName;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software"); 
            dateTimePicker1.Value = DateTime.Now.Date;
            dateTimePicker2.Value = DateTime.Now.Date;
            dateTimePicker2.Value = dateTimePicker2.Value.AddHours(23);
            dateTimePicker2.Value = dateTimePicker2.Value.AddMinutes(59);
            dateTimePicker2.Value = dateTimePicker2.Value.AddSeconds(59);
            try
            {
                key = key.OpenSubKey("aydnep IT-consulting", true);
                key = key.OpenSubKey("Objects", true);
                comboBox1.Items.AddRange(key.GetSubKeyNames());
            }
            catch (Exception ex) 
            {
                DialogResult dr = MessageBox.Show("Нет активных источников. Импортировать?","Настройка охотника", MessageBoxButtons.OKCancel);
                if (DialogResult.OK == dr)
                {
                    openFileDialog1.DefaultExt = "*.reg";
                    openFileDialog1.Filter = "Файл реестра (.reg)|*.reg";
                    openFileDialog1.ShowDialog();
                    MessageBox.Show("Импорт произведен успешно, вроде бы... Необходимо перезапустить ПО");
                    this.Close();
                }
                Log.Write(ex, ""); 
                Application.Exit(); 
            }
            listView2.FullRowSelect = true;
            listView2.FullRowSelect = true;
            /*localDB.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Application.ProductName + ".mdb";
            try
            {
                localDB.Open();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("Нарушена структура программы!");
                Application.Exit();
            }
            OleDbCommand eraseDB = new OleDbCommand("DELETE * FROM SM", localDB);
            eraseDB.ExecuteNonQuery();
            eraseDB.CommandText = "DELETE * FROM PROTOCOL";
            eraseDB.ExecuteNonQuery();
            localDB.Close();*/
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new AboutBox1();
            about.Show();
        }

        private void panel2_MouseHover(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            panel1.Visible = true;
            label5.Visible = false;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            panel1.Visible = false;
            timer1.Enabled = false;
            label5.Visible = true;
        }

        private void FillThePower()
        {
            int legalcount = 0;
            int illegalcount = 0;
            int suspectcount = 0;
            string obj = string.Empty;
            comboBox1.Invoke((MethodInvoker)delegate 
            { 
                obj = comboBox1.SelectedItem.ToString();
            });
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\aydnep IT-consulting\\Objects\\" + obj);
            intellectDB.ConnectionString = "Data Source=" + key.GetValue("Server").ToString() + ";Initial Catalog=" + key.GetValue("DB").ToString() + ";" + "user id=" + Properties.Settings.Default.name.ToString() + ";password=" + Properties.Settings.Default.phone.ToString() + "; MultipleActiveResultSets=true;";
            Boolean enter, exit, victim = false;
            DateTime sdate, edate = DateTime.Parse("1984-06-01");
            String vector = "";
            int delay = Convert.ToInt32(key.GetValue("RaysDelay").ToString());
            //Пытаемся подключиться к БД
            
            try
            {
                intellectDB.Open();
            } //Пытаемся подключиться к БД
            catch (Exception ex)
            {
                MessageBox.Show("Невозможно подключиться к БД: " + obj);
                Log.Write(ex, intellectDB.ConnectionString);
                restoreCaption();
                return;
            } // Пытаемся подключиться к БД

            // Проверяем дату

            using (SqlCommand _qDateCheck = new SqlCommand("SELECT GETDATE()",intellectDB))
            {
                try
                {
                    using (SqlDataReader _rDateCheck = _qDateCheck.ExecuteReader())
                    {
                        _rDateCheck.Read();
                        if ((DateTime.Parse(_rDateCheck[0].ToString()) - DateTime.Now).Days > 0)
                        {
                            MessageBox.Show("Похоже установлена неверная дата! Нужно исправить!");
                            restoreCaption();
                            return;
                        }
                    } // Ридер даты
                }
                catch (Exception ex)
                {
                    Log.Write(ex, "");
                    restoreCaption();
                    return;
                } // 
            } // Проверяем дату

            // Формируем структуру листвью

            listView2.Invoke((MethodInvoker)delegate
            {
                listView2.Columns.Add(new ColHeader("Проезд", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Начало", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Конец", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Направление", listView2.Width / 16, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("ТТН", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Госномер", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Культура", listView2.Width / 8, HorizontalAlignment.Left, true));
                listView2.Columns.Add(new ColHeader("Действие 1С", listView2.Width / 4, HorizontalAlignment.Left, true));
                this.listView2.ColumnClick += new ColumnClickEventHandler(listView2_ColumnClick);
            });
            startup = true;
            //localDB.Open();
            string datefrom = string.Empty;
            string dateto = string.Empty;
            dateTimePicker1.Invoke((MethodInvoker)delegate
            {
                datefrom = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");
            });
            dateTimePicker2.Invoke((MethodInvoker)delegate
            {
                dateto = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss");
            });

            // Выборка из проездов

            using (SqlCommand _qEntries = new SqlCommand("SELECT ID FROM [TruckHunterEntries]", intellectDB))
            {
                
                // Читаем проезды
                
                try
                {
                    using (SqlDataReader _rEntries = _qEntries.ExecuteReader())
                    {
                        while (_rEntries.Read())
                        { 
                            
                            // Выборка сработок

                            using (SqlCommand _qProtocol = new SqlCommand())
                            {
                                _qProtocol.CommandText = "SELECT (SELECT [name] FROM [TruckHunterEntries] WHERE ID = " + _rEntries[0].ToString() + ") as [Entry] ";
                                _qProtocol.CommandText += ",[objid],[action],[date],[vector] FROM [PROTOCOL] LEFT OUTER JOIN [TruckHunterRays] ON [PROTOCOL].[objid] = [TruckHunterRays].[Ray] ";
                                _qProtocol.CommandText += "WHERE ([objtype]='GRAY') and ([objid] in ((SELECT [Ray] FROM [TruckHunterRays] WHERE [Entry] = " + _rEntries[0].ToString() + "))) ";
                                _qProtocol.CommandText += "and ([date] >= '" + datefrom + "') ";
                                _qProtocol.CommandText += "and ([date] <= '" + dateto + "')  AND [action] IN ('ON', 'OFF') ORDER BY [date]";
                                _qProtocol.Connection = intellectDB;
                                enter = false;
                                exit = false;
                                victim = false;
                                sdate = DateTime.Parse("1984-06-01");
                                edate = DateTime.Parse("1986-06-08");
                                vector = "";
                                TimeSpan _delay = TimeSpan.FromSeconds(delay);
                                // Запрашиваем события

                                try
                                {
                                    using (SqlDataReader _rProtocol = _qProtocol.ExecuteReader())
                                    {
                                        while (_rProtocol.Read()) // Парсим действия лучей по указанному проезду
                                        {
                                            /*OleDbCommand snapshot = new OleDbCommand("INSERT INTO PROTOCOL (entry,objid,arm,actiontime) VALUES ('" + _rProtocol[0].ToString() + "'," + _rProtocol[1].ToString() + ",'" + _rProtocol[2].ToString() + "','" + _rProtocol[3].ToString() + "')", localDB);
                                            snapshot.ExecuteNonQuery();*/
                                            if (_rProtocol[2].ToString() == key.GetValue("RaysType").ToString()) // Луч замкнулся?
                                            {
                                                if (Convert.ToInt32(_rProtocol[4].ToString()) == 1) // Это вЪезд?
                                                {
                                                    enter = true;
                                                    if (exit) // Второй замкнут?
                                                    {
                                                        victim = true;
                                                        // Ничего не делаем
                                                    }
                                                    else // Если нет
                                                    {
                                                        sdate = DateTime.Parse(_rProtocol[3].ToString());
                                                        vector = "въезд";
                                                    } // Проверка второго луча
                                                }
                                                else // Это вЫезд
                                                {
                                                    exit = true;
                                                    if (enter) // Второй замкнут?
                                                    {
                                                        victim = true;
                                                        // Ничего не делаем
                                                    }
                                                    else // Если нет
                                                    {
                                                        sdate = DateTime.Parse(_rProtocol[3].ToString());
                                                        vector = "вЫезд";
                                                    } // Проверка второго луча
                                                } //Направление
                                            }
                                            else// if (_rProtocol[2].ToString() != "ALARM" && _rProtocol[2].ToString() != "DISARM" && _rProtocol[2].ToString() != "SIGNAL_LOST")//Луч разомкнулся
                                            {
                                                if (Convert.ToInt32(_rProtocol[4].ToString()) == 1) // Это вЪезд?
                                                {
                                                    enter = false;
                                                    if (exit) // Второй замкнут?
                                                    {
                                                        DateTime thistime = DateTime.Parse(_rProtocol[3].ToString());
                                                        // Ничего не делаем
                                                    }
                                                    else // Если нет
                                                    {
                                                        edate = DateTime.Parse(_rProtocol[3].ToString());
                                                        if (victim)
                                                        {
                                                            victim = false;
                                                            /*if ((edate - sdate).Seconds >= delay || (edate - sdate).Minutes > 0 || (edate - sdate).Hours > 0)
                                                            {
                                                                SqlCommand legality = new SqlCommand();
                                                                SqlDataReader _rProtocol1c = null;
                                                                OleDbCommand insertMS = new OleDbCommand("INSERT INTO SM (entryID,entry,datebegin,dateend) VALUES (" + i.ToString() + @",'" + _rProtocol[0].ToString() + @"','" + sdate.ToString() + @"','" + edate.ToString() + @"')", localDB);
                                                                insertMS.ExecuteNonQuery();*/
                                                                using (SqlCommand _q1c = new SqlCommand())
                                                                {
                                                                    _q1c.Connection = intellectDB;
                                                                    _q1c.CommandText = "SELECT TOP 1 [ID], [TruckID], [Culture], [WHO] FROM [PROTOCOL1C] ";
                                                                    _q1c.CommandText += "WHERE ([Entry] = " + _rEntries[0].ToString() + ") AND ([Date] >= '" + sdate.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                                                    _q1c.CommandText += "AND ([Date] <= '" + edate.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                                                    try
                                                                    {
                                                                        using (SqlDataReader _r1c = _q1c.ExecuteReader())
                                                                        {
                                                                            if (_r1c.Read()) // Если нашли событие 1С
                                                                            {
                                                                                listView2.Invoke((MethodInvoker)delegate
                                                                                {
                                                                                    listView2.Items.Add(_rProtocol[0].ToString()); // Название проезда
                                                                                    listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.legalEventColor;
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss")); // начало
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss")); // конец
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector); // направление
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[0].ToString()); // ТТН
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[1].ToString()); // Госномер
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[2].ToString()); // Культура
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[3].ToString()); // Инициатор
                                                                                });
                                                                                legalcount++;
                                                                            }
                                                                            else // Если не находим то...
                                                                            {
                                                                                if ((edate - sdate) >= _delay) // Точно не легальный
                                                                                {
                                                                                    listView2.Invoke((MethodInvoker)delegate
                                                                                    {
                                                                                        listView2.Items.Add(_rProtocol[0].ToString());
                                                                                        listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПОДТВЕРЖДЕННЫЙ"); // Госномер
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("С ОСТАНОВКОЙ"); // Инициатор
                                                                                    });
                                                                                    illegalcount++;
                                                                                }
                                                                                else // Подозрительный может какой человекобак
                                                                                {
                                                                                    listView2.Invoke((MethodInvoker)delegate
                                                                                    {
                                                                                        listView2.Items.Add(_rProtocol[0].ToString());
                                                                                        listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ЛЕГАЛЬНЫЙ"); // Госномер
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("!!!"); // Инициатор
                                                                                    });
                                                                                    suspectcount++;
                                                                                } // Подозрительный может какой человекобак
                                                                            } // Не смогли найти событие 1С
                                                                        } // Читаем события 1С
                                                                    } // Попытка прочитать события 1С
                                                                    catch
                                                                    {
                                                                        if ((edate - sdate) >= _delay) // Точно не легальный
                                                                        {
                                                                            listView2.Invoke((MethodInvoker)delegate
                                                                            {
                                                                                listView2.Items.Add(_rProtocol[0].ToString());
                                                                                listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПОДТВЕРЖДЕННЫЙ"); // Госномер
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("С ОСТАНОВКОЙ"); // Инициатор
                                                                            });
                                                                            illegalcount++;
                                                                        }
                                                                        else // Подозрительный может какой человекобак
                                                                        {
                                                                            listView2.Invoke((MethodInvoker)delegate
                                                                            {
                                                                                listView2.Items.Add(_rProtocol[0].ToString());
                                                                                listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ЛЕГАЛЬНЫЙ"); // Госномер
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("!!!"); // Инициатор
                                                                            });
                                                                            suspectcount++;
                                                                        } // Подозрительный может какой человекобак
                                                                    } // Не получилось прочитать события из 1С

                                                                } // SELECT FORM PROTOCOL1C
                                                            //} // врмея события больше чем что то там
                                                        } // флаг жертвы VICTIM
                                                    } // Проверка второго луча
                                                }
                                                else // Это вЫезд
                                                {
                                                    exit = false;
                                                    if (enter) // Второй замкнут?
                                                    {
                                                        DateTime thistime = DateTime.Parse(_rProtocol[3].ToString());
                                                        // Ничего не делаем
                                                    }
                                                    else // Если нет
                                                    {
                                                        edate = DateTime.Parse(_rProtocol[3].ToString());
                                                        if (victim)
                                                        {
                                                            victim = false;
                                                            /*if ((edate - sdate).Seconds >= delay || (edate - sdate).Minutes > 0 || (edate - sdate).Hours > 0)
                                                            {
                                                                SqlCommand legality = new SqlCommand();
                                                                SqlDataReader _rProtocol1c = null;
                                                                OleDbCommand insertMS = new OleDbCommand("INSERT INTO SM (entryID,entry,datebegin,dateend) VALUES (" + i.ToString() + @",'" + _rProtocol[0].ToString() + @"','" + sdate.ToString() + @"','" + edate.ToString() + @"')", localDB);
                                                                insertMS.ExecuteNonQuery();*/
                                                                using (SqlCommand _q1c = new SqlCommand())
                                                                {
                                                                    _q1c.Connection = intellectDB;
                                                                    _q1c.CommandText = "SELECT TOP 1 [ID], [TruckID], [Culture], [WHO] FROM [PROTOCOL1C] ";
                                                                    _q1c.CommandText += "WHERE ([Entry] = " + _rEntries[0].ToString() + ") AND ([Date] >= '" + sdate.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                                                    _q1c.CommandText += "AND ([Date] <= '" + edate.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                                                    try
                                                                    {
                                                                        using (SqlDataReader _r1c = _q1c.ExecuteReader())
                                                                        {
                                                                            if (_r1c.Read()) // Если нашли событие 1С
                                                                            {
                                                                                listView2.Invoke((MethodInvoker)delegate
                                                                                {
                                                                                    listView2.Items.Add(_rProtocol[0].ToString()); // Название проезда
                                                                                    listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.legalEventColor;
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss")); // начало
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss")); // конец
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector); // направление
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[0].ToString()); // ТТН
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[1].ToString()); // Госномер
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[2].ToString()); // Культура
                                                                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(_r1c[3].ToString()); // Инициатор
                                                                                });
                                                                                legalcount++;
                                                                            }
                                                                            else // Если не находим то...
                                                                            {
                                                                                if ((edate - sdate) >= _delay) // Точно не легальный
                                                                                {
                                                                                    listView2.Invoke((MethodInvoker)delegate
                                                                                    {
                                                                                        listView2.Items.Add(_rProtocol[0].ToString());
                                                                                        listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПОДТВЕРЖДЕННЫЙ"); // Госномер
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("С ОСТАНОВКОЙ"); // Инициатор
                                                                                    });
                                                                                    illegalcount++;
                                                                                }
                                                                                else // Подозрительный может какой человекобак
                                                                                {
                                                                                    listView2.Invoke((MethodInvoker)delegate
                                                                                    {
                                                                                        listView2.Items.Add(_rProtocol[0].ToString());
                                                                                        listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ЛЕГАЛЬНЫЙ"); // Госномер
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                        listView2.Items[listView2.Items.Count - 1].SubItems.Add("!!!"); // Инициатор
                                                                                    });
                                                                                    suspectcount++;
                                                                                } // Подозрительный может какой человекобак
                                                                            } // Не смогли найти событие 1С
                                                                        } // Читаем события 1С
                                                                    } // Попытка прочитать события 1С
                                                                    catch
                                                                    {
                                                                        if ((edate - sdate) >= _delay) // Точно не легальный
                                                                        {
                                                                            listView2.Invoke((MethodInvoker)delegate
                                                                            {
                                                                                listView2.Items.Add(_rProtocol[0].ToString());
                                                                                listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПОДТВЕРЖДЕННЫЙ"); // Госномер
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("С ОСТАНОВКОЙ"); // Инициатор
                                                                            });
                                                                            illegalcount++;
                                                                        }
                                                                        else // Подозрительный может какой человекобак
                                                                        {
                                                                            listView2.Invoke((MethodInvoker)delegate
                                                                            {
                                                                                listView2.Items.Add(_rProtocol[0].ToString());
                                                                                listView2.Items[listView2.Items.Count - 1].BackColor = Properties.Settings.Default.illegalEventColor;
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(edate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(vector);
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("НЕ"); // ТТН
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ЛЕГАЛЬНЫЙ"); // Госномер
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОЕЗД"); // Культура
                                                                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("!!!"); // Инициатор
                                                                            });
                                                                            suspectcount++;
                                                                        } // Подозрительный может какой человекобак
                                                                    } // Не получилось прочитать события из 1С

                                                                } // SELECT FORM PROTOCOL1C
                                                            //} // врмея события больше чем что то там
                                                        } // флаг жертвы VICTIM
                                                    } // Проверка второго луча
                                                } //Направление
                                            } // Замыкание/Размыкание
                                        } // Парсим действия лучей по указанному проезду
                                    } // Читаем события
                                } // Пытаемся прочитать протокол
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Ошибка запроса!");
                                    Log.Write(ex, _qProtocol.CommandText);
                                    restoreCaption();
                                    return;
                                } // Ошибка запроса событий

                            } // SELECT FROM PROTOCOL
                        } // Пока читаем проезды
                    } // Ридер проездов
                } // ОШИБКА запроса в проездах
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка запроса!");
                    Log.Write(ex, _qEntries.CommandText);
                    restoreCaption();
                    return;
                } // ОШИБКА запроса в проездах

            } // SELECT FROM TruckHunterEntries
            intellectDB.Close();
            textBox2.Text = legalcount.ToString();
            textBox3.Text = illegalcount.ToString();
            textBox4.Text = suspectcount.ToString();
            textBox5.Text = (legalcount + illegalcount + suspectcount).ToString();
            restoreCaption();
        }

        private void restoreCaption()
        {
            button1.Invoke((MethodInvoker)delegate
            {
                button1.Text = "Сформировать";
                button1.Enabled = true;
            });
            listView2.Invoke((MethodInvoker)delegate
            {
                listView2.UseWaitCursor = false;
            });
            wait = false;
            intellectDB.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Ждемс...";
            wait = true;
            listView2.UseWaitCursor = true;
            button1.Enabled = false;
            refreshaction = true;
            refreshrays = true;
            listView2.BringToFront();
            textBox1.Enabled = true;
            checkBox3.Checked = false;
            startup = false;
            listView2.Clear();
            listView2.Groups.Clear();
            listView2.Clear();
            listView2.Groups.Clear();
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            Thread brainstorm = new Thread(FillThePower);
            brainstorm.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit();  }
        }

        private void listView1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit(); }
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit(); }
        }

        private void dateTimePicker1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit(); }
        }

        private void dateTimePicker2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit(); }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                intellectDB.Close();
            }
            catch
            { }
            Application.Exit();

        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (wait)
            {
                MessageBox.Show("Охотник не насытился. Надо ждать...");
                return;
            }
            ChangeSelected();
            listView2.Items[listView2.SelectedIndices[0]].BackColor = Properties.Settings.Default.processedEventColor;
            listView2.Items[listView2.SelectedIndices[0]].ForeColor = Color.White;
            Hunt.Show(this);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form about = new AboutBox1();
            about.ShowDialog();
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //MessageBox.Show("Click");
            // Create an instance of the ColHeader class.
            ColHeader clickedCol = (ColHeader)this.listView2.Columns[e.Column];

            // Set the ascending property to sort in the opposite order.
            clickedCol.ascending = !clickedCol.ascending;

            // Get the number of items in the list.
            int numItems = this.listView2.Items.Count;

            // Turn off display while data is repoplulated.
            this.listView2.BeginUpdate();

            // Populate an ArrayList with a SortWrapper of each list item.
            ArrayList SortArray = new ArrayList();
            for (int i = 0; i < numItems; i++)
            {
                SortArray.Add(new SortWrapper(this.listView2.Items[i], e.Column));
            }

            // Sort the elements in the ArrayList using a new instance of the SortComparer
            // class. The parameters are the starting index, the length of the range to sort,
            // and the IComparer implementation to use for comparing elements. Note that
            // the IComparer implementation (SortComparer) requires the sort
            // direction for its constructor; true if ascending, othwise false.
            SortArray.Sort(0, SortArray.Count, new SortWrapper.SortComparer(clickedCol.ascending));

            // Clear the list, and repopulate with the sorted items.
            this.listView2.Items.Clear();
            for (int i = 0; i < numItems; i++)
                this.listView2.Items.Add(((SortWrapper)SortArray[i]).sortItem);

            // Turn display back on.
            this.listView2.EndUpdate();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\aydnep IT-consulting\\Objects\\" + comboBox1.SelectedItem.ToString());
            trackBar1.Value = Convert.ToInt32(key.GetValue("RaysDelay").ToString());
            trackBar2.Value = Convert.ToInt32(key.GetValue("AfterBegin").ToString());
            trackBar3.Value = Convert.ToInt32(key.GetValue("BeforeEnd").ToString());
            try
            {
                trackBar4.Value = Convert.ToInt32(key.GetValue("Compress").ToString());
            }
            catch
            {
                trackBar4.Value = 1;
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            timer1.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                dateTimePicker1.CustomFormat = "dd.MM.yyyy HH:mm:ss";
                dateTimePicker1.Format = DateTimePickerFormat.Custom;
                dateTimePicker1.ShowUpDown = true;
                dateTimePicker2.CustomFormat = "dd.MM.yyyy HH:mm:ss";
                dateTimePicker2.Format = DateTimePickerFormat.Custom;
                dateTimePicker2.ShowUpDown = true;
            }
            else
            {
                dateTimePicker1.Format = DateTimePickerFormat.Long;
                dateTimePicker1.ShowUpDown = false;
                dateTimePicker2.Format = DateTimePickerFormat.Long;
                dateTimePicker2.ShowUpDown = false;

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Call FindItemWithText with the contents of the textbox.
            ListViewItem foundItem =
                listView2.FindItemWithText(textBox1.Text, true, 0, true);
            if (foundItem != null)
            {
                listView2.TopItem = foundItem;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox1.Enabled = false;
                textBox1.Text = "";
                bool flag = true;
                foreach (ListViewItem l in listView2.Items)
                {
                    string strmyGroupname = l.SubItems[4].Text;
                    foreach (ListViewGroup lvg in listView2.Groups)
                    {
                        if (lvg.Name == strmyGroupname)
                        {
                            l.Group = lvg;
                            flag = false;
                        }
                    }
                    if (flag == true)
                    {
                        ListViewGroup lstGrp = new ListViewGroup(strmyGroupname, strmyGroupname);
                        listView2.Groups.Add(lstGrp);
                        l.Group = lstGrp;
                    }
                    flag = true;
                }  
            }
            else
            {
                listView2.Groups.Clear();
                textBox1.Enabled = true;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                textBox2.Visible = true;
                textBox3.Visible = true;
                textBox4.Visible = true;
                textBox5.Visible = true;
                label6.Visible = true;
                label7.Visible = true;
                label8.Visible = true;
                label9.Visible = true;
            }
            else
            {
                textBox2.Visible = false;
                textBox3.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                label1.Visible = false;
                label2.Visible = false;
                dateTimePicker1.Visible = false;
                dateTimePicker2.Visible = false;
                checkBox1.Visible = false;
                panel3.Visible = true;
                trackBar1.Visible = true;
                trackBar2.Visible = true;
                trackBar3.Visible = true;
                trackBar4.Visible = true;
                button5.Visible = true;
            }
            else
            {
                label1.Visible = true;
                label2.Visible = true;
                dateTimePicker1.Visible = true;
                dateTimePicker2.Visible = true;
                checkBox1.Visible = true;
                panel3.Visible = false;
                trackBar1.Visible = false;
                trackBar2.Visible = false;
                trackBar3.Visible = false;
                trackBar4.Visible = false;
                button5.Visible = false;
                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\aydnep IT-consulting\\Objects\\" + comboBox1.SelectedItem.ToString());
                key.SetValue("RaysDelay", trackBar1.Value.ToString());
                key.SetValue("AfterBegin", trackBar2.Value.ToString());
                key.SetValue("BeforeEnd", trackBar3.Value.ToString());
                key.SetValue("Compress", trackBar4.Value.ToString());
                key.Close();
                Properties.Settings.Default.Save();
            }
        }

        private void panel2_MouseHover_1(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void listView2_Resize(object sender, EventArgs e)
        {
            /*if (startup)
            {
                listView2.Columns[0].Width = listView2.Width / 8;
                listView2.Columns[1].Width = listView2.Width / 8;
                listView2.Columns[2].Width = listView2.Width / 8;
                listView2.Columns[3].Width = listView2.Width / 16;
                listView2.Columns[4].Width = listView2.Width / 8;
                listView2.Columns[5].Width = listView2.Width / 8;
                listView2.Columns[6].Width = listView2.Width / 8;
                listView2.Columns[7].Width = listView2.Width / 4;
            }*/
        }

        private void listView23_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //MessageBox.Show("Click");
            // Create an instance of the ColHeader class.
            ColHeader clickedCol = (ColHeader)this.listView2.Columns[e.Column];

            // Set the ascending property to sort in the opposite order.
            clickedCol.ascending = !clickedCol.ascending;

            // Get the number of items in the list.
            int numItems = this.listView2.Items.Count;

            // Turn off display while data is repoplulated.
            this.listView2.BeginUpdate();

            // Populate an ArrayList with a SortWrapper of each list item.
            ArrayList SortArray = new ArrayList();
            for (int i = 0; i < numItems; i++)
            {
                SortArray.Add(new SortWrapper(this.listView2.Items[i], e.Column));
            }

            // Sort the elements in the ArrayList using a new instance of the SortComparer
            // class. The parameters are the starting index, the length of the range to sort,
            // and the IComparer implementation to use for comparing elements. Note that
            // the IComparer implementation (SortComparer) requires the sort
            // direction for its constructor; true if ascending, othwise false.
            SortArray.Sort(0, SortArray.Count, new SortWrapper.SortComparer(clickedCol.ascending));

            // Clear the list, and repopulate with the sorted items.
            this.listView2.Items.Clear();
            for (int i = 0; i < numItems; i++)
                this.listView2.Items.Add(((SortWrapper)SortArray[i]).sortItem);

            // Turn display back on.
            this.listView2.EndUpdate();
        }
        /*private void CompareData()
        {
            tabPage2.Invoke((MethodInvoker)delegate
            {
                tabPage2.Text = "Ждемс. Не нервничаем...";
            });
            localDB.Open();
            OleDbCommand localsql = new OleDbCommand("SELECT * FROM SM", localDB);
            OleDbDataReader localrdr = localsql.ExecuteReader();
            while (localrdr.Read())
            {
                OleDbCommand erase = new OleDbCommand("DELETE * FROM PROTOCOL WHERE (entry='" + localrdr[1].ToString() + "' and actiontime>=CDate('" + localrdr[2].ToString() + "') and actiontime<=CDate('" + localrdr[3].ToString() + "'))", localDB);
                erase.ExecuteNonQuery();
            }
            localDB.Close();
            localDB.Open();
            OleDbCommand rayscmd = new OleDbCommand("SELECT * FROM PROTOCOL ORDER BY objid, actiontime" ,localDB);
            OleDbDataReader rdr = rayscmd.ExecuteReader();
            int previous = 0;
            
            DateTime prevtime = DateTime.Parse("1986-06-08");
            dateTimePicker1.Invoke((MethodInvoker)delegate
            {
                prevtime = dateTimePicker1.Value;
            });
            string RaysType = string.Empty;
            string val = string.Empty;
            comboBox1.Invoke((MethodInvoker)delegate 
            { 
                val = comboBox1.SelectedItem.ToString();
            });
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\aydnep IT-consulting\\Objects\\" + val);
            RaysType = key.GetValue("RaysType").ToString();
            while (rdr.Read())
            {
                if (Convert.ToInt32(rdr[1].ToString()) == previous) // такой же как и предыдущий?
                {
                    previous = Convert.ToInt32(rdr[1].ToString());
                    if (rdr[2].ToString() == RaysType) // ВКЛ
                        {
                            if ((DateTime.Parse(rdr[3].ToString()) - prevtime) > Properties.Settings.Default.disarm) // попався
                            {
                                listView2.Invoke((MethodInvoker)delegate
                                {
                                    listView2.Items.Add(rdr[0].ToString());
                                    listView2.Items[listView2.Items.Count - 1].BackColor = Color.Pink;
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(rdr[1].ToString());
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОСТОЙ");
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(DateTime.Parse(rdr[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(prevtime.ToString("yyyy-MM-dd HH:mm:ss")); 
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add((DateTime.Parse(rdr[3].ToString()) - prevtime).ToString()); 
                                }); 
                            } //не попався
                        }
                        else // ВЫКЛ
                        {
                            if ((DateTime.Parse(rdr[3].ToString()) - prevtime) > Properties.Settings.Default.arm) // попався
                            {
                                listView2.Invoke((MethodInvoker)delegate
                                {
                                    listView2.Items.Add(rdr[0].ToString());
                                    listView2.Items[listView2.Items.Count - 1].BackColor = Color.DeepPink;
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(rdr[1].ToString());
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add("ТРЕВОГА");
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(DateTime.Parse(rdr[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add(prevtime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    listView2.Items[listView2.Items.Count - 1].SubItems.Add((DateTime.Parse(rdr[3].ToString()) - prevtime).ToString());
                                }); 
                            } //не попався
                        } // ФКЛ/ФЫКЛ
                    prevtime = DateTime.Parse(rdr[3].ToString());
                    previous = Convert.ToInt32(rdr[1].ToString());
                }
                else // не такой как предыдущий
                {
                    dateTimePicker1.Invoke((MethodInvoker)delegate
                    {
                        prevtime = dateTimePicker1.Value;
                    });
                    if (rdr[2].ToString() == RaysType) // ВКЛ
                    {
                        if ((DateTime.Parse(rdr[3].ToString()) - prevtime) > Properties.Settings.Default.disarm) // попався
                        {
                            listView2.Invoke((MethodInvoker)delegate
                            {
                                listView2.Items.Add(rdr[0].ToString());
                                listView2.Items[listView2.Items.Count - 1].BackColor = Color.Pink;
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(rdr[1].ToString());
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ПРОСТОЙ");
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(DateTime.Parse(rdr[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(prevtime.ToString("yyyy-MM-dd HH:mm:ss"));
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add((DateTime.Parse(rdr[3].ToString()) - prevtime).ToString());
                            });
                        } //не попався
                    }
                    else // ВЫКЛ
                    {
                        if ((DateTime.Parse(rdr[3].ToString()) - prevtime) > Properties.Settings.Default.arm) // попався
                        {
                            listView2.Invoke((MethodInvoker)delegate
                            {
                                listView2.Items.Add(rdr[0].ToString());
                                listView2.Items[listView2.Items.Count - 1].BackColor = Color.DeepPink;
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(rdr[1].ToString());
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add("ТРЕВОГА");
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(DateTime.Parse(rdr[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add(prevtime.ToString("yyyy-MM-dd HH:mm:ss"));
                                listView2.Items[listView2.Items.Count - 1].SubItems.Add((DateTime.Parse(rdr[3].ToString()) - prevtime).ToString());
                            });
                        } //не попався
                    } // ФКЛ/ФЫКЛ
                    prevtime = DateTime.Parse(rdr[3].ToString());
                    previous = Convert.ToInt32(rdr[1].ToString());
                } // предыдущие
            } // end while reader read
            localDB.Close();
            tabPage2.Invoke((MethodInvoker)delegate
            {
                tabPage2.Text = "Лучи";
            });

        }*/


        private void listView23_DoubleClick(object sender, EventArgs e)
        {
            listView2.Items[listView2.SelectedIndices[0]].BackColor = Properties.Settings.Default.processedEventColor;
            listView2.Items[listView2.SelectedIndices[0]].ForeColor = Color.White;
            ChangeSelectedR();
            Hunt.Show(this);
        }


        private void listView23_Resize(object sender, EventArgs e)
        {
            if (startup)
            {
                listView2.Columns[0].Width = listView2.Width / 8;
                listView2.Columns[1].Width = listView2.Width / 8;
                listView2.Columns[2].Width = listView2.Width / 8;
                listView2.Columns[3].Width = listView2.Width / 8;
                listView2.Columns[4].Width = listView2.Width / 8;
                listView2.Columns[5].Width = listView2.Width / 8;
            }
        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s " + openFileDialog1.FileName);
            regeditProcess.WaitForExit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Цвет легальных событий!");
            colorDialog1.Color = Properties.Settings.Default.legalEventColor;
            colorDialog1.ShowDialog();
            Properties.Settings.Default.legalEventColor = colorDialog1.Color;
            MessageBox.Show("Цвет нелегальных событий!");
            colorDialog1.Color = Properties.Settings.Default.illegalEventColor;
            colorDialog1.ShowDialog();
            Properties.Settings.Default.illegalEventColor = colorDialog1.Color;
            MessageBox.Show("Цвет обработанных событий!");
            colorDialog1.Color = Properties.Settings.Default.processedEventColor;
            colorDialog1.ShowDialog();
            Properties.Settings.Default.processedEventColor = colorDialog1.Color;
        }

    }

    // An instance of the SortWrapper class is created for
    // each item and added to the ArrayList for sorting.
    public class SortWrapper
    {
        internal ListViewItem sortItem;
        internal int sortColumn;


        // A SortWrapper requires the item and the index of the clicked column.
        public SortWrapper(ListViewItem Item, int iColumn)
        {
            sortItem = Item;
            sortColumn = iColumn;
        }

        // Text property for getting the text of an item.
        public string Text
        {
            get
            {
                return sortItem.SubItems[sortColumn].Text;
            }
        }

        // Implementation of the IComparer
        // interface for sorting ArrayList items.
        public class SortComparer : IComparer
        {
            bool ascending;

            // Constructor requires the sort order;
            // true if ascending, otherwise descending.
            public SortComparer(bool asc)
            {
                this.ascending = asc;
            }

            // Implemnentation of the IComparer:Compare
            // method for comparing two objects.
            public int Compare(object x, object y)
            {
                SortWrapper xItem = (SortWrapper)x;
                SortWrapper yItem = (SortWrapper)y;

                string xText = xItem.sortItem.SubItems[xItem.sortColumn].Text;
                string yText = yItem.sortItem.SubItems[yItem.sortColumn].Text;
                return xText.CompareTo(yText) * (this.ascending ? 1 : -1);
            }
        }
    }
    // The ColHeader class is a ColumnHeader object with an
    // added property for determining an ascending or descending sort.
    // True specifies an ascending order, false specifies a descending order.
    public class ColHeader : ColumnHeader
    {
        public bool ascending;
        public ColHeader(string text, int width, HorizontalAlignment align, bool asc)
        {
            this.Text = text;
            this.Width = width;
            this.TextAlign = align;
            this.ascending = asc;
        }
    }

}

        