using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace AP.TruckHunter.SecurOS
{
    public partial class Form1 : Form
    {
        private List<Entry> entries;
        private int eventsCount;
        private int enter;
        private int exit;
        private bool startup;

        public Form1()
        {
            InitializeComponent();
            entries = new List<Entry>();
            startup = false;
            comboBox1.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now.Date;
            dateTimePicker2.Value = DateTime.Now.Date.Add(new TimeSpan(23, 59, 59));
            try
            {
                foreach (string rayConfig in Properties.Settings.Default.Entries)
                {
                    string[] cfg = rayConfig.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    entries.Add(new Entry { entryDescription = cfg[0].Replace("\"", ""), enterRay = cfg[1], exitRay = cfg[2] });
                    comboBox1.Items.Add(cfg[0].Replace("\"", ""));
                }
                //checkBoxComboBox1.Items.AddRange(new object[] { items.ToArray() });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в файле конфигурации! Дальнейшая работа невозможна!\r\n" + ex.Message, "Ошибка!");
                Application.Exit();
            }

            /*foreach (Entry entry in entries)
            {
                MessageBox.Show(String.Format("Проезд {0}, въезд: {1}, выезд: {2}\r\n", entry.entryDescription, entry.enterRay, entry.exitRay));
            }*/

        }

        private void button1_Click(object sender, EventArgs e)
        {
            eventsCount = 0;
            exit = 0;
            enter = 0;
            label3.Text = "Событий:";
            label4.Text = "вЪездов:";
            label5.Text = "вЫездов:";
            listView1.Invoke((MethodInvoker)delegate
            {
                listView1.Clear();
                listView1.Columns.Add(new ColHeader("Проезд", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Начало", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Конец", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Направление", listView1.Width / 4, HorizontalAlignment.Left, true));
                this.listView1.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);
            });
            startup = true;

            using (NpgsqlConnection database = new NpgsqlConnection(Properties.Settings.Default.DataBase))
            {
                try
                {
                    database.Open();
                } // try
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка анализа данных!\r\n" + ex.Message, "Ошибка!");
                    return;
                }

                foreach (Entry entry in entries)
                {
                    #region Analitics

                    #region Select from protocol
                    //MessageBox.Show(comboBox1.SelectedItem + "\r\n" + entry.entryDescription);
                    if (!checkBox2.Checked || comboBox1.SelectedItem.ToString() == entry.entryDescription)
                    {
                        using (NpgsqlCommand satmActionCommand = new NpgsqlCommand())
                        {
                            bool enterRay = false;
                            bool exitRay = false;
                            bool truck = false;
                            string direction = "";
                            DateTime startDate = DateTime.Parse("1984-06-01");
                            DateTime endDate = DateTime.Parse("1986-06-08");
                            satmActionCommand.Parameters.Add("@startDate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateTimePicker1.Value;
                            satmActionCommand.Parameters.Add("@endDate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateTimePicker2.Value;
                            satmActionCommand.Parameters.Add("@enterRay", NpgsqlTypes.NpgsqlDbType.Char).Value = entry.enterRay;
                            satmActionCommand.Parameters.Add("@exitRay", NpgsqlTypes.NpgsqlDbType.Char).Value = entry.exitRay;
                            satmActionCommand.CommandText = @"SELECT objid, action, time FROM ""PROTOCOL""";
                            satmActionCommand.CommandText += " WHERE objid in (@enterRay,@exitRay)";
                            satmActionCommand.CommandText += " AND time>=@startDate";
                            satmActionCommand.CommandText += " AND time<=@endDate";
                            satmActionCommand.CommandText += " AND action IN ('OPEN', 'NORM') ORDER BY time";
                            satmActionCommand.Connection = database;
                            //log.WriteEntry(satmActionCommand.CommandText, EventLogEntryType.Information);
                            #region Try to select from protocol EXECUTEREADER
                            using (NpgsqlDataReader satmActionReader = satmActionCommand.ExecuteReader())
                            {
                                while (satmActionReader.Read())
                                {
                                    //pk.Add(satmActionReader.GetString(3));
                                    if (satmActionReader.GetString(1) == Properties.Settings.Default.RayType)
                                    {
                                        if (satmActionReader.GetString(0) == entry.enterRay)
                                        {
                                            enterRay = true;
                                            if (exitRay) // вЫезд замкнут?
                                                truck = true;
                                            else // вЫезд не замкнут
                                            {
                                                startDate = satmActionReader.GetDateTime(2);
                                                direction = "ввоз";
                                            } // вЫезд не замкнут
                                        } // Если замкнулись на вЪезд
                                        else if (satmActionReader.GetString(0) == entry.exitRay)
                                        {
                                            exitRay = true;
                                            if (enterRay) // вЪезд замкнут?
                                                truck = true;
                                            else // вЪезд не замкнут
                                            {
                                                startDate = satmActionReader.GetDateTime(2);
                                                direction = "вывоз";
                                            } // вЪезд не замкнут
                                        } // Если замкнулись на вЫезд
                                    } // Если замкнулись
                                    else //if (satmActionReader.GetString(1) != "ALARM" && satmActionReader.GetString(1) != "DISARM") // Если разомкнулись
                                    {
                                        if (satmActionReader.GetString(0) == entry.enterRay)
                                        {
                                            enterRay = false;
                                            if (!exitRay) // Если вЫезд  НЕ замкнут то 
                                            {
                                                if (truck) // Был грузовик?
                                                {
                                                    endDate = satmActionReader.GetDateTime(2);
                                                    truck = false;
                                                    if ((endDate - startDate) > Properties.Settings.Default.Delay)
                                                    {
                                                        listView1.Invoke((MethodInvoker)delegate
                                                        {
                                                            listView1.Items.Add(entry.entryDescription); // Название проезда
                                                        listView1.Items[listView1.Items.Count - 1].SubItems.Add(startDate.ToString("yyyy-MM-dd HH:mm:ss")); // начало
                                                        listView1.Items[listView1.Items.Count - 1].SubItems.Add(endDate.ToString("yyyy-MM-dd HH:mm:ss")); // конец
                                                        listView1.Items[listView1.Items.Count - 1].SubItems.Add(direction); // направление
                                                    });
                                                        eventsCount++;
                                                        if (direction == "ввоз")
                                                        {
                                                            enter++;
                                                        }
                                                        else
                                                        {
                                                            exit++;
                                                        }
                                                    } // Если длительность события больше задержки
                                                      //log.WriteEntry("Got Event", EventLogEntryType.Information);
                                                } // Был ли грузовик?
                                            } // Если вЫезд  НЕ замкнут 
                                        } // Если разомкнулись на вЪезд
                                        else if (satmActionReader.GetString(0) == entry.exitRay)
                                        {
                                            exitRay = false;
                                            if (!enterRay) // Если вЪезд  НЕ замкнут то 
                                            {
                                                //DateTime.TryParse(satmActionReader.GetString(2), out endDate);
                                                if (truck) // Был грузовик?
                                                {
                                                    endDate = satmActionReader.GetDateTime(2);
                                                    truck = false;
                                                    if ((endDate - startDate) > Properties.Settings.Default.Delay)
                                                    {
                                                        listView1.Invoke((MethodInvoker)delegate
                                                    {
                                                        listView1.Items.Add(entry.entryDescription); // Название проезда
                                                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(startDate.ToString("yyyy-MM-dd HH:mm:ss")); // начало
                                                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(endDate.ToString("yyyy-MM-dd HH:mm:ss")); // конец
                                                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(direction); // направление
                                                });
                                                        eventsCount++;
                                                        if (direction == "ввоз")
                                                        {
                                                            enter++;
                                                        }
                                                        else
                                                        {
                                                            exit++;
                                                        }
                                                    } // Если длительность события больше задержки
                                                      //log.WriteEntry("Got Event", EventLogEntryType.Information);
                                                      //InsertInto(Server, satmEntryReader.GetString(2), direction, startDate, endDate, id, carID, culture, who, legal, Guid.NewGuid());
                                                } // Был ли грузовик?
                                            } // Если вЫезд  НЕ замкнут 
                                        } // Если разомкнулись на вЫезд
                                    } // Если разомкнулись
                                } // Пока парсим лучи
                            } //Деструктор ридера событий
                            #endregion
                        } // Запрос парсера
                    } // Если выбран проезд
                    #endregion

                    #endregion
                } // foreach entry
            } // Using connection
            label3.Text = String.Format("Событий: {0}", eventsCount);
            label4.Text = String.Format("вЪездов: {0}", enter);
            label5.Text = String.Format("вЫездов: {0}", exit);
            if (eventsCount > 0)
            {
                comboBox1.Enabled = true;
            }
        }

        private void FillListView()
        {
            listView1.Invoke((MethodInvoker)delegate
            {
                listView1.Columns.Add(new ColHeader("Проезд", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Начало", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Конец", listView1.Width / 4, HorizontalAlignment.Left, true));
                listView1.Columns.Add(new ColHeader("Направление", listView1.Width / 4, HorizontalAlignment.Left, true));
                this.listView1.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);
            });

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //MessageBox.Show("Click");
            // Create an instance of the ColHeader class.
            ColHeader clickedCol = (ColHeader)this.listView1.Columns[e.Column];

            // Set the ascending property to sort in the opposite order.
            clickedCol.ascending = !clickedCol.ascending;

            // Get the number of items in the list.
            int numItems = this.listView1.Items.Count;

            // Turn off display while data is repoplulated.
            this.listView1.BeginUpdate();

            // Populate an ArrayList with a SortWrapper of each list item.
            ArrayList SortArray = new ArrayList();
            for (int i = 0; i < numItems; i++)
            {
                SortArray.Add(new SortWrapper(this.listView1.Items[i], e.Column));
            }

            // Sort the elements in the ArrayList using a new instance of the SortComparer
            // class. The parameters are the starting index, the length of the range to sort,
            // and the IComparer implementation to use for comparing elements. Note that
            // the IComparer implementation (SortComparer) requires the sort
            // direction for its constructor; true if ascending, othwise false.
            SortArray.Sort(0, SortArray.Count, new SortWrapper.SortComparer(clickedCol.ascending));

            // Clear the list, and repopulate with the sorted items.
            this.listView1.Items.Clear();
            for (int i = 0; i < numItems; i++)
                this.listView1.Items.Add(((SortWrapper)SortArray[i]).sortItem);

            // Turn display back on.
            this.listView1.EndUpdate();
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
                dateTimePicker1.Value = dateTimePicker1.Value.Date;
                dateTimePicker2.Value = dateTimePicker2.Value.Date.Add(new TimeSpan(23, 59, 59));
            }
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            if (startup)
            {
                listView1.Columns[0].Width = listView1.Width / 4;
                listView1.Columns[1].Width = listView1.Width / 4;
                listView1.Columns[2].Width = listView1.Width / 4;
                listView1.Columns[3].Width = listView1.Width / 4;
            }

        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = ((CheckBox)sender).Checked;
        }
    }
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

    internal class Entry
    {
        internal string entryDescription
        {
            get;
            set;
        }
        internal string enterRay
        {
            get;
            set;
        }
        internal string exitRay
        {
            get;
            set;
        }
    }

}
