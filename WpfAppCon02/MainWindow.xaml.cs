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
using Data;
using System.Collections.ObjectModel;

namespace WpfAppCon02
{
    public partial class MainWindow : Window
    {
        private Car selectedCar;
        private bool carSelectionChangedEventDisabledFlag = false;
        private Database db = null;
        private ObservableCollection<Car> obsCars = null;
        private ObservableCollection<Owner> obsOwners = null;
        private ObservableCollection<Sale> obsSales = null;
        private bool otherFlag=false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMyComponent();
        }

        private void InitializeMyComponent()
        {
            try
            {                
                db = Database.NewInstance(comboBox.SelectedIndex==0);
                MessageBox.Show("connected");
                obsCars = new ObservableCollection<Car>();
                obsOwners = new ObservableCollection<Owner>();
                obsSales = new ObservableCollection<Sale>();
                listCars.ItemsSource = obsCars;
                listOwners.ItemsSource = obsOwners;
                listSales.ItemsSource = obsSales;
                FillObsCars();
                FillObsOwners();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #region Fill various obs lists methods
        private void FillObsSales(Owner o)
        {
            obsSales.Clear();
            foreach (Sale s in db.Read_Sales_from_DB(o))
                obsSales.Add(s);
        }
        private void FillObsOwners()
        {
            obsOwners.Clear();
            foreach (Owner o in db.Read_Owners_From_DB())
                obsOwners.Add(o);
        }
        private void FillObsCars()
        {
            listCars.SelectionChanged -= listCars_SelectionChanged;
            carSelectionChangedEventDisabledFlag = true;
            obsCars.Clear();
            foreach (Car car in db.Read_Cars_from_DB())
                obsCars.Add(car);
            carSelectionChangedEventDisabledFlag = false;
            listCars.SelectionChanged += listCars_SelectionChanged;
        }
        #endregion

        #region SelectionChanged Events
        private void ComboBoxTransactionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (db != null)
                db.IsolationLevel = comboBoxTransactionMode.SelectedIndex == 0 ? System.Data.IsolationLevel.ReadCommitted : System.Data.IsolationLevel.Serializable;
        }
        /// <summary>
        /// Pfusch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (otherFlag == true)
                    return;
                if (!carSelectionChangedEventDisabledFlag)
                {
                    otherFlag = true;
                    btn_update.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    selectedCar = (Car)listCars.SelectedItem;
                    if (selectedCar == null) return;
                    int selectedIndex = listCars.SelectedIndex;
                    FillObsCars();
                    carSelectionChangedEventDisabledFlag = true;
                    db.SwitchCarTransaction(selectedCar);                  
                    listCars.CurrentItem = listCars.Items[selectedIndex];
                    selectedCar = (Car)listCars.Items[selectedIndex];
                    textId.Text = selectedCar?.CarId.ToString();
                    textName.Text = selectedCar?.CarName;
                    textMessages.Text = selectedCar?.ToString() + " selected";
                    textBestand.Text = selectedCar?.Bestand.ToString();
                    otherFlag = false;
                    carSelectionChangedEventDisabledFlag = false;
                }
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
            }
            
        }
        private void ListOwners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Owner o = (Owner)listOwners.SelectedItem;
            FillObsSales(o);
        }
        #endregion

        #region Car Operation Events
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Car car = new Car(int.Parse(textId.Text), textName.Text, int.Parse(textBestand.Text));
                db.AddCar(car);
                FillObsCars();
                textMessages.Text = "Car added!";
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
            }
        }
        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(textId.Text);
                string name = textName.Text;
                int bestand = int.Parse(textBestand.Text);
                db.UpdateCar(new Car(id, name, bestand));
                textMessages.Text = "Car updated!";
                FillObsCars();
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedCar == null)
                    throw new Exception("No car selected");
                db.RemoveCar(selectedCar);
                textMessages.Text = "Car deleted!";
                FillObsCars();
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
            }           
        }
        #endregion

        #region Sales Operation Click Events
        private void BtnResale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnResale.IsEnabled = false;
                btnSale.IsEnabled = false;
                if (listSales.SelectedItem == null)
                    throw new Exception("no sale selected");
                db.ExecuteResaleTransaction((Sale)listSales.SelectedItem);
                FillObsSales((Owner)listOwners.SelectedItem);
                FillObsCars();
                textMessages.Text = "Resale commited!";
                btnSale.IsEnabled = true;
                btnResale.IsEnabled = true;
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
                btnSale.IsEnabled = true;
                btnResale.IsEnabled = true;
            }
        }
        private void BtnSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSale.IsEnabled = false;
                btnResale.IsEnabled = false;
                if (listOwners.SelectedItem == null)
                    throw new Exception("no owner selected");
                if (listCars.SelectedItem == null)
                    throw new Exception("no car selected");
                db.ExecuteSaleTransaction(new Sale(((Car)listCars.SelectedItem).CarId, ((Owner)listOwners.SelectedItem).ONr));
                FillObsSales((Owner)listOwners.SelectedItem);
                FillObsCars();
                textMessages.Text = "Sale commited!";
                btnSale.IsEnabled = true;
                btnResale.IsEnabled = true;
            }
            catch (Exception ex)
            {
                textMessages.Text = ex.Message;
                btnSale.IsEnabled = true;
                btnResale.IsEnabled = true;
            }
        }
        #endregion

        private void BtnReconnect_Click(object sender, RoutedEventArgs e)
        {
            db = Database.NewInstance(comboBox.SelectedIndex == 0);
            MessageBox.Show("connected");
            obsCars = new ObservableCollection<Car>();
            obsOwners = new ObservableCollection<Owner>();
            obsSales = new ObservableCollection<Sale>();
            listCars.ItemsSource = obsCars;
            listOwners.ItemsSource = obsOwners;
            listSales.ItemsSource = obsSales;
            FillObsCars();
            FillObsOwners();
        }
    }
}