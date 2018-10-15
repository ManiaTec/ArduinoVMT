using System;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Speech.Recognition;
using System.Net;
using System.Speech.Synthesis;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace ArduinoV
{
    public partial class Home : Form
    {
        /* Declaração e configuração de sistemas para comando de voz e conexão com portas COM (Serial) */
        private SpeechRecognitionEngine sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-BR"));
        private bool ligado = false;
        private String compt;
        private XDocument doc;
        private XDocument doc2;
        private Double nconf;
        public int volresp;
        public int velresp;
        private bool props;
        private bool eaq;
        private bool eaq2 = false;
        [ImportMany(typeof(pll))]
        public IEnumerable<pll> Plugins { get; set; }

        public Home()
        {
            InitializeComponent();
            //-----------------------
            if (System.IO.Directory.Exists("plugins")) {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog("plugins"));

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }else
            {
                System.IO.Directory.CreateDirectory("plugins");
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog("plugins"));

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            COMLb.Show();
            COMPtrs.Show();
            string[] ports = SerialPort.GetPortNames();
            COMPtrs.DataSource = ports;
            var src = DateTime.Now;
            horas.Text = src.Hour + ":" + src.Minute;

            inithoras(horas);
            Choices colors = new Choices();
            dataGridView1.AllowDrop = false;
            dataGridView1.ColumnCount = 6;
            dataGridView1.Columns[0].Name = "Comando";
            dataGridView1.Columns[1].Name = "Tem resposta ?";
            dataGridView1.Columns[2].Name = "resposta";
            dataGridView1.Columns[3].Name = "Tem Comando (COM)?";
            dataGridView1.Columns[4].Name = "Comando (COM)?";
            dataGridView1.Columns[5].Name = "Descrição:";
            if (!File.Exists(Directory.GetCurrentDirectory() + "/configs.xml"))
            {
                doc2 = new XDocument(new XElement("configs",
                          new XElement("conf",
                              new XAttribute("nconf", "0.7"),
                              new XAttribute("volresp", "100"),
                              new XAttribute("velresp", "0"),
                              new XAttribute("props", "sim"))));
                doc2.Save("configs.xml");
                doc2 = XDocument.Load("configs.xml");
                nconf = Convert.ToDouble( doc2.Element("configs").Element("conf").Attribute("nconf").Value );
                volresp = Convert.ToInt16(doc2.Element("configs").Element("conf").Attribute("volresp").Value);
                velresp = Convert.ToInt16(doc2.Element("configs").Element("conf").Attribute("velresp").Value);
                if (doc2.Element("configs").Element("conf").Attribute("props").Value.Equals("sim"))
                {
                    checkBox1.CheckState = CheckState.Unchecked;
                    button3.Show();
                }else{
                    eaq = true;
                    checkBox1.CheckState = CheckState.Checked;
                    button2.Hide();
                    button3.Hide();
                }
                setconfvals(nconf, volresp, velresp);
            }else{
                doc2 = XDocument.Load("configs.xml");
                nconf = Convert.ToDouble(doc2.Element("configs").Element("conf").Attribute("nconf").Value);
                volresp = Convert.ToInt16(doc2.Element("configs").Element("conf").Attribute("volresp").Value);
                velresp = Convert.ToInt16(doc2.Element("configs").Element("conf").Attribute("velresp").Value);
                if (doc2.Element("configs").Element("conf").Attribute("props").Value.Equals("sim"))
                {
                    checkBox1.CheckState = CheckState.Unchecked;
                    button3.Show();
                }
                else
                {
                    eaq = true;
                    checkBox1.CheckState = CheckState.Checked;
                    button2.Hide();
                    button3.Hide();
                }
                setconfvals(nconf, volresp, velresp);
            }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/comandos.xml")){
               
                doc = new XDocument(new XElement("comandos",
                            new XElement("olá",
                                new XAttribute("temresp", "sim"),
                                new XAttribute("resp", "Olá tudo bem."),
                                new XAttribute("temcomcoma", "não"),
                                new XAttribute("com", "null"),
                                new XAttribute("desc", "Um mero oi"))));
               
                doc.Save("comandos.xml");
                doc = XDocument.Load("comandos.xml");
                
                string[] comandos = new string[doc.Element("comandos").Elements().Count()];
                for (int i = 1; i <= doc.Element("comandos").Elements().Count(); i++)
                {
                    int d = i - 1;
                    comandos[i - 1] = doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString().Replace("-", " ");
                    var temr = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temresp").Value.ToString();
                    var resp = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("resp").Value.ToString();
                    var temcomc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temcomcoma").Value.ToString();
                    var comc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("com").Value.ToString();
                    var descc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("desc").Value.ToString();
                    dataGridView1.Rows.Add(doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString(), temr, resp, temcomc, comc, descc);
                }
                colors.Add(comandos);
            }else{
                doc = XDocument.Load("comandos.xml");
                string[] comandos = new string[doc.Element("comandos").Elements().Count()];
                for (int i = 1; i <= doc.Element("comandos").Elements().Count(); i++)
                {
                    int d = i - 1;
                    comandos[i - 1] = doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString().Replace("-", " ");
                    var temr = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temresp").Value.ToString();
                    var resp = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("resp").Value.ToString();
                    var temcomc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temcomcoma").Value.ToString();
                    var comc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("com").Value.ToString();
                    var descc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("desc").Value.ToString();
                    dataGridView1.Rows.Add(doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString(), temr, resp, temcomc, comc, descc);
                }
                colors.Add( comandos);
            }
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(colors);
            Grammar g = new Grammar(gb);
            sre.LoadGrammar(g);
            sre.SpeechRecognized +=
            new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            sre.SetInputToDefaultAudioDevice();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;

        }

        private void loaddict()
        {
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-BR"));
            dataGridView1.Rows.Clear();
            eaq2 = false;
            Choices colors = new Choices();
            string[] comandos = new string[doc.Element("comandos").Elements().Count()];

            for (int i = 1; i <= doc.Element("comandos").Elements().Count(); i++)
            {
                int d = i - 1;
                comandos[i - 1] = doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString().Replace("-", " ");
                var temr = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temresp").Value.ToString();
                var resp = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("resp").Value.ToString();
                var temcomc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("temcomcoma").Value.ToString();
                var comc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("com").Value.ToString();
                var descc = doc.Element("comandos").Elements().ElementAt(i - 1).Attribute("desc").Value.ToString();
                dataGridView1.Rows.Add(doc.Element("comandos").Elements().ElementAt(i - 1).Name.ToString(), temr, resp, temcomc, comc, descc);
            }
            eaq2 = true;
            colors.Add(comandos);
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(colors);
            Grammar g = new Grammar(gb);
            sre.LoadGrammar(g);
            sre.SpeechRecognized +=
            new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            sre.SetInputToDefaultAudioDevice();
        }
        static private void inithoras(Label horas)
        {
            var src = DateTime.Now;
            
            horas.Text = src.Hour + ":" + src.Minute;
        }
        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            
            if (e.Result.Confidence > nconf){
                try
                {
                    var bata = e.Result.Text.Replace(" ", "-");
                    foreach (var plugin in Plugins)
                    {
                       plugin.cmdd(e.Result.Text);
                    }
                    if (doc.Element("comandos").Element(bata).Attribute("temresp").Value.ToLower().Equals("sim"))
                    {
                        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                        synthesizer.Volume = volresp;  // 0...100
                        synthesizer.Rate = velresp;     // -10...10
                        synthesizer.Speak(doc.Element("comandos").Element(bata).Attribute("resp").Value.ToString());
                    }
                    if (doc.Element("comandos").Element(bata).Attribute("temcomcoma").Value.ToLower().Equals("sim") )
                    {
                        SerialPort port = new SerialPort(compt, 9600);
                        try
                        {
                            port.Open();
                            port.Write(doc.Element("comandos").Element(bata).Attribute("com").Value.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        port.Close();
                    }
                }
                catch{
                    
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ligado)
            {
                button1.Text = "Desligar sistema";
                ligado = true;
                sre.RecognizeAsync(RecognizeMode.Multiple);
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
                dataGridView1.ReadOnly = true;
            }
            else
            {
                button1.Text = "Ligar sistema";
                ligado = false;
                sre.RecognizeAsyncStop();
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                dataGridView1.ReadOnly = false;
            }

        }
        private void COMPtrs_SelectedIndexChanged(object sender, EventArgs e)
        {
            compt = COMPtrs.Text;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCOrHdPaH7YrN2c1tCIkwN3g");
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (!eaq) {
                    MessageBox.Show("Agradeço atenção espero que volte a acessar o canal em breve!", "ok :(", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                button2.Hide();
                button3.Hide();
                props = false;
                eaq = false;
            }else{
                props = true;
                button2.Show();
                button3.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string webData = "async";
            bool error;
            try
            {
                error = false;
                System.Net.WebClient wc = new System.Net.WebClient();
                webData = wc.DownloadString("http://guityware.com/linkc.txt");
            }
            catch (WebException er)
            {
                error = true;
                Debug.WriteLine(er.GetType().FullName);
                Debug.WriteLine(er.GetBaseException().ToString());
            }
            if (!error) {
                System.Diagnostics.Process.Start(webData);
            }else
            {
                System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCOrHdPaH7YrN2c1tCIkwN3g");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("O uso é simples, na primeira coluna indique o comando de voz, na segunda você coloca apenas sim ou não, caso use outra palavra qualquer sera identificada como não, está escolha é para resposta por voz. A terceira coluna refere-se a resposta, caso exista, a indique. A quarta coluna se refere se vai ou não vai enviar comandos por meio da porta COM(Arduino), caso sim indique na próxima coluna o comando o qual  melhor se adequa a seu código. Por fim dê uma descrição somente para sua organização!", "Instruções", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            confl.Text = trackBar1.Value.ToString() + "%";
            nconf = Convert.ToDouble(trackBar1.Value) / 100;
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            VolL.Text = trackBar2.Value.ToString();
            volresp = trackBar2.Value;
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            velfl.Text = trackBar3.Value.ToString();
            velresp = trackBar3.Value;
        }
        private void setconfvals(Double v1, int v2, int v3 )
        {
            if (v1 * 100 > 100) { trackBar1.Value = Convert.ToInt16(v1 * 10); } else { trackBar1.Value = Convert.ToInt16(v1 * 100); }
            confl.Text = trackBar1.Value.ToString() + "%";
            //--------------------------------------------
            trackBar2.Value = v2;
            VolL.Text = trackBar2.Value.ToString();
            //--------------------------------------------
            trackBar3.Value = v3;
            velfl.Text = trackBar3.Value.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            doc2.Element("configs").Element("conf").Attribute("nconf").Value =  nconf.ToString();
            doc2.Element("configs").Element("conf").Attribute("volresp").Value = volresp.ToString();
            doc2.Element("configs").Element("conf").Attribute("velresp").Value = velresp.ToString();
            if (props) { doc2.Element("configs").Element("conf").Attribute("props").Value = "sim"; }else { doc2.Element("configs").Element("conf").Attribute("props").Value = "não"; }
            doc2.Save("configs.xml");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            eaq2 = true;
            dataGridView1.Rows.Add("Comando", "sim", "Resposta", "não", "comando(COM)", "descrição");
        }

        private void button8_Click(object sender, EventArgs e)
        {
           
            if (this.dataGridView1.RowCount > 1)
            {
                foreach (DataGridViewRow item in this.dataGridView1.SelectedRows)
                {
                    doc.Element("comandos").Elements().ElementAt(item.Index).Remove();
                    dataGridView1.Rows.RemoveAt(item.Index);
                    loaddict();
                }
            }else{
                MessageBox.Show("Desculpe por isso, mas não se pode deletar todos os comandos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var newvalue = (string)dataGridView1[e.ColumnIndex, e.RowIndex].Value;
            if (e.ColumnIndex - 1 >= 0 ) {
                doc.Element("comandos").Elements().ElementAt(e.RowIndex).Attributes().ElementAt(e.ColumnIndex - 1).SetValue(newvalue);
                loaddict();
            }
            else
            {
                doc.Element("comandos").Elements().ElementAt(e.RowIndex).Name = newvalue.Replace(" ","-");
                loaddict();
            }
        }


        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            
            doc.Element("comandos").Elements().ElementAt(e.Row.Index).Remove();
            loaddict();
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
           
            if (eaq2) {
                var comando = (string)dataGridView1[0, e.RowIndex].Value;
                var temresp2 = (string)dataGridView1[1, e.RowIndex].Value;
                var resp2 = (string)dataGridView1[2, e.RowIndex].Value;
                var temcomcoma2 = (string)dataGridView1[3, e.RowIndex].Value;
                var com2 = (string)dataGridView1[4, e.RowIndex].Value;
                var desc2 = (string)dataGridView1[5, e.RowIndex].Value;
                doc.Element("comandos").Add(new XElement(comando,
                                        new XAttribute("temresp", temresp2),
                                        new XAttribute("resp", resp2),
                                        new XAttribute("temcomcoma", temcomcoma2),
                                        new XAttribute("com", com2),
                                        new XAttribute("desc", desc2)));
                loaddict();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            loaddict();
            doc.Save("comandos.xml");
        }

    }
}
