using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Threading;
using System.ServiceModel;
using System.Xml.Linq;
using System.Data.SqlTypes;
using Newtonsoft.Json;
using wcfServer.Services;
using System.Text.Json.Nodes;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace ECR_Test_Application
{

    [ServiceContract]
    public interface IPaymentService
    {

        [OperationContract]
        string sendRequest(string request);

        [OperationContract]
        string getPosStatus();

    }
    public partial class ECR_Test_Application : Form
    {
        string uri;
        NetTcpBinding binding;
        private int timeLeft;
        ChannelFactory<IPaymentService> channel;
        //State Flag is false at begining, after sending request to POS set it to true
        //and After Getting response from POS set it to false
        bool waiting = false;
        string pat = "123456789";

        public ECR_Test_Application()
        {
            InitializeComponent();
            uri = "net.tcp://localhost:8080/IPCS";
            binding = new NetTcpBinding(SecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            binding.SendTimeout = TimeSpan.FromMinutes(5);
            channel = new ChannelFactory<IPaymentService>(binding);
            button2.BackColor = Color.Yellow;
            StartDetectionTimer();
            InitializeForm();
        }

        //async await function
        private async Task<string> SendRequestAsync(string uniqueTransId, string paymentType, float amount, long tip, string remarks, string pat)
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            var response = "";
            try
            {
                response = await Task.Run(() => proxy?.sendRequest(CreateTransRequest(uniqueTransId, paymentType, amount, tip, remarks, pat)));
            }
            catch (Exception ex)
            {
                response = "Service Time Out." + ex.Message;

            }

            return response;
        }
            private async Task<string> SendRestartRequestAsync( string paymentType)
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            string response = "";
            this.Enabled = false;
            try
            {
                var requestTask =  Task.Run(() => proxy?.sendRequest(CreateRestartRequest(paymentType)));
                // Task to delay for the timeout period  
                var timeoutTask = Task.Delay(2000);
                // Wait for either the request to complete or the timeout period to elapse
                var completedTask = await Task.WhenAny(requestTask, timeoutTask);
                if (completedTask == requestTask)
                {
                    // If the request completed within the timeout, get the result
                    response = await requestTask;
                    ShowMessageAndStartTimer();
                    if (response == null)
                        throw new InvalidOperationException("The proxy returned null.");
                }
                else
                {
                    lblCountdown.Text = "Waiting for operation...";
                    // Handle timeout scenario
                   // response = "Request timed out.";
                    lblCountdown.Visible = true;
                    MessageBox.Show("The Service is yet to Start Please start it.", "Timeout Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);                   
                }
            }
            catch (EndpointNotFoundException enf)
            {
                response = "Could not connect to the service. Please check the service status and network connection.";
                MessageBox.Show(response, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetApplicationForRetry(); 
            }
            catch (Exception ex)
            {
                response = "Restart operation failed: " + ex.Message;
                MessageBox.Show(response, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Enabled = true;
            return response;
        }
        private void ResetApplicationForRetry()
        {
            button4.Enabled = true;
        }
        string CreateRestartRequest(string paymentType)
        {
            var request = new
            {
                transType = paymentType,
            };

            string jsonString = JsonConvert.SerializeObject(request);
            return jsonString;
        }

        string CreateTransRequest(string uniqueTransId, string paymentType, float amount, long tip, string remark, string pat)
        {
            TransRequestJson NewMessage = new TransRequestJson
            {
                transId = uniqueTransId,

                transType = paymentType,
                amount = amount,
                tip = tip,
                remark = remark,
                pat = pat

            };
            string jsonString = JsonConvert.SerializeObject(NewMessage);
            return jsonString;
        }
        private bool validate()
        {
            float v; 
            if (!string.IsNullOrEmpty(txtAmount.Text) && float.TryParse(txtAmount.Text, out v))
            {
                return true; 
            }
            else
            {
                return false; 
            }
        }


        //Sale payment
        private void button1_Click(object sender, EventArgs e)
        {
            proceedTransaction("sale");

        }

        //Fonepay payment
        private void button1_Click_1(object sender, EventArgs e)
        {
            proceedTransaction("fonepay");

            #region extra codes
            //SaleResponseJson saleResponse = JsonConvert.DeserializeObject<SaleResponseJson>(r);

            //Get all services in windows. 
            /*ServiceController[] scServices;
            scServices = ServiceController.GetServices();

            foreach (ServiceController scTemp in scServices)
            {

                if (scTemp.ServiceName == "IPCS")
                {
                    ServiceController sc = new ServiceController("IPCS");
                    Console.WriteLine("Status = " + sc.Status);
                    lblStatus.Text = sc.Status.ToString();
                    Console.WriteLine("Can Pause and Continue = " + sc.CanPauseAndContinue);
                    Console.WriteLine("Can ShutDown = " + sc.CanShutdown);
                    Console.WriteLine("Can Stop = " + sc.CanStop);
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        while (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            Thread.Sleep(1000);
                            sc.Refresh();
                        }
                    }
                }
                else
                {

                }
            }
            */
            #endregion
        }

        //Nepalpay payment
        private void button1_Click_2(object sender, EventArgs e)
        {

            proceedTransaction("nepalpay");

        }
        //for restart
        private async void button4_Click(object sender, EventArgs e)
        {
            string paymentType = "restart";
            var response = await SendRestartRequestAsync(paymentType);
            MessageBox.Show(response, "Notification");
        }

        //CountDown timer
        private void countdownTimer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                
                lblCountdown.Text = $"{timeLeft } seconds remaining";
                timeLeft--;
            }
            else
            {
                countdownTimer.Stop();
                lblCountdown.Text = "Service restarted Successfully!";
                // Additional actions after countdown
            }
        }

        private void ShowMessageAndStartTimer()
        {
            StartTimer();
        }
        private void StartTimer()
        {
            if (!countdownTimer.Enabled) 
            {
                timeLeft = 16; 
                lblCountdown.Text = $"{timeLeft} seconds remaining";
                countdownTimer.Start(); 
            }
        }

        //Cash payment
        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private async void proceedTransaction(string paymentType)
        {
            txtMessage.Text = "";
            lblstatus.Text = "";
                Random rnd = new Random();
                int uniqueTransId = rnd.Next();
                txtUniqueTransId.Text = uniqueTransId.ToString();
          //  Debug.WriteLine($"Current txtAmount.Text: '{txtAmount.Text}'");

            //Validate the amount
             if (!validate())
             {
                  MessageBox.Show("Please enter valid amount.");
                  return;
              }
                this.Enabled = false;
                waiting = true;
                var response = await SendRequestAsync(uniqueTransId.ToString(), paymentType, long.Parse(txtAmount.Text.ToString()) * 100, 0, "", pat);
                CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);

            if (_CommonJson != null)
            {


                //Handle response
                if (_CommonJson.resultcode == "000")
                {
                    lblstatus.Text = "Payment Successful";
                    txtAmount.Text = "";
                    cbItems.Focus();
                    t.Rows.Clear();


                }
                else if (_CommonJson.resultcode == "05")
                {
                    lblstatus.Text = _CommonJson.status;

                }
            }
            else
            {
                MessageBox.Show("Failed to process transaction please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

                txtMessage.Text = FormatCommonJson(_CommonJson);
                this.Enabled = true;
                MessageBox.Show(lblstatus.Text, "Message Box");
            
        }
        //Verify the transaction using unique transaction id

        private void btnVerifyTrans_Click(object sender, EventArgs e)
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            string response = "";
            var transType = "verification";
            string uniqueTransId = txtUniqueTransId.Text;
            if (uniqueTransId == "" )
            {
                MessageBox.Show("Please enter valid transaction id.");
                return;
            }
            try
            {
                response = proxy?.sendRequest(CreateVerifyRequest(uniqueTransId, transType, pat));
                CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);

                // Handle response
                if (_CommonJson.resultcode == "000")
                {
                    lblstatus.Text = "Payment Successful";
                    txtAmount.Text = "";
                   cbItems.Focus();
                    t.Rows.Clear();
                }
                else if (_CommonJson.resultcode == "05")
                {
                    lblstatus.Text = _CommonJson.status;
                   
                }
                else
                {
                    lblstatus.Text = "An error occurred during the transaction.";
                }

                txtMessage.Text = FormatCommonJson(_CommonJson);
                this.Enabled = true;
                MessageBox.Show(lblstatus.Text, "Transaction Status");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing response: " + ex.Message, "Error");
            }
        }
        private string FormatCommonJson(CommonJson _CommonJson)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_CommonJson.invoiceNo))
                sb.AppendLine($"Invoice Number: {_CommonJson.invoiceNo}");
                sb.AppendLine($"Message: {_CommonJson.message}");
                sb.AppendLine($"Result Code: {_CommonJson.resultcode}");
            if (_CommonJson.transactionAmount != null) // Assuming it's a nullable type; adjust as necessary
                sb.AppendLine($"Transaction Amount: {_CommonJson.transactionAmount}");
            if (!string.IsNullOrEmpty(_CommonJson.transactionDate))
                sb.AppendLine($"Transaction Date: {_CommonJson.transactionDate}");
            if (!string.IsNullOrEmpty(_CommonJson.transactionTime))
                sb.AppendLine($"Transaction Time: {_CommonJson.transactionTime}");
            if (!string.IsNullOrEmpty(_CommonJson.transactionType))
                sb.AppendLine($"Transaction Type: {_CommonJson.transactionType}");
            if (!string.IsNullOrEmpty(_CommonJson.verifyTransId))
                sb.AppendLine($"Verify Transaction ID: {_CommonJson.verifyTransId}");

            return sb.ToString();
        }

        string CreateVerifyRequest(string uniqueTransId, string tranType, string pat)
        {
            TransRequestJson verifyObject = new TransRequestJson
            {
                transId = uniqueTransId,
                transType = tranType,
                pat = pat
            };
            string jsonString = JsonConvert.SerializeObject(verifyObject);
            return jsonString;
        }

        DataTable t = new DataTable();
        double grossAmount = 0;
        private void ECR_Test_Application_Load(object sender, EventArgs e)
        {
            cbItems.SelectedIndex = 0;
            t.Columns.Add("NAME", typeof(string));
            t.Columns.Add("PRICE", typeof(string));
            t.Columns.Add("QTY", typeof(string));
            t.Columns.Add("TOTAL", typeof(double));
            dgvItems.DataSource = t;

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            double netAmount = (Double.Parse(txtPrice.Text) * Double.Parse(txtQty.Text));
            t.Rows.Add(cbItems.SelectedItem.ToString(), txtPrice.Text, txtQty.Text, netAmount);

            grossAmount += netAmount;
            txtAmount.Text = grossAmount.ToString();
        }


        private System.Threading.Timer detectionTimer;
        private bool devicePresent = false;
        private void StartDetectionTimer()
        {
            TimerCallback callback = DetectDevices;
            detectionTimer?.Dispose(); 
            detectionTimer = new System.Threading.Timer(callback, null, 0, 1000);
        }
        private void DetectDevices(object state)
        {
            UpdateDeviceConnectionStatus();
        }
        private void UpdateDeviceConnectionStatus()
        {
            // Check if handle is created and form is not disposed
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke(new Action(() => {
                    try
                    {
                        string[] availablePorts = SerialPort.GetPortNames();
                        bool isPresent = Array.IndexOf(availablePorts, "COM8") != -1;
                        if (isPresent && !devicePresent)
                        {
                            button2.BackColor = Color.Green;
                        }
                        else if (!isPresent && devicePresent)
                        {
                            button2.BackColor = Color.Red;
                        }
                        devicePresent = isPresent;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in device detection: {ex.Message}");
                    }
                }));
            }
            else
            {
                Console.WriteLine("Handle not created or form is disposed.");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            detectionTimer?.Dispose();
        }

        private async void button2_Click(object sender, EventArgs e)
        {

        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
        }
        private void InitializeForm()
        {
            // Reset text fields 
            txtMessage.Text = "";
            lblstatus.Text = "";
            txtAmount.Text = ""; 
            txtUniqueTransId.Text = "";
            txtPrice.Text = "1";
            txtQty.Text = "1";
            grossAmount = 0;

            if (dgvItems.DataSource is DataTable)
            {
                ((DataTable)dgvItems.DataSource).Clear();
            }

        }
        private void button5_Click(object sender, EventArgs e)
        {
            InitializeForm();
        }
        private void dgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void label3_Click(object sender, EventArgs e)
        {
        }
        private void txtAmount_TextChanged(object sender, EventArgs e)
        {
        }
        private void lblCountdown_Click(object sender, EventArgs e)
        {
        }

        private void cbItems_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
