using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ChatApp
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;

        public Form1()
        {

            InitializeComponent();
            ComboBox();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            textLocalIP.Text = GetLocalIp();
            textFriendIP.Text = GetLocalIp();
        }

       
        public void ComboBox()
        {
            
            comboBox1.DisplayMember = "Text";
            comboBox1.ValueMember = "Value";

            comboBox2.DisplayMember = "Text";
            comboBox2.ValueMember = "Value";

            var items = new[] {
             new { Text = "Arabic", Value = "ar" },
             new { Text = "Bosnian (Latin)", Value = "bs-Latn" },
             new { Text = "Bulgarian", Value = "bg" },
             new { Text = "Catalan", Value = "ca" },
             new { Text = "Chinese Simplified", Value = "zh-CHS" },
             new { Text = "Chinese Traditional", Value = "zh-CHT" },
             new { Text = "Croatian", Value = "hr" },
             new { Text = "Czech", Value = "cs" },
             new { Text = "Danish", Value = "da" },
             new { Text = "Dutch", Value = "nl" },
             new { Text = "English", Value = "en" },
             new { Text = "Estonian", Value = "et" },
             new { Text = "Finnish", Value = "fi" },
             new { Text = "French", Value = "fr" },
             new { Text = "German", Value = "de" },
             new { Text = "Greek", Value = "el" },
             new { Text = "Haitian Creole", Value = "ht" },
             new { Text = "Hebrew", Value = "he" },
             new { Text = "Hindi", Value = "hi" },
             new { Text = "Hmong Daw", Value = "mww" },
             new { Text = "Hungarian", Value = "hu" },
             new { Text = "Indonesian", Value = "id" },
             new { Text = "Italian", Value = "it" },
             new { Text = "Japanese", Value = "ja" },
             new { Text = "Kiswahili", Value = "sw" },
             new { Text = "Klingon", Value = "tlh" },
             new { Text = "Klingon (pIqaD)", Value = "tlh-Qaak" },
             new { Text = "Korean", Value = "ko" },
             new { Text = "Latvian", Value = "lv" },
             new { Text = "Lithuanian", Value = "lt" },
             new { Text = "Malay", Value = "ms" },
             new { Text = "Maltese", Value = "mt" },
             new { Text = "Norwegian", Value = "no" },
             new { Text = "Persian", Value = "fa" },
             new { Text = "Polish", Value = "pl" },
             new { Text = "Portuguese", Value = "pt" },
             new { Text = "Romanian", Value = "ro" },
             new { Text = "Russian", Value = "ru" },
             new { Text = "Slovak", Value = "sk" },
             new { Text = "Slovenian", Value = "sl" },
             new { Text = "Spanish", Value = "es" },
             new { Text = "Swedish", Value = "sv" },
             new { Text = "Thai", Value = "th" },
             new { Text = "Turkish", Value = "uk" },
             new { Text = "Urdu", Value = "ur" },
             new { Text = "Vietnamese", Value = "vi" },
            };

            comboBox1.DataSource = items;
            comboBox1.SelectedIndex = 0;
            comboBox2.DataSource = items;
            comboBox2.SelectedIndex = 0;

        }
        private string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            AdmAccessToken admToken;
            string headerValue;
            string message = "";
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if (size > 0)
                {
                    byte[] receivedData = new byte[1464];
                    receivedData = (byte[])aResult.AsyncState;
                    UnicodeEncoding encoding = new UnicodeEncoding();
                    string receivedMessage = encoding.GetString(receivedData);

                   // AdmAuthentication admAuth = new AdmAuthentication("Client Id", "Client secret");
                    AdmAuthentication admAuth = new AdmAuthentication("Windows_Chat_App", "bFKUtbOtSQ0fANOqalIAmebhC7ESonE7saRWw3BBwJM=");
                    try
                    {
                        admToken = admAuth.GetAccessToken();
                        // Create a header with the access_token property of the returned token
                        headerValue = "Bearer " + admToken.access_token;
                        string from = DetectMethod(headerValue, receivedMessage);
                        string to = comboBox2.SelectedValue.ToString();
                        message = TranslateMethod(headerValue, receivedMessage, from, to);
                    }
                    catch (WebException e)
                    {
                        ProcessWebException(e);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                    }
                    listMessage.Items.Add("Anonymous: " + message);
                    int visibleItems = listMessage.ClientSize.Height / listMessage.ItemHeight;
                    listMessage.TopIndex = Math.Max(listMessage.Items.Count - visibleItems + 1, 0);
                }

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static void ProcessWebException(WebException e)
        {
            Console.WriteLine("{0}", e.ToString());
            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
        }

        private static string TranslateMethod(string authToken, string text, string from, string to)
        {
            text = text.Replace("\0", "");
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + WebUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);

            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string translation = (string)dcs.ReadObject(stream);
                    return translation;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIP.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendIP.Text), Convert.ToInt32(textFriendPort.Text));
                sck.Connect(epRemote);

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                button2.Text = "Connected";
                button2.Enabled = false;
                button1.Enabled = true;
                button1.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            string to = comboBox2.SelectedValue.ToString();
            AdmAccessToken admToken;
            string headerValue;
            string message = "";
            try
            {

                ASCIIEncoding encoding = new ASCIIEncoding();
                AdmAuthentication admAuth = new AdmAuthentication("Windows_Chat_App", "bFKUtbOtSQ0fANOqalIAmebhC7ESonE7saRWw3BBwJM=");
                try
                {
                    admToken = admAuth.GetAccessToken();
                    // Create a header with the access_token property of the returned token
                    headerValue = "Bearer " + admToken.access_token;
                    string from = DetectMethod(headerValue, Msg.Text);
                    message = TranslateMethod(headerValue, Msg.Text, from, to);
                    Rmsg.Text = message;
                }
                catch (WebException ev)
                {
                    ProcessWebException(ev);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static string DetectMethod(string authToken, string textToDetect)
        {

            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + textToDetect;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string languageDetected = (string)dcs.ReadObject(stream);
                    return languageDetected;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMesage.Text);

                sck.Send(msg);
                listMessage.Items.Add("You:" + textMesage.Text);
                int visibleItems = listMessage.ClientSize.Height / listMessage.ItemHeight;
                listMessage.TopIndex = Math.Max(listMessage.Items.Count - visibleItems + 1, 0);
                textMesage.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
