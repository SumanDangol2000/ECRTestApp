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
        private async Task<string> SendRequestAsync(string paymentType, float amount, long tip, string remarks)
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            var response = "";
            try
            {
                response = await Task.Run(() => proxy?.sendRequest(CreateSaleRequest(paymentType, amount, tip, remarks)));
            }
            catch (Exception ex)
            {
                response = "Service Time Out." + ex.Message;

            }

            return response;
        }

        private async Task<string> getPosStatusAsync()
        {
            var endpoint = new EndpointAddress(uri);
            var proxy = channel.CreateChannel(endpoint);
            var response = "";
            try
            {
                response = await Task.Run(() => proxy?.getPosStatus());
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

        //Sale
        private async void button1_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "";
            lblstatus.Text = "";

            //var statusResponse = await getPosStatusAsync();

            //if (statusResponse == "Online")
            //{
                if (!validate())
                {
                    MessageBox.Show("Please enter valid amount.");
                    return;
                }
                this.Enabled = false;
                var response = await SendRequestAsync("sale", long.Parse(txtAmount.Text) * 100, 0, "");
                CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);
                if (_CommonJson.resultcode == "000")
                {
                    lblstatus.Text = "Payment Successful";
                }
                else if (_CommonJson.resultcode == "05")
                {
                    lblstatus.Text = _CommonJson.message;
                }
         
                txtMessage.Text = response;
                this.Enabled = true;
                SaleResponseJson saleResponse = JsonConvert.DeserializeObject<SaleResponseJson>(response);
            //}
            //else
            //{
            //    MessageBox.Show(statusResponse);
            //}


        }

        //Fonepay
        private async void button1_Click_1(object sender, EventArgs e)
        {
            txtMessage.Text = "";
            lblstatus.Text = "";
            //var statusResponse = await getPosStatusAsync();

            //if (statusResponse == "Online")
            //{
                if (!validate())
                {
                    MessageBox.Show("Please enter valid amount.");
                    return;
                }
                this.Enabled = false;
                waiting = true;
                //MessageBox.Show(Convert.ToString( float.Parse(txtAmount.Text) * 100));
                var response = await SendRequestAsync("fonePay", float.Parse(txtAmount.Text.ToString()) * 100, 0, "");

                CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);
                if (_CommonJson.resultcode == "000")
                {
                    lblstatus.Text = "Payment Successful";
                    txtAmount.Text = "";
                    cbItems.Focus();
                    t.Rows.Clear();
                }
                else if (_CommonJson.resultcode == "05")
                {
                    lblstatus.Text = _CommonJson.message;
                }

                txtMessage.Text = response;
                this.Enabled = true;
                MessageBox.Show(lblstatus.Text, "Message Box");
            //}
            //else
            //{
            //    MessageBox.Show(statusResponse);
            //}
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

        //Nepalpay
        private async void button1_Click_2(object sender, EventArgs e)
        {
    
            txtMessage.Text = "";
            lblstatus.Text = "";
            //var statusResponse = await getPosStatusAsync();

            //if (statusResponse == "Online")
            //{
                if (!validate())
                {
                    MessageBox.Show("Please enter valid amount.");
                    return;
                }
                this.Enabled = false;
                waiting = true;
                //MessageBox.Show(Convert.ToString( float.Parse(txtAmount.Text) * 100));
                var response = await SendRequestAsync("nepalPay", long.Parse(txtAmount.Text.ToString()) * 100, 0, "");

                CommonJson _CommonJson = JsonConvert.DeserializeObject<CommonJson>(response);
                if (_CommonJson.resultcode == "000")
                {
                    lblstatus.Text = "Payment Successful";
                    txtAmount.Text = "";
                    cbItems.Focus();
                    t.Rows.Clear();
                }
                else if (_CommonJson.resultcode == "05")
                {
                    lblstatus.Text = _CommonJson.message;
                }

                txtMessage.Text = response;
                this.Enabled = true;
                MessageBox.Show(lblstatus.Text, "Message Box");
            //}
            //else
            //{
            //    MessageBox.Show(statusResponse);
            //}

        }

        SaleResponseJson DecodeTransactionResponse(string Response)
        {
            SaleResponseJson sr = JsonConvert.DeserializeObject<SaleResponseJson>(Response);
            return sr;
        }
        string CreateSaleRequest(string tranType, float amount, long tip, string remark)
        {
            SaleRequestJson NewMessage = new SaleRequestJson
            {
                transType = tranType,
                amount = amount,
                tip = tip,
                remark = remark
            };
            string jsonString = JsonConvert.SerializeObject(NewMessage);
            return jsonString;
        }

        DataTable t = new DataTable();

        private void ECR_Test_Application_Load(object sender, EventArgs e)
        {
            cbItems.SelectedIndex = 0;


            t.Columns.Add("NAME", typeof(string));
            t.Columns.Add("PRICE", typeof(string));
            t.Columns.Add("QTY", typeof(string));
            t.Columns.Add("TOTAL", typeof(double));
            dgvItems.DataSource = t;

        }

        double grossAmount = 0;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            double netAmount = (Double.Parse(txtPrice.Text) * Double.Parse(txtQty.Text));
            t.Rows.Add(cbItems.SelectedItem.ToString(), txtPrice.Text, txtQty.Text, netAmount);

            grossAmount += netAmount;
            txtAmount.Text = grossAmount.ToString();
        }


        private async void button3_Click(object sender, EventArgs e)
        {

        }


    }
}
