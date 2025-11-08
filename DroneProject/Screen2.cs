using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DroneProject.DataStructres;
using Microsoft.Azure.Cosmos;

namespace DroneProject
{
    public partial class Screen2 : Form
    {
        private Drone current;
        private Microsoft.Azure.Cosmos.Container tableObj;
        private string DbName;
        private string TableName;


        public Screen2()
        {
            InitializeComponent();
        }

        public Screen2(string db, string table, Drone currentDrone, Microsoft.Azure.Cosmos.Container tableObj)
        {
            InitializeComponent();
            this.tableObj = tableObj;
            current=currentDrone;
            DbName = db;
            TableName = table;
            var json = new JsonSerializerOptions { WriteIndented = true, };
            richTextBox_DataSwitch.Text = System.Text.Json.JsonSerializer.Serialize(current, json);
        }

        private async void button_Replace_Click(object sender, EventArgs e)
        {
            try
            {
                Drone update =  System.Text.Json.JsonSerializer.Deserialize<Drone>(richTextBox_DataSwitch.Text);
                await tableObj.ReplaceItemAsync<Drone>(update, update.id);
                MessageBox.Show("The changes were made successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }

            this.Close();
        }

        private async void button_Delete_Click(object sender, EventArgs e)
        {
        
            try
            {
                await tableObj.DeleteItemAsync<Drone>(current.id, new Microsoft.Azure.Cosmos.PartitionKey(current.id));
                MessageBox.Show("Deletion completed successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
             
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot be deleted.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            this.Close();
        }
    }
}
