
using System;
using System.IO.Ports;
using System.Speech.Synthesis;
using System.Windows.Forms;
/* Funções de API */
namespace ArduinoV
{
    public class api
    {
        Home o = new Home();
        public void falar(String fala)
        {
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            
            synthesizer.Volume = o.volresp;  // 0...100
            synthesizer.Rate = o.velresp;     // -10...10
            synthesizer.Speak(fala);
        }
        public void mandarcmdc(String cmd)
        {

            SerialPort port = new SerialPort(o.COMPtrs.Text, 9600);
            try
            {
                port.Open();
                port.Write(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            port.Close();
        }
    }
}
