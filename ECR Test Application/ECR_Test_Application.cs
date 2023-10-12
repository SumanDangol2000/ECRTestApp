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
        ChannelFactory<IPaymentService> channel;
        //State Flag is false at begining, after sending request to POS set it to true
        //and After Getting response from POS set it to false
        bool waiting = false;
        string pat = "T!3@$T#P$@%T^";

        public ECR_Test_Application()
        {
            InitializeComponent();
            uri = "net.tcp://localhost:8080/IPCS";
            binding = new NetTcpBinding(SecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            binding.SendTimeout = TimeSpan.FromMinutes(5);
            channel = new ChannelFactory<IPaymentService>(binding);

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

        private bool validate()
        {
            bool value = true;
            float v;
            if (txtAmount.Text == "" || !float.TryParse(txtAmount.Text, out v))
            {
                value = false;
            }
            else
            {
                value = true;
            }
            return value;
        }

        //Sale payment
        private void button1_Click(object sender, EventArgs e)
        {
            proceedTransaction("sale");

        }

        //Fonepay payment
        private void button1_Click_1(object sender, EventArgs e)
        {
            proceedTransaction("fonePay");

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

            proceedTransaction("nepalPay");

        }

        //Cash payment
        private void button3_Click(object sender, EventArgs e)
        {
            //Write code for cash payment
        }


        private async void proceedTransaction(string paymentType)
        {
            txtMessage.Text = "";
            lblstatus.Text = "";

            //Generate random number for unique transaction ID
            Random rnd = new Random();
            int uniqueTransId = rnd.Next();
            txtUniqueTransId.Text = uniqueTransId.ToString();

            //Validate the amount
            if (!validate())
            {
                MessageBox.Show("Please enter valid amount.");
                return;
            }
            this.Enabled = false;
            waiting = true;

            //Request and Response
            var response = await SendRequestAsync(uniqueTransId.ToString(), paymentType, long.Parse(txtAmount.Text.ToString()) * 100, 0, "", pat);
            CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);

            //Handel response
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

            txtMessage.Text = response;
            this.Enabled = true;
            MessageBox.Show(lblstatus.Text, "Message Box");

        }

        //Verify the transaction using uniqye transaction id
        private void btnVerifyTrans_Click(object sender, EventArgs e)
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            var response = "";
            var transType = "verification";
            string uniqueTransId = txtUniqueTransId.Text;
            if (uniqueTransId == "" )
            {
                MessageBox.Show("Please enter valid transaction id.");
                return;
            }

            response = proxy?.sendRequest(CreateVerifyRequest(uniqueTransId, transType, pat));
            CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);

            //Handel response
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

            txtMessage.Text = response;
            this.Enabled = true;
            MessageBox.Show(lblstatus.Text, "Message Box");

        }

        string CreateTransRequest(string uniqueTransId, string tranType, float amount, long tip, string remark, string pat)
        {
            TransRequestJson NewMessage = new TransRequestJson
            {
                uniqueTransId = uniqueTransId,

                transType = tranType,
                amount = amount,
                tip = tip,
                remark = remark,
                pat = pat

            };
            string jsonString = JsonConvert.SerializeObject(NewMessage);
            return jsonString;
        }

        string CreateVerifyRequest(string uniqueTransId, string tranType, string pat)
        {
            TransRequestJson verifyObject = new TransRequestJson
            {
                uniqueTransId = uniqueTransId,
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


    }
}
