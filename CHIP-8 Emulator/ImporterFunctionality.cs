using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Plastic.Antlr3.Runtime.Debug;
using UnityEngine;


namespace CHIP_8_Emulator
{
    public class ImporterFunctionality
    {
        [Header(("Registers"))] [Tooltip("General Registers")]
        private static byte[] V = new byte[16];
        
        [Header(("RAM"))] 
        [Tooltip("General RAM")] private static byte[] ram = new byte[4096];

        [Tooltip("Address Registers")] private static ushort I;

        [Tooltip("Program Counter")] private static ushort PC;

        [Tooltip("Stack Pointer")] private static ushort SP = 0;
        [Tooltip("Stack")] private static ushort[] stack = new ushort[16];

        [Header("Timers")] 
        [Tooltip("Delay Timer")] private int delayTimer;
        [Tooltip("Sound Timer")] private float soundTimer;

        private bool waitingforKeypress;
        private int waitingRegister;
        
        public static void ReadData()
        {
            byte[] romData = File.ReadAllBytes("Assets/Scripts/CHIP-8 Emulator/eaty.ch8");

            for (int i = 0; i < romData.Length; i++)
            {
                ram[512 + i] = romData[i];
            }

            PC = 512;
        }

        public static void Tick()
        {
            DecodeAndExecute(ram[PC], ram[PC+1]);
        }
        
        public static void DecodeAndExecute(byte byteOne, byte byteTwo)
        {
            //get first and second nibble of first byte
            byte byteOneNibbleOne = (byte)(byteOne >> 4);
            byte byteOneNibbleTwo = (byte)(byteOne & 0x0F);

            //get the first and second nibble of second byte
            byte byteTwoNibbleOne = (byte)(byteTwo >> 4);
            byte byteTwoNibbleTwo = (byte)(byteTwo & 0x0F);

            byte F = 0xF;
            byte N = (byte)0x000F;
            byte NN = (byte)0x00FF;
            ushort NNN = (ushort)0x0FFF;

            //identify and use within a switch to find instruction

            switch (byteOneNibbleOne)
            {
                case 0:
                    if (byteTwo == 0xEE && byteOne == 0x00)
                    {
                        //call 00EE opcode
                        SP -= 1;
                        PC = stack[SP];
                        
                        Debug.Log("opcode 00EE found.");
                    }

                    else if (byteTwo == 0xE0 && byteOne == 0x00)
                    {
                        //call 00E0 opcode
                        Debug.Log("opcode 00E0 found.");
                    }

                    else
                    {
                        //call 0NNN opcode
                        
                        //unimplemented, apparently not required unless on hardware.
                        Debug.Log("opcode 0NNN found.");
                    }
                    break;

                case 1:
                    Debug.Log("opcode 1NNN found.");
                    //call method
                    PC = 0x0FFF; //or NNN
                    break;

                case 2:
                    Debug.Log("opcode 2NNN found.");
                    //call method
                    
                    
                    break;

                case 3:
                    Debug.Log("opcode 3XNN found.");
                    //call method
                    if (V[byteOneNibbleTwo] == byteTwo) SkipNextInstruction();
                    else NextInstruction();
                    
                    break;

                case 4:
                    Debug.Log("opcode 4XNN found.");
                    //call method
                    break;

                case 5:
                    if (byteTwoNibbleTwo == 0x00)
                    {
                        Debug.Log("opcode 5XYN found.");
                        //call method

                        //TODO
                    }

                    break;

                case 6:
                    Debug.Log("opcode 6XNN found.");
                    //call method
                    V[byteOneNibbleTwo] = byteTwo;
                    break;

                case 7:
                    Debug.Log("opcode 7XNN found.");
                    //call method
                    V[byteOneNibbleTwo] += byteTwo;
                    break;

                case 8:
                    Case8(byteOneNibbleOne, byteOneNibbleTwo, byteTwoNibbleOne, byteTwoNibbleTwo, F);
                    break;

                case 9:
                    if (byteTwoNibbleTwo == 0x00)
                    {
                        Debug.Log("opcode 9XY0 found.");
                        //call method

                        //TODO
                    }

                    break;

                case 0x0A:
                    Debug.Log("opcode ANNN found.");
                    //call method
                    I = NNN;

                    break;

                case 0x0B:
                    Debug.Log("opcode BNNN found.");
                    //call method
                    break;

                case 0x0C:
                    Debug.Log("opcode CXNN found.");
                    //call method
                    break;

                case 0x0D:
                    Debug.Log("opcode DXYN found.");
                    //call method
                    break;

                case 0x0E:
                    if (byteTwoNibbleTwo == 0x01)
                    {
                        Debug.Log("opcode EXA1 found.");
                        //call method
                    }

                    else if (byteTwoNibbleTwo == 0x0E)
                    {
                        Debug.Log("opcode EX9E found.");
                        //call method
                    }

                    break;

                case 0x0F:
                    CaseF(byteTwoNibbleTwo);
                    break;

                default: break;
            }
        }

