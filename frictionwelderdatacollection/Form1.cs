using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Logix;
using MySql.Data.MySqlClient;


namespace FrictionWelderDataCollection
{
    public partial class Form1 : Form
    {
        PeerMessage peerMsg = new PeerMessage();
        
        string defaultSaveDirectory = "";
        Dictionary<string, WeldSchedule> weldScheduleDictionary = new Dictionary<string, WeldSchedule>();

        int messageReceivedCount = 0;
        int successCount = 0;

        public Form1()
        {
            InitializeComponent();
            defaultSaveDirectory = @"C:\Friction Weld Data\Weld Graphs\";
            

            if (!Directory.Exists(defaultSaveDirectory))
            {
                Directory.CreateDirectory(defaultSaveDirectory);
            }


            peerMsg.Received += new EventHandler(peerMsg_Received);
            listen();

        }

        private void listen()
        {

            try
            {

                peerMsg.IPAddressNIC = "";
                peerMsg.Protocol = PeerMessage.MSGPROTOCOL.CIP;
                peerMsg.Connections = 40;
                peerMsg.Listen();

            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",listen," + ex.Message.ToString();
                logData(errorString);
            }
        }

        private void peerMsg_Received(object sender, EventArgs e)
        {
            try
            {
                //////////////////////////////////////////////////
                // since tag_changed is being called from the plcUpdate
                // thread, we need to ceated a delegate to handle the UI
                if (InvokeRequired)
                    Invoke(new MsgReceivedDelegate(MsgReceived), new object[] { (MessageEventArgs)e });
                else
                    MsgReceived((MessageEventArgs)e);
            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",peerMsg_Received," + ex.Message.ToString();
                logData(errorString);
            }
        }

        /// <summary>
        /// Delegate for the UI update for the 
        /// </summary>
        /// <param name="e">
        /// </param>
        delegate void MsgReceivedDelegate(MessageEventArgs e);

        /// <summary>
        /// Update listView with unsolicited messages
        /// </summary>
        private void MsgReceived(MessageEventArgs e)
        {
            try
            {
                if (messageReceivedCount > 32000)
                {
                    messageReceivedCount = 0;
                }
                messageReceivedCount++;
                messageRecievedLabel.Text = "Messages Received: " + messageReceivedCount.ToString();
                string ipAddress = e.IPSender.ToString();

                Array sArray = (Array)e.Value;

                string serialNumber = extractString(sArray, 5, 6);
                string cell_Id = extractString(sArray, 0, 5);

                if (serialNumber == "graph_data_" && sArray.Length == 32)
                {
                    sendGraphDataToDatabase(cell_Id, getGraphDataFromArray(sArray, 12));
                }
                else
                {
                    WeldSchedule ws = new WeldSchedule();
                    ws.rpm = Convert.ToInt32(sArray.GetValue(12));
                    ws.scrubPressure = Convert.ToInt32(sArray.GetValue(13));
                    ws.burnPressure = Convert.ToInt32(sArray.GetValue(14));
                    ws.forgePressure = Convert.ToInt32(sArray.GetValue(15));
                    ws.scrubTime = Convert.ToDouble(sArray.GetValue(16)) * .01;
                    ws.burnTime = Convert.ToDouble(sArray.GetValue(17)) * .01;
                    ws.forgeTime = Convert.ToDouble(sArray.GetValue(18)) * .01;

                    getData(ipAddress, serialNumber, ws, cell_Id);
                }


                
            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",MsgReceived," + ex.Message.ToString();
                logData(errorString);
            }

        }

        private string[] getGraphDataFromArray(Array data, int position)
        {
            string[] myArray = new string[10];
            int arrayPointer = 0;
            for (int i = position; i < position + 20; i++)
            {
                if ((i%2)==1)
                {
                    myArray[arrayPointer] = data.GetValue(i - 1).ToString() + "." + data.GetValue(i).ToString();
                    arrayPointer++;
                }
            }

            return myArray;

        } 

