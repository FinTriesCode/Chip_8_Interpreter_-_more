using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Random = System.Random;


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
        [Tooltip("Delay Timer")] private static byte delayTimer;
        [Tooltip("Sound Timer")] private static float soundTimer;

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
            ushort fullOpcode = (ushort)(byteOne << 8 + byteTwo);
            
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
                case 0x0:
                    if (byteTwo == 0xEE && byteOne == 0x00)
                    {
                        //call 00EE opcode
                        SP -= 1;
                        PC = stack[SP];
                        
                        Debug.Log("opcode 00EE found.");
                        
                        //TODO: Double check this aligns with my use of the SP and PC
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

                case 0x1:
                    Debug.Log("opcode 1NNN found.");
                    //call method
                    PC = 0x0FFF; //or NNN
                    break;

                case 0x2:
                    Debug.Log("opcode 2NNN found.");
                    // //call method
                    SP++;
                    stack[SP] = PC;
                    PC = 0x0FFF; //NNN
                    
                    break;

                case 0x3:
                    Debug.Log("opcode 3XNN found.");
                    //call method
                    if (V[byteOneNibbleTwo] == byteTwo) SkipNextInstruction();
                    else NextInstruction();
                    
                    break;

                case 0x4:
                    Debug.Log("opcode 4XNN found.");
                    //call method
                    if (V[byteOneNibbleTwo] != byteTwo) SkipNextInstruction();
                    else NextInstruction();
                    break;

                case 0x5:
                    if (byteTwoNibbleTwo == 0x00)
                    {
                        Debug.Log("opcode 5XY0 found.");
                        //call method
                        if(V[byteOneNibbleTwo] == V[byteTwoNibbleOne])
                            SkipNextInstruction();
                    }

                    break;

                case 0x6:
                    Debug.Log("opcode 6XNN found.");
                    //call method
                    V[byteOneNibbleTwo] = byteTwo;
                    break;

                case 0x7:
                    Debug.Log("opcode 7XNN found.");
                    //call method
                    V[byteOneNibbleTwo] += byteTwo;
                    break;

                case 0x8:
                    Case8(byteOneNibbleOne, byteOneNibbleTwo, byteTwoNibbleOne, byteTwoNibbleTwo, F);
                    break;

                case 0x9:
                    if (byteTwoNibbleTwo == 0x00)
                    {
                        Debug.Log("opcode 9XY0 found.");
                        //call method
                        if(V[byteOneNibbleTwo] != V[byteTwoNibbleOne])
                            SkipNextInstruction();
                        
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
                    PC = (ushort)(fullOpcode & 0x0FFF + V[0]); //use mask to get last 3 nibbles of opcode and add V[0]
                    break;

                case 0x0C:
                    Debug.Log("opcode CXNN found.");
                    //call method
                    var rand = new Random();
                    V[byteOneNibbleTwo] = (byte)(rand.Next() & byteTwo);
                    break;

                case 0x0D:
                    Debug.Log("opcode DXYN found.");
                    //call method
                    
                    //TODO
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
                    CaseF(byteOneNibbleTwo, byteTwo);
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
                case 0x0:
                    Debug.Log("8XY0 found.");
                    V[byteOneNibbletwo] = V[byteTwoNibbleOne];
                    break;

                case 0x1:
                    Debug.Log("8XY1 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] | V[byteTwoNibbleOne]);
                    break;

                case 0x2:
                    Debug.Log("8XY2 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] & V[byteTwoNibbleOne]);
                    break;

                case 0x3:
                    Debug.Log("8XY3 found.");
                    V[byteOneNibbletwo] = (byte)(V[byteOneNibbletwo] ^ V[byteTwoNibbleOne]);
                    break;

                case 0x4:
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

                case 0x5:
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

                case 0x6:
                    Debug.Log("8XY6 found.");

                    V[F] = (byte)(V[byteOneNibbletwo] & 0x1);
                    V[byteOneNibbletwo] = (byte)(V[byteTwoNibbleOne] >> 1);

                    break;

                case 0x7:
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

        private static void CaseF(byte byteOneNibbleTwo, byte byteTwo)
        {
            switch (byteTwo)
            {
                case 0x7:
                    Debug.Log("FX07 found.");

                    V[byteOneNibbleTwo] = delayTimer;
                    break;

                case 0x0A:
                    Debug.Log("FX0A found.");
                    break;

                case 0x15:
                    Debug.Log("FX15 found.");

                    delayTimer = V[byteOneNibbleTwo];
                    break;

                case 0x18:
                    Debug.Log("FX18 found.");
                    
                    soundTimer = V[byteOneNibbleTwo];
                    break;

                case 0x1E:
                    Debug.Log("FX1E found.");

                    I = (ushort)((I + V[byteOneNibbleTwo]) & 0xFFF); 
                    break;

                case 0x29:
                    Debug.Log("FX29 found.");
                    break;

                case 0x33:
                    Debug.Log("FX33 found.");

                    ram[I] = (byte)(V[byteOneNibbleTwo] / 100); //100's
                    ram[I + 1] = (byte)((V[byteOneNibbleTwo] / 100) % 10); //10's
                    ram[I + 2] = (byte)((V[byteOneNibbleTwo] % 100) % 10); //1's
                    break;

                case 0x55:
                    Debug.Log("FX55 found.");

                    for (int i = 0; i < byteTwo; i++)
                        ram[I + i] = V[i];
                    
                    break;

                case 0x65:
                    Debug.Log("FX65 found.");
                    
                    for (int i = 0; i < byteTwo; i++)
                        V[i] = ram[I + i];
                    
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