using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCam.ArduinoConnection
{
    public class InoConn
    {
        public static SerialPort _serialPort;
        public byte[] Buff = new byte[2];

        public InoConn()
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";
            _serialPort.BaudRate = 9600;
        }

        public void turnTheLaserOn(double th1, double th2)
        {
            if(_serialPort.IsOpen)
            {
                _serialPort.Write("1" + "," + "0" + "," + th1 + "," + th2 + "\n");
            }

            else if(!_serialPort.IsOpen)
            {
                _serialPort.Open();
                _serialPort.Write("1" + "," + "0" + "," + th1 + "," + th2 + "\n");
            }

        }

        public void turnTheLaserOff()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write("0" + "," + "0" + "," + "0" + "," + "0" + "\n");
            }
        }
    
    }
}