        private static void Case8(byte byteOneNibbleOne, byte byteOneNibbletwo, byte byteTwoNibbleOne,
            byte byteTwoNibbleTwo, byte F)
        {
            ushort newValue = 0;

            switch (byteTwoNibbleTwo)
            {
                case 0:
                    Debug.Log("8XY0 found.");
                    V[byteOneNibbletwo] = V[byteTwoNibbleOne];
                    break;

                case 1:
                    Debug.Log("8XY1 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] | V[byteTwoNibbleOne]);
                    break;

                case 2:
                    Debug.Log("8XY2 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] & V[byteTwoNibbleOne]);
                    break;

                case 3:
                    Debug.Log("8XY3 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] ^ V[byteTwoNibbleOne]);
                    break;

                case 4:
                    Debug.Log("8XY4 found.");
                    newValue = (ushort)(V[byteOneNibbletwo] + V[byteTwoNibbleOne]);

                    if (newValue > 255)
                    {
                        newValue -= 256;
                        V[F] = 1;
                    }
                    else V[F] = 0;

                    V[byteOneNibbletwo] = (byte)newValue;
                    break;

                case 5:
                {
                    Debug.Log("8XY5 found.");

                    short newValueShort = (short)(V[byteOneNibbletwo] - V[byteTwoNibbleOne]);

                    if (newValueShort < 0)
                    {
                        newValueShort += 256;
                        V[F] = 1;
                    }
                    else V[F] = 0;

                    V[byteOneNibbletwo] = (byte)newValueShort;
                }
                    break;

                case 6:
                    Debug.Log("8XY6 found.");

                    V[F] = (byte)(V[byteOneNibbletwo] & 0x1);
                    V[byteOneNibbletwo] = (byte)(V[byteTwoNibbleOne] >> 1);

                    break;

                case 7:
                {
                    Debug.Log("8XY7 found.");
                    short newValueShort = (short)(V[byteTwoNibbleOne] - V[byteOneNibbletwo]);

                    if (newValueShort < 0)
                    {
                        newValueShort += 256;
                        V[F] = 1;
                    }
                    else V[F] = 0;

                    V[byteOneNibbletwo] = (byte)newValueShort;
                }
                    break;

                case 0x0E:
                    Debug.Log("8XYE found.");
                    newValue = (ushort)(V[byteOneNibbletwo] << 1);

                    if (newValue > 255)
                    {
                        newValue -= 256;
                        V[F] = 1;
                    }
                    else V[F] = 0;

                    V[byteOneNibbletwo] = (byte)newValue;
                    break;
            }
        }

        private static void CaseF(byte byteTwoNibbleTwo)
        {
            switch (byteTwoNibbleTwo)
            {
                case 7:
                    Debug.Log("FX07 found.");
                    break;

                case 0x0A:
                    Debug.Log("FX0A found.");
                    break;

                case 15:
                    Debug.Log("FX15 found.");
                    break;

                case 18:
                    Debug.Log("FX18 found.");
                    break;

                case 0x0E:
                    Debug.Log("FX1E found.");
                    break;

                case 29:
                    Debug.Log("FX29 found.");
                    break;

                case 33:
                    Debug.Log("FX33 found.");
                    break;

                case 55:
                    Debug.Log("FX55 found.");
                    break;

                case 65:
                    Debug.Log("FX65 found.");
                    break;
            }
        }

        private static void NextInstruction()
        {
            PC += 2;
        }

        private static void SkipNextInstruction()
        {
            PC += 4;
        }
        
    }
}