        private void getData(string ipAddress, string serialNumber, WeldSchedule weldSchedule, string cell_ID)
        {

            try
            {
                const int Number_Of_Data_Points = 1000;
                string fileName = @"WeldGraphSettings.txt";
                var allLines = File.ReadAllLines(fileName);

                Controller plc = new Controller(ipAddress);
                plc.Connect();

                Tag rpmTag = new Tag(allLines[0]);
                rpmTag.DataType = Logix.Tag.ATOMIC.DINT;
                rpmTag.Length = Number_Of_Data_Points;
                plc.ReadTag(rpmTag);

                Array rpmArray = (Array)rpmTag.Value;

                Logix.Tag upsetTag = new Logix.Tag(allLines[2]);
                upsetTag.DataType = Logix.Tag.ATOMIC.REAL;
                upsetTag.Length = Number_Of_Data_Points;
                plc.ReadTag(upsetTag);
                Array upsetArray = (Array)upsetTag.Value;

                Logix.Tag pressureTag = new Logix.Tag(allLines[1]);
                pressureTag.Length = Number_Of_Data_Points;
                pressureTag.DataType = Logix.Tag.ATOMIC.DINT;
                plc.ReadTag(pressureTag);

                Array pressureArray = (Array)pressureTag.Value;

                saveResults(rpmArray, pressureArray, upsetArray, serialNumber, weldSchedule, cell_ID);

                if (checkBox1.Checked)
                {
                    sendToDatabase(rpmArray, pressureArray, upsetArray, serialNumber, weldSchedule, cell_ID);
                }

                plc.Disconnect();
            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",getData," + ex.Message.ToString();
                logData(errorString);
            }
        }

        private void saveResults(Array rpm, Array pressure, Array upset, string serialNumber, WeldSchedule weldSchedule, string cell_ID)
        {
            try
            {
                var now = DateTime.Now;
                StringBuilder mySB = new StringBuilder();
                mySB.AppendLine(rpm.Length.ToString());
                mySB.AppendLine(serialNumber + ", ," + now.ToString("MM-dd-yyyy hh:mm:ss tt"));


                var dateString = now.ToString("MM_dd_yyyy");
                var workingDirectory = "";


                if (File.Exists(@"C:\Friction Weld Data\saveFiles.txt"))
                {
                    var sF = File.ReadLines(@"C:\Friction Weld Data\saveFiles.txt");

                    foreach (var f in sF)
                    {
                        if (Directory.Exists(f))
                        {
                            workingDirectory = f;
                        }
                    }
                }
                else
                {
                    workingDirectory = defaultSaveDirectory;
                }


                string savePath = workingDirectory + @"\" + cell_ID + @"\" + dateString + @"\";

                if (!Directory.Exists(savePath))
                {
                    try
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    catch (Exception)
                    {

                    }
                    
                }


                int folderPointer = Directory.GetDirectories(savePath).Length;

                if (weldScheduleDictionary.ContainsKey(cell_ID))
                {
                    WeldSchedule weldScheduleStorage = weldScheduleDictionary[cell_ID];
                    if (!weldSchedule.compare(weldScheduleDictionary[cell_ID]))
                    {
                        folderPointer++;
                        weldScheduleDictionary[cell_ID] = weldSchedule;
                    }
                }
                else
                {
                    folderPointer++;
                    weldScheduleDictionary[cell_ID] = weldSchedule;
                }

                if (folderPointer == 0)
                {
                    folderPointer++;
                }

                string folderString = (folderPointer < 10) ? "0" + folderPointer.ToString() : folderPointer.ToString();

                savePath += folderString + @"\";

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }


                string saveName = serialNumber + ".csv";

                string filePath = savePath + saveName;

                //save weld schedule for current part
                mySB.AppendLine("");
                mySB.AppendLine("" + "," + "Weld Schedule");
                mySB.AppendLine("Burn Mode, Burn Mode");
                mySB.AppendLine("Forge Pressure," + weldSchedule.ForgePressure);
                mySB.AppendLine("Burn Pressure," + weldSchedule.BurnPressure);
                mySB.AppendLine("Scrub Pressure," + weldSchedule.ScrubPressure);
                mySB.AppendLine("Forge Time," + weldSchedule.ForgeTime);
                mySB.AppendLine("Burn Time," + weldSchedule.BurnTime);
                mySB.AppendLine("Scrub Time," + weldSchedule.ScrubTime);
                mySB.AppendLine("Weld Speed," + weldSchedule.RPM);


                //save weld results for current part
                mySB.AppendLine("");
                mySB.AppendLine("" + "," + "Weld Results");
                mySB.AppendLine("Pre Bond Gap Position ,  Pre Bond Gap Position  in");
                mySB.AppendLine("Pre Bond Part Contact ,  Pre Bond Part Contact  in");
                mySB.AppendLine("Total Weld Time, Weld Time  sec");
                mySB.AppendLine("Total Upset , Total Upset  in");
                mySB.AppendLine("Forge Time ,  Forge Time  sec");
                mySB.AppendLine("Forge Upset , Forge Upset  in");
                mySB.AppendLine("Forge Pressure , Forge Pressure  psi");
                mySB.AppendLine("Forge Speed , Forge Speed RPM");
                mySB.AppendLine("Burn Time , Burn Time  sec");
                mySB.AppendLine("Scrub Time , Scrub Time sec");
                mySB.AppendLine("Burn Upset , Burn Upset  in");
                mySB.AppendLine("Burn Pressure   Burn Pressure  psi");
                mySB.AppendLine("Burn Speed Burn Speed RPM");
                mySB.AppendLine("Scrub Upset Scrub Upset  in");
                mySB.AppendLine("Scrub Pressure , Scrub Pressure  psi");
                mySB.AppendLine("Scrub Speed , Scrub Speed RPM");


                //save weld graph for current part
                mySB.AppendLine("");
                mySB.AppendLine("RPM,Pressure,Upset");
                for (int i = 0; i < rpm.Length; i++)
                {
                    mySB.AppendLine(Convert.ToInt32(rpm.GetValue(i)).ToString() + "," + Convert.ToInt32(pressure.GetValue(i)).ToString() + "," + Convert.ToDouble(upset.GetValue(i)).ToString());
                }


                File.WriteAllText(filePath, mySB.ToString());

                if (successCount > 32000)
                {
                    successCount = 0;

                }
                successCount++;
                successCountLabel.Text = "Successful Transactions: " + successCount.ToString();
            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",saveResults," + ex.Message.ToString();
                logData(errorString);
            }

        }

