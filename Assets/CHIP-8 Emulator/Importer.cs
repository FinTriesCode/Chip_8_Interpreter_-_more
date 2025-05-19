using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CHIP_8_Emulator
{
    public class Importer : MonoBehaviour
    {
        private void Start()
        {
            ImporterFunctionality.ReadData();
        }

        private void Update()
        {
            ImporterFunctionality.Tick();
        }
    }
}