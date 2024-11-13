using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Async___01___PI
{
    public delegate void UpdateProgress(int i);

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cts = null;
        public UpdateProgress updateDelegat;
        
        public MainWindow()
        {
            
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            ComputePIWrap();   
        }

        public async void ComputePIWrap()
        {
       
            cts = new CancellationTokenSource();
            this.Cursor = Cursors.Wait;
            startButton.IsEnabled = false;
            cancelButton.IsEnabled = true;

            try
            {
                PITextBlock.Text = String.Empty;
                PITextBlock.Text += await Task.Run(() => ComputePi(cts));
            }
            catch(OperationCanceledException e)
            {
                MessageBox.Show(e.Message);
                PITextBlock.Text = "Operation abgebrochen";
                await FortschrittMethode(0);
            }
            finally
            {
                
                cts.Dispose();
                Fortschritt.Value = 0;
            }
            
            startButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
            this.Cursor = Cursors.Arrow;
            
        }

        public async Task<double> ComputePi(CancellationTokenSource token)
        {
            int progress = 0;
            double max = 1_000_000_000;
            double sum = 0.0;
            const double step = 1e-9;
            for (int i = 0; i < 1_000_000_000; i++)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                double x = (i + 0.5) * step;
                sum += 4.0 / (1.0 + x * x);

                if (i % 10000000 == 0 || i == 999_999_999)
                {
                    if(i == 999_999_999)
                    {
                        progress = 100;
                    }
                    else
                    {
                         progress = (int)((double)i / max * 100);
                    }
                    await FortschrittMethode(progress);
                }
            }
            return sum * step;
        }
        public async Task FortschrittMethode(int i)
        {
            if (Fortschritt.Dispatcher.CheckAccess())
            {
                Fortschritt.Value = i;
                fortschrittLabel.Content = i.ToString() + " %";

            }
            else
            {
             await Fortschritt.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Fortschritt.Value = i;
                    fortschrittLabel.Content = i.ToString()+" %";
                }));
            }
        }

        public void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cts != null) cts.Cancel();
            }
            catch(OperationCanceledException ex)
            {
                MessageBox.Show(ex.Message);
            }   
        }
    }
}
