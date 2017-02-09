/*
 * Copyright (c) 2017 Jan Henrik Sawatzki
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */
using System;

using Simple_Raw_HID;

namespace Simple_Raw_HID_Test
{
    class Program
    {
        static int Main(string[] args)
        {
            Simple_Raw_HID.Simple_Raw_HID srHID = new Simple_Raw_HID.Simple_Raw_HID();
            byte[] buf = new byte[64];

            int retValue = srHID.Open(1, 0x16C0, 0x0480, 0xFFAB, 0x0200);
            if (retValue <= 0)
            {
                Console.WriteLine("No Temp-Sensor found");
                return -1;
            }
            Console.WriteLine("Found Temp-Sensor");

            while (true)
            {
                //....................................
                // check if any Raw HID packet has arrived
                //....................................
                int num = srHID.Receive(0, ref buf, buf.Length, 220);
                if (num < 0)
                {
                    Console.WriteLine("Error Reading");
                    srHID.Close();
                    return 0;
                }

                if (num == 64)
                {
                    float temperature = (float)BitConverter.ToInt16(buf, 4) / 10.0f;
                    string power;
                    if (buf[2] != 0)
                    {
                        power = "External";
                    }
                    else
                    {
                        power = "Parasite";
                    }
                    Console.Write("Sensor #" + (char)buf[1] + " of " + (char)buf[0] + ": " + temperature + "°C, Power: " + power + ", ID: ");

                    for (int i = 0x08; i < 0x10; i++)
                    {
                        Console.Write(buf[i].ToString("X2") + " ");
                    }
                    Console.WriteLine();
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Escape)
                    {
                        return 0;
                    }

                    if (cki.KeyChar >= 32)
                    {
                        Console.WriteLine("Got key '" + cki.KeyChar + "', sending...");
                        buf[0] = (byte)cki.KeyChar;
                        for (int i = 1; i < buf.Length; i++)
                        {
                            buf[i] = 0;
                        }
                        srHID.Send(0, buf, buf.Length, 100);
                    }
                }
            }
        }
    }
}
