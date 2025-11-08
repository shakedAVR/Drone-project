using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DroneProject.DataStructres;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace DroneProject
{

    public partial class Form1 : Form
    {
        private static readonly string uri = System.Configuration.ConfigurationManager.AppSettings["URI"];
        private static readonly string primary = System.Configuration.ConfigurationManager.AppSettings["PrimaryKey"];

        private static readonly string ID = System.Configuration.ConfigurationManager.AppSettings["ID"];
        private static new readonly string Name = System.Configuration.ConfigurationManager.AppSettings["Name"];
        private static readonly string Source = System.Configuration.ConfigurationManager.AppSettings["Emulator"];
        private static readonly string Email = System.Configuration.ConfigurationManager.AppSettings["Email"];
        private Microsoft.Azure.Cosmos.CosmosClient myclient;
        string[] options = { "greater than", "less than", "equal to" };
        bool exists = false;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            textBox_EndPoint.Text = uri;
            textBox_Primary.Text = primary;
            textBox_ID.Text = ID;
            textBox_Name.Text = Name;
            textBox_Source.Text = Source;
            textBox_Email.Text = Email;
            tabControl1.TabPages.Remove(tabPage4);
            tabControl1.TabPages.Remove(tabPage5);
            comboBox_SearchYear.Items.AddRange(options);
            comboBox_SearchCapacity.Items.AddRange(options);
            comboBox_SearchChargeTime.Items.AddRange(options);
            comboBox_SearchFrameRate.Items.AddRange(options);
            comboBox_SearchPrice.Items.AddRange(options);
            comboBox_SearchCapacity.SelectedIndex = 0;
            comboBox_SearchChargeTime.SelectedIndex = 0;
            comboBox_SearchPrice.SelectedIndex = 0;
            comboBox_SearchYear.SelectedIndex = 0;
            comboBox_SearchFrameRate.SelectedIndex = 0;
            groupBox_Buttons.Visible = false;
            try
            {
                myclient = new Microsoft.Azure.Cosmos.CosmosClient(uri, primary);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to the service", "Cosmos Client Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("Connection successfully completed.", "Cosmos Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dataGridView_Statiscis.DataSource = await GetALLDataStatiscis();
        }

        private async void btn_CreateInCloud_Click(object sender, EventArgs e)
        {
            string userDB = textBox_DBCreate.Text;
            string userTable = textBox_Table.Text;

            await CreateDBandTableInCloud(userDB, userTable);
        }
        private async Task CreateDBandTableInCloud(string userDB, string userTable)
        {
            Microsoft.Azure.Cosmos.DatabaseResponse dbResalt
                 = await myclient.CreateDatabaseIfNotExistsAsync(userDB);

            HttpStatusCode statusCode = dbResalt.StatusCode;

            if (statusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("the database " + userDB + "was succsecfully created",
                    "Database created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteToLog("the database " + userDB + "was succsecfully created   Database created");
            }
            else if (statusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("the database " + userDB + "was already created",
                    "Database Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteToLog("the database " + userDB + "was already create Database Exists");
            }
            else
            {
                MessageBox.Show("the database " + userDB + "Failed to be created , we got" + dbResalt,
                     "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteToLog("the database " + userDB + "Failed to be created , we got" + dbResalt + "Failure");
                return;
            }
            //step2: Table Creation
            if (string.IsNullOrEmpty(userDB)) return;

            Microsoft.Azure.Cosmos.Database dbObject = dbResalt.Database;

            Microsoft.Azure.Cosmos.ContainerResponse containerResult = await dbObject.CreateContainerIfNotExistsAsync(userTable, "/id");

            HttpStatusCode statusFromContainerCreation = containerResult.StatusCode;
            if (statusFromContainerCreation == HttpStatusCode.Created)
            {
                MessageBox.Show("the Table " + userTable + "was succsecfully created",
                    "Table created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteToLog("the Table " + userTable + "was succsecfully created" + "Table created");

            }
            else if (statusFromContainerCreation == HttpStatusCode.OK)
            {
                MessageBox.Show("the table " + userTable + "was already created",
                    "table Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteToLog("the table " + userTable + "was already created" + "table Exists");
            }
            else
            {
                MessageBox.Show("the table " + userDB + "Failed to be created , we got" + containerResult,
                     "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteToLog("the table " + userDB + "Failed to be created  we got" + containerResult + "Failure");
            }
            await uploadDataForLog();
        }
        private void WriteToLog(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                string fullMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";

                if (!File.Exists(logPath))
                {
                    using (FileStream fs = File.Create(logPath)) { }
                }

                File.AppendAllText(logPath, fullMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write to log file: " + ex.Message);
            }
        }
        public async Task uploadDataForLog()
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

                if (File.Exists(logPath))
                {
                    string content = File.ReadAllText(logPath);
                    richTextBox_LOG.Text = content;
                }
                else
                {
                    richTextBox_LOG.Text = "No log file found.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading log file: " + ex.Message);
            }
        }

        public async Task<List<StatisicsDataCloud>> GetALLDataStatiscis()
        {
            List<StatisicsDataCloud> result = new List<StatisicsDataCloud>();
            int count = 0;

            Microsoft.Azure.Cosmos.FeedIterator<DatabaseProperties> dbIterator = myclient.GetDatabaseQueryIterator<DatabaseProperties>();
            while (dbIterator.HasMoreResults)
            {
                foreach (DatabaseProperties currentDB in await dbIterator.ReadNextAsync())
                {

                    Microsoft.Azure.Cosmos.Database database = myclient.GetDatabase(currentDB.Id);

                    Microsoft.Azure.Cosmos.FeedIterator<ContainerProperties> tableIterator =
                         database.GetContainerQueryIterator<ContainerProperties>();

                    while (tableIterator.HasMoreResults)
                    {
                        foreach (ContainerProperties currentTable in await tableIterator.ReadNextAsync())
                        {
                            Microsoft.Azure.Cosmos.Container tableObj = myclient.GetContainer(currentDB.Id, currentTable.Id);
                            FeedIterator<Drone> objectIterator = tableObj.GetItemQueryIterator<Drone>();

                            while (objectIterator.HasMoreResults)
                            {

                                foreach (Drone currentItem in await objectIterator.ReadNextAsync())
                                {
                                    count++;

                                }
                            }
                            result.Add(new StatisicsDataCloud { DbName = currentDB.Id, TableName = currentTable.Id, ItemNumbers = count });
                            count = 0;
                        }

                    }
                }
            }

            if (result.Count == 0) result.Add(new StatisicsDataCloud { DbName = "empty", TableName = "empty", ItemNumbers = 0 });
            return result;
        }

        private async void dataGridView_Statiscis_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            label_DbSelcted.Text = dataGridView_Statiscis.CurrentRow.Cells["DbName"].Value.ToString();
            label_TableSelected.Text = dataGridView_Statiscis.CurrentRow.Cells["TableName"].Value.ToString();
            WriteToLog(" the Db selected: " + label_DbSelcted.Text + " the table selected: " + label_TableSelected.Text);
            if (!exists) { tabControl1.TabPages.Add(tabPage4); tabControl1.TabPages.Add(tabPage5); exists = true; }

            await uploadDataForLog();
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a JSON file";
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string data = System.IO.File.ReadAllText(openFileDialog.FileName);
                richTextBox_Json.Text = data;
            }
            else
            {
                MessageBox.Show("No file was selected", "No file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        private async void button4_Click(object sender, EventArgs e)
        {
            string db = label_DbSelcted.Text;
            string table = label_TableSelected.Text;
            Microsoft.Azure.Cosmos.Container tableObj = myclient.GetContainer(db, table);    //אתה יוצר כאן אובייקט מיקרוסופטי שמקבל מסד וטבלה
                                                                                             //ואתה מקבל את הקונטטינר שלו ואיתו אתה יכול לעשות פעולות על האייטמים
            List<Drone> drones = Drone.ConvertFromStringToListOfObject(richTextBox_Json.Text);
            string selectedActivity = radioButton_Insert.Checked ? "Insert" :
                                             radioButton_Replace.Checked ? "Replace" :
                                             radioButton_Delete.Checked ? "Delete" :
                                             null;
            if (selectedActivity == null)
            {
                MessageBox.Show("No Button Selected", "Radio Buttons", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WriteToLog("No radio button selected.");
                return;
            }
            await performActivityInCloud(db, table, selectedActivity, drones);
        }
        private async Task performActivityInCloud(string Dbs, string Tables, string selectedActivity, List<Drone> drones)
        {
            Microsoft.Azure.Cosmos.Container tableObj = myclient.GetContainer(Dbs, Tables);
            bool isDroneExists = false;
            foreach (Drone currentDrone in drones)
            {
                try
                {
                    await tableObj.ReadItemAsync<Drone>(currentDrone.id, new PartitionKey(currentDrone.id));
                    isDroneExists = true;
                }
                catch (Exception ex)
                {
                    isDroneExists = false;
                }
                if (selectedActivity == "Insert")
                {
                    if (!isDroneExists)
                    {
                        await tableObj.CreateItemAsync<Drone>(currentDrone);
                        MessageBox.Show("Data uploaded successfully", "ok", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WriteToLog("Data uploaded successfully. " + currentDrone.ToString() + " DB: " + Dbs);

                    }
                    else
                        WriteToLog("Drone already exists. " + currentDrone.ToString() + " Not Succses");
                }
                else if (selectedActivity == "Replace")
                {
                    if (isDroneExists)
                    {
                        await tableObj.ReplaceItemAsync<Drone>(currentDrone, currentDrone.id, new PartitionKey(currentDrone.id));
                        MessageBox.Show("Data was successfully exchanged..", "ok", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WriteToLog("Data was successfully exchanged.. " + currentDrone.ToString());
                    }
                    else
                        WriteToLog("Drone does NOT exist." + currentDrone.ToString());
                }
                else if (selectedActivity == "Delete")
                {
                    if (isDroneExists)
                    {
                        await tableObj.DeleteItemAsync<Drone>(currentDrone.id, new PartitionKey(currentDrone.id));
                        MessageBox.Show("Data deleted successfully...", "ok", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WriteToLog("Data deleted successfully. " + currentDrone.ToString());
                    }
                    else
                        WriteToLog("Drone does NOT exist cant deleted. " + currentDrone.ToString());

                }
            }
            await uploadDataForLog();


        }

        private async void button_Search_Click(object sender, EventArgs e)
        {
            string Id = string.IsNullOrEmpty(textBox_SearchID.Text) ? null : textBox_SearchID.Text;
            string model = string.IsNullOrEmpty(textBox_SearchModel.Text) ? null : textBox_SearchModel.Text;
            int year = string.IsNullOrEmpty(textBox_SearchYear.Text) ? 0 : Convert.ToInt32(textBox_SearchYear.Text);
            double price = string.IsNullOrEmpty(textBox_SearchPrice.Text) ? 0 : Convert.ToDouble(textBox_SearchPrice.Text);
            string type = string.IsNullOrEmpty(textBox_SearchType.Text) ? null : textBox_SearchType.Text;
            double capacity = string.IsNullOrEmpty(textBox_SearchCapacity.Text) ? 0 : Convert.ToDouble(textBox_SearchCapacity.Text);
            double chargeTime = string.IsNullOrEmpty(textBox_SearchChargeTime.Text) ? 0 : Convert.ToDouble(textBox_SearchChargeTime.Text);
            string resolution = string.IsNullOrEmpty(textBox_SearchResolution.Text) ? null : textBox_SearchResolution.Text;
            double frameRate = string.IsNullOrEmpty(textBox_SearchFrameRate.Text) ? 0 : Convert.ToDouble(textBox_SearchFrameRate.Text);
            string lensType = string.IsNullOrEmpty(textBox_SearchLensType.Text) ? null : textBox_SearchLensType.Text;
            dataGridView_Search.DataSource = await searchInCloud(Id,model,year,price,type,capacity,chargeTime,resolution,frameRate,lensType);
        }

        private async Task<object> searchInCloud(string id, string model, int year, double price, string type, double capacity, double chargeTime, string resolution, double frameRate, string lensType)
        {
            List<DroneDetails> result = new List<DroneDetails>();
            try
            {

                Microsoft.Azure.Cosmos.Container tableObj = myclient.GetContainer(label_DbSelcted.Text, label_TableSelected.Text);
                FeedIterator<Drone> DroneIterator = tableObj.GetItemQueryIterator<Drone>();
                while (DroneIterator.HasMoreResults)
                {
                    foreach (Drone currentDrone in await DroneIterator.ReadNextAsync())

                    {
                        bool flagType = false, flagCapacity = false, flagChargeTime = false;
                        bool flagRes = false, flagFrameRate = false, flagLensType = false;
                        if (currentDrone == null) continue;
                        if (id != null)
                        {
                            if (currentDrone.id == null) continue;
                            if (!id.Equals(currentDrone.id)) continue;

                        }
                        if (model != null)
                        {
                            if (currentDrone.Model == null) continue;
                            if (!model.Equals(currentDrone.Model)) continue;
                        }
                        if (year > 0)
                        {
                            if (currentDrone.Year == 0) continue;
                            if (comboBox_SearchYear.SelectedIndex == 0)//greater
                            {
                                if (currentDrone.Year < year) continue;
                            }
                            else if (comboBox_SearchYear.SelectedIndex == 1)//less than
                            {
                                if (currentDrone.Year > year) continue;
                            }
                            else if (comboBox_SearchYear.SelectedIndex == 2)  // equals
                            {
                                if (currentDrone.Year != year) continue;
                            }
                        }
                        if (price > 0)
                        {
                            if (currentDrone.Price == 0) continue;
                            if (comboBox_SearchPrice.SelectedIndex == 0)
                            {
                                if (currentDrone.Price < price) continue;
                            }
                            else if (comboBox_SearchPrice.SelectedIndex == 1)
                            {
                                if (currentDrone.Price > price) continue;
                            }
                            else if (comboBox_SearchPrice.SelectedIndex == 2)
                            {
                                if (currentDrone.Price != price) continue;
                            }
                        }
                        if (currentDrone.Batteries != null)
                        {
                           
                            foreach (Battery Currentbattery in currentDrone.Batteries)
                            {
                                if (Currentbattery == null) continue;
                                if (type != null)
                                {
                                    if (!Currentbattery.Type.Equals(type)) continue;
                                    flagType = true;

                                }
                                if (capacity > 0)
                                {
                                    if (Currentbattery.Capacity == 0) continue;
                                    if (comboBox_SearchCapacity.SelectedIndex == 0)
                                    {
                                        if (Currentbattery.Capacity < capacity) continue;
                                    }
                                    else if (comboBox_SearchCapacity.SelectedIndex == 1)
                                    {
                                        if (Currentbattery.Capacity > capacity) continue;
                                    }
                                    else if (comboBox_SearchCapacity.SelectedIndex == 2)
                                    {
                                        if (Currentbattery.Capacity != capacity) continue;
                                    }
                                    flagCapacity = true;

                                }
                                if (chargeTime > 0)
                                {
                                    if (Currentbattery.ChargeTime == 0) continue;
                                    if (comboBox_SearchChargeTime.SelectedIndex == 0)
                                    {
                                        if (Currentbattery.ChargeTime < chargeTime) continue;
                                    }
                                    else if (comboBox_SearchChargeTime.SelectedIndex == 1)
                                    {
                                        if (Currentbattery.ChargeTime > chargeTime) continue;
                                    }

                                    else if (comboBox_SearchChargeTime.SelectedIndex == 2)
                                    {
                                        if (Currentbattery.ChargeTime != chargeTime) continue;

                                    }
                                    flagChargeTime = true;





                                }
                            }
                        }
                        if (type != null && !flagType || capacity > 0 && !flagCapacity || chargeTime > 0 && !flagChargeTime) continue;
                        if (currentDrone.Cameras != null)
                        {
                            foreach (Camera CurrentCamera in currentDrone.Cameras)
                            {
                                if (CurrentCamera == null) continue;
                                if (resolution != null)
                                {
                                    if (!CurrentCamera.Resolution.Equals(resolution)) continue;
                                    flagRes = true;
                                }
                                if (frameRate > 0)
                                {
                                    if (comboBox_SearchFrameRate.SelectedIndex == 0)
                                    {
                                        if (CurrentCamera.FrameRate < frameRate) continue;
                                    }
                                    else if (comboBox_SearchFrameRate.SelectedIndex == 1)
                                    {
                                        if (CurrentCamera.FrameRate > frameRate) continue;
                                    }
                                    else if (comboBox_SearchFrameRate.SelectedIndex == 2)
                                    {
                                        if (CurrentCamera.FrameRate != frameRate) continue;
                                    }
                                    flagFrameRate = true;



                                }
                                if (lensType != null)
                                {
                                    if (!CurrentCamera.LensType.Equals(lensType)) continue;
                                    flagLensType = true;
                                }



                            }

                        }
                        if (resolution != null && !flagRes || frameRate > 0 && !flagFrameRate || lensType !=null && !flagLensType) continue;
                        if (type!=null&&currentDrone.Batteries==null|| capacity > 0 && currentDrone.Batteries == null|| chargeTime>0 && currentDrone.Batteries == null||
                            type != null && currentDrone.Batteries.Length == 0 || capacity > 0 && currentDrone.Batteries.Length == 0 || chargeTime > 0 && currentDrone.Batteries.Length == 0) continue;
                        if (resolution != null && currentDrone.Cameras == null || frameRate > 0 && currentDrone.Cameras == null || lensType!= null && currentDrone.Cameras == null||
                            resolution != null && currentDrone.Cameras.Length == 0 || frameRate > 0 && currentDrone.Cameras.Length == 0 || lensType != null && currentDrone.Cameras.Length == 0) continue;
                        result.Add(new DroneDetails {id=currentDrone.id, Dronedetails = currentDrone.ToString() });
                    }
                }
            }
            catch
            {

            }
            return result;
        }

        private async void dataGridView_Search_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string ID= dataGridView_Search.CurrentRow.Cells["id"].Value.ToString();
            string db = label_DbSelcted.Text;
            string table = label_TableSelected.Text;
            Microsoft.Azure.Cosmos.Container tableObj = myclient.GetContainer(db, table);
            Drone currentDrone = await tableObj.ReadItemAsync<Drone>(ID, new PartitionKey(ID));
            Screen2 screens = new Screen2 (db, table, currentDrone, tableObj);
            screens.ShowDialog();
        }

        private void richTextBox_Json_TextChanged(object sender, EventArgs e)
        {
            if (IsValidJson(richTextBox_Json.Text))
            {
                groupBox_Buttons.Visible = true;
            }
            else
            {
                groupBox_Buttons.Visible = false;
            }
        }
    }
}

