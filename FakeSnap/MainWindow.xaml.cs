using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;


namespace FakeSnap
{   

    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            load_stickers();          
        } 
        static bool captured = false;
        static double x_shape, x_canvas, y_shape, y_canvas;
        static Image source = null;        
        double rotate = 0;
        static double _zoomValue = 1.0;
        static TextBlock source_text = null;
        static int xds = 0;
        static int ads = 0;

        private void pickSticker()
        {
            MessageBox.Show("Wybierz Element!");
        }
        private void New_img_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = "*.jpg";
            openFileDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp, *.gif *.tiff) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp; *.gif; *.tiff";
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // wrzuca do image_boxa
            if (result == true)
            {
                string filename = openFileDlg.FileName;
                BitmapImage img = new BitmapImage(new Uri(openFileDlg.FileName));
                bg_canvas.Source = img;
                Canvas_main.Width = img.Width;
                Canvas_main.Height = img.Height;             
                bg_canvas.Stretch = Stretch.None;                 
                                             
            }
        }

        private void Bigger_Click(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                source.Width += 5;
                source.Height += 5;
            }
           
        }

        private void Smaller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (source != null)
                {
                    source.Width -= 5;
                    source.Height -= 5;
                }
               
            }
            catch (System.ArgumentException)
            {
                MessageBox.Show("WYMIAR NIE MOZE BYC UJEMNY!\nDodaj nowa nalypke");
                Canvas_main.Children.Remove(source);
                source = null;
            }
           
        }

        private void Rotate_Right_Click(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                rotate += 1;
                obracanie(rotate);
            }
           
        }

        private void Rotate_Lefr_Click(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                rotate -= 1;
                obracanie(rotate);
            }
           
        }
        public void obracanie(double a)
        {
            if (source != null)
            {
                RotateTransform rotateTransform = new RotateTransform(a);
                source.RenderTransform = rotateTransform;
            }
          
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Image"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "PNG File (.png)|*.png"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;                
                WriteToPng(Canvas_main, dlg.FileName);
            }

        }
        public void WriteToPng(UIElement element, string filename)
        {
            try
            {                
                var rect = new Rect(element.RenderSize);
                var visual = new DrawingVisual();

                using (var dc = visual.RenderOpen())
                {
                    dc.DrawRectangle(new VisualBrush(element), null, rect);
                }

                var bitmap = new RenderTargetBitmap(
                    (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
                bitmap.Render(visual);
                //var crop = new CroppedBitmap(bitmap, new Int32Rect(0, 0, Convert.ToInt32(Canvas_main.ActualWidth), Convert.ToInt32(Canvas_main.ActualHeight)));
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var file = File.OpenWrite(filename))
                {
                    encoder.Save(file);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("Plik w uzyciu!\nNie możesz nadpisać pliku aktualnie załadowanego w programie.");
            }
        }                      

        private void Cusotm_Sticker_add(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = "*.jpg";
            openFileDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp, *.gif *.tiff) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp; *.gif; *.tiff";
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // wrzuca do image_boxa
           
            if (result == true)
            {               
                Image img2 = new Image
                {
                    MaxWidth = 66,
                    MaxHeight = 66,
                    MinWidth = 66,
                    MinHeight = 66,
                    Source = new BitmapImage(new Uri(openFileDlg.FileName)),
                    Focusable = false,
                    Margin = new Thickness(6),
                };             
                int fileCount = (from file in Directory.EnumerateFiles(@"Resources/", "*.png", SearchOption.AllDirectories)select file).Count();
                int b = 1;
                while (b <= fileCount+1) {
                    try {                        
                        string nr_nazwy = b.ToString();
                        File.Copy(openFileDlg.FileName, @"Resources/Sticker_" + nr_nazwy + ".png");
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        b++;                       
                    }               
                 }
                img2.MouseLeftButtonDown += new MouseButtonEventHandler(DragImage);               
                this.naklejki.Children.Add(img2);                
            }
        }

        private void Width_up_Click(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                source.Stretch = Stretch.Fill;
                source.Width += 1;
            }
           
        }

        private void Width_low_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (source != null)
                {
                    source.Stretch = Stretch.Fill;
                    source.Width -= 1;
                }
               
            }
            catch (System.ArgumentException)
            {
                MessageBox.Show("WYMIAR NIE MOZE BYC UJEMNY!\nDodaj nową nalypke");
                Canvas_main.Children.Remove(source);
                source = null;
            }

        }

        private void Height_up_Click(object sender, RoutedEventArgs e)
        {
            if (source != null)
            {
                source.Stretch = Stretch.Fill;
                source.Height += 1;
            }
            
        }

        private void Height_low_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (source != null)
                {
                    source.Stretch = Stretch.Fill;
                    source.Height -= 1;
                }
               
            }
            catch (System.ArgumentException)
            {
                MessageBox.Show("WYMIAR NIE MOZE BYC UJEMNY!\nDodaj nową nalypke");
                Canvas_main.Children.Remove(source);
                source = null;
            }

        }            
      
        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source_text = null;             
            source = (Image)sender;                 
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(Canvas_main).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(Canvas_main).Y;           
            double angle = 0.0;
            Transform transform = source.RenderTransform;
            RotateTransform xd = new RotateTransform();
            angle = ((RotateTransform)transform).Angle;
            rotate = angle;
            source.Focus();    
        }
        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (source_text == null)
            {
                if (captured)
                {
                    double x = e.GetPosition(Canvas_main).X;
                    double y = e.GetPosition(Canvas_main).Y;
                    x_shape += x - x_canvas;
                    Canvas.SetLeft(source, x_shape);
                    x_canvas = x;
                    y_shape += y - y_canvas;
                    Canvas.SetTop(source, y_shape);
                    y_canvas = y;
                }
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;            
        }
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (source != null)
            {               
                if (e.Key == Key.NumPad2)
                {
                    Canvas.SetTop(source, Canvas.GetTop(source) + 1);
                }
                else if (e.Key == Key.NumPad8)
                {
                    Canvas.SetTop(source, Canvas.GetTop(source) - 1);
                }
                else if (e.Key == Key.NumPad4)
                {                    
                    Canvas.SetLeft(source, Canvas.GetLeft(source) - 1);
                }
                else if (e.Key == Key.NumPad6)
                {
                    Canvas.SetLeft(source, Canvas.GetLeft(source) + 1);
                }
                else if (e.Key == Key.W)
                {
                    source.Height += 5;
                    source.Width  += 5;
                }
                else if (e.Key == Key.S)
                {
                    try
                    {
                        if (source != null)
                        {
                            source.Width -= 5;
                            source.Height -= 5;
                        }
                       
                    }
                    catch (System.ArgumentException)
                    {
                        MessageBox.Show("WYMIAR NIE MOZE BYC UJEMNY!\nDodaj nowa nalypke");
                        Canvas_main.Children.Remove(source);
                        source = null;
                    }
                }               
                else if (e.Key == Key.Q)
                {
                    rotate -= 1;
                    obracanie(rotate);
                }
                else if (e.Key == Key.E)
                {
                    rotate += 1;
                    obracanie(rotate);
                }
                else if (e.Key == Key.Delete)
                {
                    if (source != null)
                    {
                        Canvas_main.Children.Remove(source);
                        source = null;
                    }
                    
                }
            }           
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ScrollViewer.SetVerticalScrollBarVisibility(viewer, ScrollBarVisibility.Disabled);
                ScrollViewer.SetHorizontalScrollBarVisibility(viewer, ScrollBarVisibility.Disabled);
                if (e.Delta > 0)
                {
                    _zoomValue += 0.1;
                }
                else
                {
                    if (_zoomValue > 0.2)
                    {
                        _zoomValue -= 0.1;
                    }
                }
                ScaleTransform scale = new ScaleTransform(_zoomValue, _zoomValue);
                Canvas_main.LayoutTransform = scale;
                e.Handled = true;
            }
            else
            {
                ScrollViewer.SetVerticalScrollBarVisibility(viewer, ScrollBarVisibility.Auto);
                ScrollViewer.SetHorizontalScrollBarVisibility(viewer, ScrollBarVisibility.Auto);
            }
        }       
        private void Del_sticker_Click(object sender, RoutedEventArgs e)
        {                     
                if (MessageBox.Show("Usuniecie naklejki bedzie trwałe i nieodwracalne, NIE BEDZIE MOŻNA JEJ WIECEJ UŻYĆ", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Process.Start(@"Resources\");
                }         
        }

        private void DragImage(object sender, MouseButtonEventArgs e)
        {
            source = (Image)sender;
            DependencyObject parentObject = VisualTreeHelper.GetParent(source);           
            source = e.Source as Image;
            DataObject data = new DataObject(typeof(ImageSource), source.Source);
            DragDrop.DoDragDrop(source, data, DragDropEffects.Copy);
            if (parentObject == naklejki)
            {
                source = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Image imgTemp;
            List<string> lstFileNames = new List<string>(System.IO.Directory.EnumerateFiles(@"Resources/", " *.png"));
            foreach (string fileName in lstFileNames)
            {
                imgTemp = new Image();
                imgTemp.Source = new BitmapImage(new Uri(fileName));
                imgTemp.Height = imgTemp.Width = 100;                         
            }
        }

        private void DropImage(object sender, DragEventArgs e)

        {
            ImageSource source2 = e.Data.GetData(typeof(ImageSource)) as ImageSource;

            Image img = new Image()
            {
                Width = 64,
                Height = 64,
                Source = source2,
                Stretch = Stretch.Uniform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(0),
                Focusable = true,                
            };
            img.MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDown);
            img.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp);            
            img.MouseMove += new MouseEventHandler(MouseMove);
            img.KeyDown += new KeyEventHandler(Canvas_KeyDown);
            img.MouseLeave += new MouseEventHandler(xd);

            Canvas.SetLeft(img, (e.GetPosition(this.Canvas_main).X)-(img.Width/2));

            Canvas.SetTop(img, (e.GetPosition(this.Canvas_main).Y)-(img.Height/2));
            foreach (UIElement child in Canvas_main.Children)
            {
                xds++;
            }
            this.Canvas_main.Children.Add(img);
           

        }
        public void xd(object sender, MouseEventArgs e)
        {
            foreach (UIElement child in Canvas_main.Children)
            {
                int a = 0;
                a++;
                ads=a;
            }
            if (ads < xds){
                source = (Image)sender;
            }
            foreach (UIElement child in Canvas_main.Children)
            {
                int a = 0;
                a++;
                xds=a;
            }
        }
        private void load_stickers()
        {
            int fileCount = (from file in Directory.EnumerateFiles(@"Resources/", "*", SearchOption.AllDirectories) select file).Count();
            int sticker_nr = 0;
            int stickers_in_dir = fileCount;
            while (stickers_in_dir != 0)
            {
                try
                {
                    while (stickers_in_dir != 0)
                    {
                        string mystring = sticker_nr.ToString();
                        Image img2 = new Image
                        {
                            MaxWidth = 66,
                            MaxHeight = 66,
                            MinWidth = 66,
                            MinHeight = 66,
                            Margin =new Thickness(6),
                            Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/Sticker_" + mystring + ".png")),
                            Focusable = false,
                            Name = "Sticker_" + mystring,
                        };
                        img2.MouseLeftButtonDown += new MouseButtonEventHandler(DragImage);                      
                        this.naklejki.Children.Add(img2);
                        sticker_nr++;
                    }

                }
                catch (System.IO.FileNotFoundException)
                {
                    //MessageBox.Show("Wystapił problem z ładowaniem naklejek :c");
                    sticker_nr++;
                }
                stickers_in_dir--;
            }
        }

        private void Add_text_Click(object sender, RoutedEventArgs e)
        {
            Txt1.Text = textBox1.Text;
            Txt1_2.Text = textBox1.Text;
            Txt1.MaxWidth = Canvas_main.ActualWidth;
            Txt1.MaxHeight = Canvas_main.ActualHeight;
            Txt1_2.MaxWidth = Canvas_main.ActualWidth;
            Txt1_2.MaxHeight = Canvas_main.ActualHeight;
            Txt1.Visibility = Visibility.Visible;
            Txt1_2.Visibility = Visibility.Visible;
            Txt2.Text = textBox2.Text;
            Txt2_2.Text = textBox2.Text;
            Txt2.MaxWidth = Canvas_main.ActualWidth;
            Txt2.MaxHeight = Canvas_main.ActualHeight;
            Txt2_2.MaxWidth = Canvas_main.ActualWidth;
            Txt2_2.MaxHeight = Canvas_main.ActualHeight;
            Txt2.Visibility = Visibility.Visible;
            Txt2_2.Visibility = Visibility.Visible;
            Canvas.SetLeft(Txt1_2, (Canvas_main.ActualWidth / 2) - Txt1_2.ActualWidth / 2);
            Canvas.SetLeft(Txt1, (Canvas_main.ActualWidth / 2) - Txt1.ActualWidth / 2);
            Canvas.SetLeft(Txt2_2, (Canvas_main.ActualWidth / 2) - Txt2_2.ActualWidth / 2);
            Canvas.SetLeft(Txt2, (Canvas_main.ActualWidth / 2) - Txt2.ActualWidth / 2);
            Canvas.SetTop(Txt2_2, (Canvas_main.ActualHeight) - Txt2_2.ActualHeight);
            Canvas.SetTop(Txt2, (Canvas_main.ActualHeight) - Txt2.ActualHeight);
        }

        private void Hide_Text_Click(object sender, RoutedEventArgs e)
        {
            Txt2.Visibility = Visibility.Hidden;
            Txt2_2.Visibility = Visibility.Hidden;
            Txt1.Visibility = Visibility.Hidden;
            Txt1_2.Visibility = Visibility.Hidden;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
            {
                source_text.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
                source_text.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
                source_text.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
                source_text.Foreground = new SolidColorBrush(Colors.Blue);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
                source_text.Foreground = new SolidColorBrush(Colors.Yellow);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (source_text != null)
                source_text.Foreground = new SolidColorBrush(Colors.Green);           
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {                   
         Txt2_2.FontSize++;         
         Txt2.FontSize++;         
         Txt1_2.FontSize++;
         Txt1.FontSize++;
            Canvas.SetLeft(Txt1_2, (Canvas_main.ActualWidth / 2) - Txt1_2.ActualWidth / 2);
            Canvas.SetLeft(Txt1, (Canvas_main.ActualWidth / 2) - Txt1.ActualWidth / 2);
            Canvas.SetLeft(Txt2_2, (Canvas_main.ActualWidth / 2) - Txt2_2.ActualWidth / 2);
            Canvas.SetLeft(Txt2, (Canvas_main.ActualWidth / 2) - Txt2.ActualWidth / 2);
            Canvas.SetTop(Txt2_2, (Canvas_main.ActualHeight) - Txt2_2.ActualHeight);
            Canvas.SetTop(Txt2, (Canvas_main.ActualHeight) - Txt2.ActualHeight);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {

            if (Txt1.FontSize >= 8)
            {
                Txt2_2.FontSize--;
                Txt2.FontSize--;
                Txt1_2.FontSize--;
                Txt1.FontSize--;
                Canvas.SetLeft(Txt1_2, (Canvas_main.ActualWidth / 2) - Txt1_2.ActualWidth / 2);
                Canvas.SetLeft(Txt1, (Canvas_main.ActualWidth / 2) - Txt1.ActualWidth / 2);
                Canvas.SetLeft(Txt2_2, (Canvas_main.ActualWidth / 2) - Txt2_2.ActualWidth / 2);
                Canvas.SetLeft(Txt2, (Canvas_main.ActualWidth / 2) - Txt2.ActualWidth / 2);
                Canvas.SetTop(Txt2_2, (Canvas_main.ActualHeight) - Txt2_2.ActualHeight);
                Canvas.SetTop(Txt2, (Canvas_main.ActualHeight) - Txt2.ActualHeight);
            }
           
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(Txt1_2, (Canvas_main.ActualWidth / 2) - Txt1_2.ActualWidth / 2);
            Canvas.SetLeft(Txt1, (Canvas_main.ActualWidth / 2) - Txt1.ActualWidth / 2);
            Canvas.SetLeft(Txt2_2, (Canvas_main.ActualWidth / 2) - Txt2_2.ActualWidth / 2);
            Canvas.SetLeft(Txt2, (Canvas_main.ActualWidth / 2) - Txt2.ActualWidth / 2);
            Canvas.SetTop(Txt2_2, (Canvas_main.ActualHeight) - Txt2_2.ActualHeight);
            Canvas.SetTop(Txt2, (Canvas_main.ActualHeight) - Txt2.ActualHeight);
        }
            public void MouseLeftButtonDown_text(object sender, MouseButtonEventArgs e)
        {
            source = null;
            source_text = (TextBlock)sender;
            Mouse.Capture(source_text);
            captured = true;
            x_shape = Canvas.GetLeft(source_text);
            x_canvas = e.GetPosition(Canvas_main).X;
            y_shape = Canvas.GetTop(source_text);
            y_canvas = e.GetPosition(Canvas_main).Y;          
            source_text.Focus();          
        }
        public void MouseMove_text(object sender, MouseEventArgs e)
        {
            if (source == null)
            {
                if (captured)
                {                   
                    double x = e.GetPosition(Canvas_main).X;
                    double y = e.GetPosition(Canvas_main).Y;
                    x_shape += x - x_canvas;
                    if (source_text == Txt1_2)
                    {
                        Canvas.SetLeft(Txt1, x_shape);
                        Canvas.SetTop(Txt1, y_shape);
                    }else if(source_text == Txt1){
                        Canvas.SetLeft(Txt1_2, x_shape);
                        Canvas.SetTop(Txt1_2, y_shape);
                    }
                    if (source_text == Txt2)
                    {
                        Canvas.SetLeft(Txt2_2, x_shape);
                        Canvas.SetTop(Txt2_2, y_shape);
                    }
                    else if (source_text == Txt2_2)
                    {
                        Canvas.SetLeft(Txt2, x_shape);
                        Canvas.SetTop(Txt2, y_shape);
                    }
                    Canvas.SetLeft(source_text, x_shape);
                    x_canvas = x;
                    y_shape += y - y_canvas;
                    Canvas.SetTop(source_text, y_shape);                  
                    y_canvas = y;
                   
                }
            }
        }
        public void MouseLeftButtonUp_text(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }
        
    } 
    
}