        int tempCount = 0;
        private void button1_Click(object sender, EventArgs e)
        {

            //var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
            //var errorString = d + ",listen," + "This is a test error message";
            //logData(errorString);

            int[] myArray = new int[32];
            myArray[0] = 18739;
            myArray[1] = 12857;
            myArray[2] = 12593;
            myArray[3] = 0;
            myArray[4] = 0;
            myArray[5] = 19505;
            myArray[6] = 13383;
            myArray[7] = 12594;
            myArray[8] = 13105;
            myArray[9] = 12851;
            myArray[10] = 13312;
            myArray[11] = 0;
            myArray[12] = 50;
            myArray[13] = 70;
            myArray[14] = 45;
            myArray[15] = 0;
            myArray[16] = 60;
            myArray[17] = 0;
            myArray[18] = 52;
            myArray[19] = 50;
            myArray[20] = 55;
            myArray[21] = 50;
            myArray[22] = 0;
            myArray[23] = 0;
            myArray[24] = 0;
            myArray[25] = 0;
            myArray[26] = 0;
            myArray[27] = 0;
            myArray[28] = 0;
            myArray[29] = 0;
            myArray[30] = 0;
            myArray[31] = 0;

            Array thisArray = (Array)myArray;

            var cell = extractString(thisArray, 0, 5);
            var serial = "graph_data_";
            tempCount++;
            //serial += tempCount.ToString();
            if ((tempCount % 2) == 0)
            {
                cell += tempCount.ToString();
            }

            if (serial == "graph_data_")
            {
                //var anArray = getGraphDataFromArray(thisArray, 12);
                sendGraphDataToDatabase(cell, getGraphDataFromArray(thisArray, 12));
            }
            else
            {
                WeldSchedule ws = new WeldSchedule();
                ws.rpm = Convert.ToInt32(thisArray.GetValue(12));
                ws.scrubPressure = Convert.ToInt32(thisArray.GetValue(13));
                ws.burnPressure = Convert.ToInt32(thisArray.GetValue(14));
                ws.forgePressure = Convert.ToInt32(thisArray.GetValue(15));
                ws.scrubTime = Convert.ToDouble(thisArray.GetValue(16)) * .01;
                ws.burnTime = Convert.ToDouble(thisArray.GetValue(17)) * .01;
                ws.forgeTime = Convert.ToDouble(thisArray.GetValue(18)) * .01;

                getData("10.50.192.50", serial, ws, cell);
            }


            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                peerMsg.ShutDown();
            }
            catch (System.Exception ex)
            {
                var d = DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt");
                var errorString = d + ",FormClosing," + ex.Message.ToString();
                logData(errorString);
            }
        }

        private void logData(string dataToLog)
        {
            var fileName = @"C:\Friction Weld Data\log.csv";
            if (!File.Exists(fileName))
            {
                var title = "Log Files";
                File.WriteAllText(fileName, title);
            }

            var allLines = File.ReadLines(fileName);
            StringBuilder sb = new StringBuilder();
            foreach (var line in allLines)
            {
                sb.AppendLine(line);
            }
            sb.AppendLine(dataToLog);
            File.WriteAllText(fileName, sb.ToString());
        }

        private void sendToDatabase(Array rpm, Array pressure, Array upset, string serialNumber, WeldSchedule weldSchedule, string cell_ID)
        {
            var now = DateTime.Now;
            serialNumber += "_" + now.ToString("MM-dd-yyyy hh:mm:ss tt");

            string rpm_string = "";
            string pressure_string = "";
            string upset_string = "";

            for (int i = 0; i < rpm.Length; i++)
            {
                if (i == rpm.Length - 1)
                {
                    rpm_string += Convert.ToInt32(rpm.GetValue(i)).ToString();
                    pressure_string += Convert.ToInt32(pressure.GetValue(i)).ToString();
                    upset_string += Convert.ToDouble(upset.GetValue(i)).ToString();
                }
                else
                {
                    rpm_string += Convert.ToInt32(rpm.GetValue(i)).ToString() + ",";
                    pressure_string += Convert.ToInt32(pressure.GetValue(i)).ToString() + ",";
                    upset_string += Convert.ToDouble(upset.GetValue(i)).ToString() + ",";
                }

            }

            string ws = "";

            ws += "Forge Pressure," + weldSchedule.ForgePressure + ",";
            ws += "Burn Pressure," + weldSchedule.BurnPressure + ",";
            ws += "Scrub Pressure," + weldSchedule.ScrubPressure + ",";
            ws += "Forge Time," + weldSchedule.ForgeTime + ",";
            ws += "Burn Time," + weldSchedule.BurnTime + ",";
            ws += "Scrub Time," + weldSchedule.ScrubTime + ",";
            ws += "Weld Speed," + weldSchedule.RPM;

            string server = "10.50.200.133";
            string database = "graph_db";
            string uid = "newuser";
            string password = "newpassword";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";


            MySqlConnection myDatabase = new MySqlConnection(connectionString);

            var query = "UPDATE weld_graph SET rpm='"+rpm_string+"', pressure='"+pressure_string+"', upset='"+upset_string+"', serial_number='"+serialNumber+"', weld_schedule='"+ws+"' WHERE cell_id='"+cell_ID+"'";

            myDatabase.Open();

            MySqlCommand cmd = new MySqlCommand(query, myDatabase);

            cmd.ExecuteNonQuery();

            myDatabase.Close();
        }

        private void sendGraphDataToDatabase(string cell_id, string[] data)
        {

            string server = "10.50.71.154";
            string database = "new_schema";
            string uid = "root";
            string password = "jobb7116";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";


            MySqlConnection myDatabase = new MySqlConnection(connectionString);

            var query = "DELETE FROM graph_data WHERE cell_id='" + cell_id + "' AND id < (SELECT min(tempTable.id) FROM (SELECT id FROM graph_data WHERE cell_id='" + cell_id + "' ORDER BY id DESC LIMIT 9)tempTable);";
            query += "INSERT INTO graph_data (cell_id, val_0, val_1, val_2, val_3, val_4, val_5, val_6, val_7, val_8, val_9) VALUES ('";
            query += cell_id + "', '";
            query += data[0] + "', '";
            query += data[1] + "', '";
            query += data[2] + "', '";
            query += data[3] + "', '";
            query += data[4] + "', '";
            query += data[5] + "', '";
            query += data[6] + "', '";
            query += data[7] + "', '";
            query += data[8] + "', '";
            query += data[9] + "')";

            Console.WriteLine(query);

            myDatabase.Open();



            MySqlCommand cmd = new MySqlCommand(query, myDatabase);

            cmd.ExecuteNonQuery();

            myDatabase.Close();

        }

        private string extractString(Array data, int position, int length)
        {

            string stringToReturn = "";
            for (int i = position; i < (position + length); i++)
            {
                int x = Convert.ToInt32(data.GetValue(i));
                int a = x / 256;
                int b = x - (a * 256);

                if (a > 0)
                {
                    stringToReturn += (char)a;
                }
                if (b > 0)
                {
                    stringToReturn += (char)b;
                }

            }

            return stringToReturn;

        }




    }


}
