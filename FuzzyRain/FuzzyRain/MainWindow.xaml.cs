﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SimulationMethods;
using System.IO;
using System.Xml;
using FuzzyRain.Model;

namespace FuzzyRain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer UpdateAnimationTimer;

        public MainWindow()
        {
            InitializeComponent();
            UpdateAnimationTimer = new DispatcherTimer();
            UpdateAnimationTimer.Tick += new EventHandler(UpdateAnimationTimer_Tick);
            UpdateAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }
        private void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".txt";
            dlg.Filter = "XML documents (.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                TextBoxArchivoEntrada.Text = filename;
                this.ButtonComenzar.IsEnabled = true;                
            }
        }

        private void MenuItemSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemCargarLLuvia_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void ButtonExaminar_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void ButtonComenzar_Click(object sender, RoutedEventArgs e)
        {
            BeginSimulation();
            
            // Parseo
            Distribution[] valuesParsed = ParseFile(TextBoxArchivoEntrada.Text);

            // Simulacion
            Simulation(valuesParsed);

            stkDataInput.Visibility = System.Windows.Visibility.Visible;
            stkDataOutput.Visibility = System.Windows.Visibility.Visible;

            ButtonComenzar.Content = "Re-intentar";
        }

        private void UpdateAnimationTimer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            double newSize = random.NextDouble() * 200;
            ImageLLuvia.Width = newSize;
            ImageLLuvia.Height = newSize;
            LabelLLuvia.Content = newSize.ToString("00.00") + "mm";

            double newSize1 = random.NextDouble() * 200;
            ImageSuperficie.Width = newSize1;
            ImageSuperficie.Height = newSize1;
            LabelSuperficie.Content = newSize1.ToString("00.00") + "mm";

            double newSize2 = random.NextDouble() * 200;
            ImageVolumen.Width = newSize2;
            ImageVolumen.Height = newSize2;
            LabelVolumen.Content = newSize2.ToString("00.00") + "mm";
        }

        private void BeginSimulation()
        {
            UpdateAnimationTimer.Start();
        }

        private void Simulation(Distribution[] distributions)
        {
            // TODO: Los rangos estan harcodeados, de todos modos no aplicarían cuando el calculo se hace utilizando la libreria react.Net.
            // Para el caso del algoritmo de MonteCarlo implementado "artesanalmente" debieramos ver como se armarían los rangos o si, talvez,
            // debieramos importarlo desde el archivo de entrada tambien. Analizar!!
            var rankCount = 12;
                        
            Rank[] ranks = new Rank[rankCount];

            for (int i = 0; i < rankCount; i++)
            {            
                ranks[i] = new Rank(i * 10, i * 10 + 10);
            }
            
            int numberOfEvents = string.IsNullOrEmpty(txtCountEvents.Text) ? 0 : int.Parse(txtCountEvents.Text);

            // Set Parsed Data
            SetDataMonths(distributions, true);

            Distribution[] simulations = new Distribution[13];
            for (int i = 10; i <= 12; i++)
            {
                simulations[i] = new Distribution();
                var myModel = new MonteCarloModel(distributions[i].Average, distributions[i].Std_Desv);
                
                simulations[i].ValuesInOrderOfAppearance = myModel.GetFirstNEvents(numberOfEvents);
            }

            // Set Simulated Data
            SetDataMonths(simulations, false);
        }

        public Distribution[] ParseFile(string fileName)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(fileName);            

            // TODO: Es un array de 12 Distributions, 1 x mes, las cuales tienen los valores de lluvias de ese mes para la cantidad de años muestreados.
            // no se si sera una buena estructura pero fue la primera que se me ocurrió para probar el parseo. De ultima habria que cambiarla
            Distribution[] monthsPrecipitations = new Distribution[13];

            for (int i = 1; i <= 12; i++)
            {
                monthsPrecipitations[i] = new Distribution();            
            }

            try
            {
                int month;
                double precipitation;
                SimulationType simulationType = GetSimulationType();

                foreach (XmlNode item in xDoc.SelectNodes("/rainfall/yearfall/fall"))
                {                    
                    month = int.Parse(item.SelectSingleNode("month").Attributes["value"].InnerText);
                    precipitation = double.Parse(item.SelectSingleNode("precipitation").Attributes["value"].InnerText);
                    
                    if (month == 1)
                        monthsPrecipitations[1].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 2)
                        monthsPrecipitations[2].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 3)
                        monthsPrecipitations[3].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 4)
                        monthsPrecipitations[4].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 5)
                        monthsPrecipitations[5].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 6)
                        monthsPrecipitations[6].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 7)
                        monthsPrecipitations[7].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 8)
                        monthsPrecipitations[8].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 9)
                        monthsPrecipitations[9].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 10)
                        monthsPrecipitations[10].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 11)
                        monthsPrecipitations[11].AddValueInOrderOfAppearance(precipitation, simulationType);
                    else if (month == 12)
                        monthsPrecipitations[12].AddValueInOrderOfAppearance(precipitation, simulationType);
                }

            }
            catch (Exception ex)
            {
                monthsPrecipitations = null;
            }

            return monthsPrecipitations;
        }

        private SimulationType GetSimulationType()
        {
            switch (cmbSimulationType.SelectedIndex)
            {
                case 0:
                    return SimulationType.Daily;
                case 1:
                    return SimulationType.Weekly;
                default:
                    return SimulationType.Monthly;
            }
        }

        private void SetDataMonths(Distribution[] distributions, bool isInput)
        {
            if (isInput)
            {                                
                inputTab.SetDataMonths(distributions);
            }
            else
            {                
                outputTab.SetDataMonths(distributions);
            }            
        }

    }    
}
