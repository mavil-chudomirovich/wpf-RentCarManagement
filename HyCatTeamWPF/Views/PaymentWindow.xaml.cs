using HyCatTeamWPF.ApiServices;
using HyCatTeamWPF.Constant;
using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System.Windows;
using System.Windows.Media;

namespace HyCatTeamWPF.Views
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private Guid _contractId;
        private readonly RentalContractApiService _rentalService;
        private readonly InvoiceApiService _invoiceService;

        public PaymentWindow(Guid contractId)
        {
            InitializeComponent();
            _contractId = contractId;
            _rentalService = new RentalContractApiService(ApiClient.Client);
            _invoiceService = new InvoiceApiService(ApiClient.Client);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = await _rentalService.GetContractById(_contractId);
                var handoverInvoice = c.Invoices.Where(i => i.Type == (int)InvoiceType.Handover).FirstOrDefault();
                if (c != null && handoverInvoice != null)
                {
                    TxtId.Text = handoverInvoice.Id.ToString();
                    TxtDeposit.Text = handoverInvoice.Deposit?.Amount.ToString() ?? "0";
                    TxtSubtotal.Text = handoverInvoice.Subtotal.ToString();
                    TxtTax.Text = handoverInvoice.Tax.ToString();
                    TxtItemSubtotal.Text = handoverInvoice.InvoiceItems.Sum(ii => ii.SubTotal).ToString();
                    TxtTotalAmount.Text = handoverInvoice.Total.ToString();
                    TxtStatus.Foreground = Brushes.Black;
                    switch (handoverInvoice.Status)
                    {
                        case (int)InvoiceStatus.Pending:
                            TxtStatus.Text = "Pending";
                            TxtStatus.Background = Brushes.Yellow;
                            break;

                        case (int)InvoiceStatus.Paid:
                            TxtStatus.Text = "Paid";
                            TxtStatus.Background = Brushes.LightGreen;
                            break;

                        case (int)InvoiceStatus.Cancelled:
                            TxtStatus.Text = "Cancelled";
                            TxtStatus.Background = Brushes.Red;
                            break;

                        default:
                            TxtStatus.Text = "Unknown";
                            TxtStatus.Background = Brushes.White;
                            break;
                    }
                    if (handoverInvoice.Status != (int)InvoiceStatus.Pending)
                    {
                        BtnPayment.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnPayment_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtPaymount.Text))
                {
                    MessageBox.Show("Pls input pay amount", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (decimal.TryParse(TxtPaymount.Text, out var payAmount))
                {
                    var req = new PaymentReq()
                    {
                        PaymentMethod = (int)PaymentMethod.Cash,
                        Amount = payAmount,
                        FallbackUrl = "https://www.youtube.com/watch?v=dWVoV-eZWEE"
                    };
                    var invoiceId = Guid.Parse(TxtId.Text);
                    await _invoiceService.PaymentInvoice(invoiceId, req);
                    MessageBox.Show("Payment successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAsync(Guid.Parse(TxtId.Text));
                }
                else
                {
                    MessageBox.Show("Invalid format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAsync(Guid id)
        {
            var handoverInvoice = await _invoiceService.GetInvoiceById(id);
            if (handoverInvoice != null)
            {
                TxtId.Text = handoverInvoice.Id.ToString();
                TxtDeposit.Text = handoverInvoice.Deposit?.Amount.ToString() ?? "0";
                TxtSubtotal.Text = handoverInvoice.Subtotal.ToString();
                TxtTax.Text = handoverInvoice.Tax.ToString();
                TxtItemSubtotal.Text = handoverInvoice.InvoiceItems.Sum(ii => ii.SubTotal).ToString();
                TxtTotalAmount.Text = handoverInvoice.Total.ToString();
                TxtStatus.Foreground = Brushes.Black;
                switch (handoverInvoice.Status)
                {
                    case (int)InvoiceStatus.Pending:
                        TxtStatus.Text = "Pending";
                        TxtStatus.Background = Brushes.Yellow;
                        break;

                    case (int)InvoiceStatus.Paid:
                        TxtStatus.Text = "Paid";
                        TxtStatus.Background = Brushes.LightGreen;
                        break;

                    case (int)InvoiceStatus.Cancelled:
                        TxtStatus.Text = "Cancelled";
                        TxtStatus.Background = Brushes.Red;
                        break;

                    default:
                        TxtStatus.Text = "Unknown";
                        TxtStatus.Background = Brushes.White;
                        break;
                }
                if (handoverInvoice.Status != (int)InvoiceStatus.Pending)
                {
                    BtnPayment.IsEnabled = false;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}