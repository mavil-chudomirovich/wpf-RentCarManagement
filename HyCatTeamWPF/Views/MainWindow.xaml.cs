using HyCatTeamWPF.ApiServices;
using HyCatTeamWPF.Constant;
using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using HyCatTeamWPF.Models.HyCatTeamWPF.Models;
using HyCatTeamWPF.Views;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace HyCatTeamWPF
{
    public partial class MainWindow : Window
    {
        private readonly StationApiService _stationService;
        private readonly UserApiService _userService;
        private readonly VehicleApiService _vehicleService;
        private readonly RentalContractApiService _rentalService;
        private Guid? _selectedModelId = null;
        private UserProfileViewRes User = null;

        public MainWindow()
        {
            InitializeComponent();

            _stationService = new StationApiService(ApiClient.Client);
            _userService = new UserApiService(ApiClient.Client);
            _vehicleService = new VehicleApiService(ApiClient.Client);
            _rentalService = new RentalContractApiService(ApiClient.Client);
            LoadUserHeader();
            LoadStations();
            sidebarHome.Background = Brushes.Green;
            contractTitle.Visibility = Visibility.Collapsed;
            ContractPanel.Visibility = Visibility.Collapsed;
            BtnReload.Visibility = Visibility.Collapsed;
        }

        // Load stations
        private async void LoadStations()
        {
            var stations = await _stationService.GetAllStationsAsync();

            if (stations == null || stations.Count == 0)
            {
                MessageBox.Show("Station not found",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CboStation.ItemsSource = stations;
        }

        // Tạo chữ cái fallback
        private string GetInitial(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            return name.Substring(0, 1).ToUpper();
        }

        // ✅ Load avatar + role header
        private async void LoadUserHeader()
        {
            try
            {
                var user = await _userService.GetMeAsync();
                if (user == null) return;
                User = user;
                // Set full name
                TxtFullName.Text = $"{user.FirstName} {user.LastName}";

                // ✅ Set fallback chữ cái
                string initial = GetInitial(user.FirstName);
                TxtAvatarLetter.Text = initial;

                try
                {
                    if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                    {
                        // ✅ Nếu là GIF
                        if (user.AvatarUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                        {
                            var gif = new BitmapImage();
                            gif.BeginInit();
                            gif.UriSource = new Uri(user.AvatarUrl);
                            gif.CacheOption = BitmapCacheOption.OnLoad;
                            gif.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            gif.EndInit();

                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(ImgAvatar, gif);

                            ImgAvatar.Visibility = Visibility.Visible;
                            TxtAvatarLetter.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            // ✅ PNG/JPG
                            var img = new BitmapImage();
                            img.BeginInit();
                            img.UriSource = new Uri(user.AvatarUrl);
                            img.CacheOption = BitmapCacheOption.OnLoad;
                            img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            img.EndInit();

                            ImgAvatar.Source = img;

                            ImgAvatar.Visibility = Visibility.Visible;
                            TxtAvatarLetter.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        // ✅ Không có Avatar → fallback
                        ImgAvatar.Visibility = Visibility.Collapsed;
                        TxtAvatarLetter.Visibility = Visibility.Visible;
                    }
                    if (User.Role.Name == "Customer")
                    {
                        await LoadMyContracts();
                    }
                    else
                    {
                        await LoadAllContracts();
                    }
                }
                catch
                {
                    // ✅ Avatar lỗi → fallback chữ cái
                    ImgAvatar.Visibility = Visibility.Collapsed;
                    TxtAvatarLetter.Visibility = Visibility.Visible;
                }

                // ✅ Role badge
                TxtRole.Text = user.Role?.Name ?? "Unknown";

                switch (user.Role?.Name)
                {
                    case "Admin":
                        RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d97706"));
                        break;

                    case "Staff":
                        RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b82f6"));
                        break;

                    default:
                        RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user profile: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra null
                if (CboStation.SelectedValue == null ||
                    DateStart.SelectedDate == null ||
                    DateEnd.SelectedDate == null)
                {
                    MessageBox.Show("Please fill all fields", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime start = DateStart.SelectedDate.Value;
                DateTime end = DateEnd.SelectedDate.Value;

                // Kiểm tra start < end
                if (start >= end)
                {
                    MessageBox.Show("Start date must be earlier than End date.",
                        "Invalid Date Range",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Guid stationId = (Guid)CboStation.SelectedValue;

                //  Convert chuẩn DateTimeOffset
                var startDate = new DateTimeOffset(start, TimeSpan.Zero);
                var endDate = new DateTimeOffset(end, TimeSpan.Zero);

                //  Gọi API
                var result = await _vehicleService.SearchVehiclesAsync(stationId, startDate, endDate);

                //  Render UI
                RenderVehicleCards(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching vehicles: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        private void RenderVehicleCards(List<VehicleModelViewRes> vehicles)
        {

            VehiclePanel.Children.Clear();

            foreach (var v in vehicles)
            {
                if (v.AvailableVehicleCount == 0)
                    continue;

                var card = new Border
                {
                    Width = 260,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(12),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 8,
                        ShadowDepth = 2,
                        Opacity = 0.25
                    },
                    Tag = v.Id // Thêm ID của vehicle vào thuộc tính Tag của card
                };

                // Thêm sự kiện click vào card
                card.MouseLeftButtonDown += Card_MouseLeftButtonDown; // Sự kiện cho mỗi card

                var stack = new StackPanel { Orientation = Orientation.Vertical };

                // Ảnh
                var img = new Image
                {
                    Height = 150,
                    Margin = new Thickness(0, 0, 0, 10),
                    Stretch = Stretch.UniformToFill,
                    Source = new BitmapImage(new Uri(v.ImageUrl ?? "")),
                    ClipToBounds = true
                };

                // Tên xe
                var nameTxt = new TextBlock
                {
                    Text = v.Name,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Margin = new Thickness(0, 5, 0, 2)
                };

                // Brand
                var brandTxt = new TextBlock
                {
                    Text = v.Brand.Name,
                    Foreground = Brushes.Gray,
                    FontSize = 13
                };

                // Giá thuê
                var priceTxt = new TextBlock
                {
                    Text = $"{v.CostPerDay:N0}₫ / day",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Green,
                    Margin = new Thickness(0, 6, 0, 2)
                };

                // Số lượng xe
                var countTxt = new TextBlock
                {
                    Text = $"Available: {v.AvailableVehicleCount}",
                    FontSize = 13,
                    Foreground = Brushes.DarkBlue
                };

                stack.Children.Add(img);
                stack.Children.Add(nameTxt);
                stack.Children.Add(brandTxt);
                stack.Children.Add(priceTxt);
                stack.Children.Add(countTxt);

                card.Child = stack;

                VehiclePanel.Children.Add(card);
            }
        }

        private void Card_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Deselect tất cả các card trước khi chọn card mới
            foreach (var child in VehiclePanel.Children)
            {
                if (child is Border card)
                {
                    card.Background = Brushes.White; // Đặt lại màu nền cho các card khác
                }
            }

            // Lấy card được click
            var clickedCard = sender as Border;
            if (clickedCard != null)
            {
                // Lấy Vehicle ID từ Tag
                var selectedVehicleId = (Guid)clickedCard.Tag;

                // Thay đổi màu nền của card được chọn
                clickedCard.Background = Brushes.LightGreen; // Làm nổi bật card đã chọn
                _selectedModelId = (Guid)clickedCard.Tag;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check Station
                if (CboStation.SelectedValue == null)
                {
                    MessageBox.Show("Please choose a station.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Guid stationId = (Guid)CboStation.SelectedValue;

                //Check Start Date
                if (DateStart.SelectedDate == null)
                {
                    MessageBox.Show("Please select a start date.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DateTime startDate = DateStart.SelectedDate.Value;

                //Check End Date
                if (DateEnd.SelectedDate == null)
                {
                    MessageBox.Show("Please select an end date.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DateTime endDate = DateEnd.SelectedDate.Value;

                //Validate date range
                if (startDate >= endDate)
                {
                    MessageBox.Show("Start date must be earlier than end date.",
                                    "Invalid date range",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                //Check selected vehicle
                if (_selectedModelId == null)
                {
                    MessageBox.Show("Please select a vehicle.",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }
                Guid modelId = _selectedModelId.Value;

                //Convert to formatted string
                string startStr = startDate.ToString("yyyy-MM-ddTHH:mm:ss");
                string endStr = endDate.ToString("yyyy-MM-ddTHH:mm:ss");

                //Convert to DateTimeOffset (UTC)
                var startOffset = DateTimeOffset.Parse(startStr + "+00:00");
                var endOffset = DateTimeOffset.Parse(endStr + "+00:00");

                //Tạo request DTO
                var request = new CreateRentalContractReq
                {
                    ModelId = modelId,
                    StationId = stationId,
                    StartDate = startOffset,
                    EndDate = endOffset
                };

                //Gọi API
                await _rentalService.CreateRentalContractAsync(request);
                await LoadMyContracts();
                MessageBox.Show("Rental contract created successfully!",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating rental contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        private async Task LoadMyContracts()
        {
            var contracts = await _rentalService.GetMyContractsAsync();

            ContractPanel.Children.Clear();

            foreach (var c in contracts)
            {
                var border = new Border
                {
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 6,
                        ShadowDepth = 2,
                        Opacity = 0.2
                    }
                };

                var stack = new StackPanel();

                // ==== INFO ====
                string modelName = c.Vehicle == null ? "Unknown Model" : c.Vehicle.Model?.Name!;

                // ✅ Model Name (title + value)
                var modelNameText = new TextBlock();
                modelNameText.Inlines.Add(new Run("Model: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                modelNameText.Inlines.Add(new Run(modelName)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(modelNameText);


                // ✅ From Date
                var fromText = new TextBlock { Margin = new Thickness(0, 5, 0, 0) };
                fromText.Inlines.Add(new Run("From: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                fromText.Inlines.Add(new Run($"{c.StartDate:dd/MM/yyyy}")
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(fromText);


                // ✅ To Date
                var toText = new TextBlock();
                toText.Inlines.Add(new Run("To: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                toText.Inlines.Add(new Run($"{c.EndDate:dd/MM/yyyy}")
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(toText);
                // ✅ License Plate
                var licensePlate = c.Vehicle == null ? "N/A" : c.Vehicle.LicensePlate;
                var licenseText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                licenseText.Inlines.Add(new Run("License Plate: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                licenseText.Inlines.Add(new Run(licensePlate)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(licenseText);
                string status = "";
                switch (c.Status)
                {
                    case (int)RentalContractStatus.RequestPeding:
                        {
                            status = "Request Pending";
                            break;
                        }
                    case (int)RentalContractStatus.PaymentPending:
                        {
                            status = "Payment Pending";
                            break;
                        }
                    case (int)RentalContractStatus.Active:
                        {
                            status = "Active None Use";
                            if (c.IsSignedByCustomer && c.IsSignedByStaff)
                            {
                                status = "Active In Use";
                            }
                            break;
                        }
                    case (int)RentalContractStatus.Returned:
                        {
                            status = "Returned";
                            break;
                        }
                    case (int)RentalContractStatus.Completed:
                        {
                            status = "Complete";
                            break;
                        }
                    case (int)RentalContractStatus.Cancelled:
                        {
                            status = "Cancel";
                            break;
                        }
                }
                var color = Brushes.Red;
                switch (c.Status)
                {
                    case (int)RentalContractStatus.RequestPeding:
                        {
                            color = Brushes.Green;
                            break;
                        }
                    case (int)RentalContractStatus.PaymentPending:
                        {
                            color = Brushes.Orange;
                            break;
                        }
                    case (int)RentalContractStatus.Active:
                        {
                            color = Brushes.Blue;
                            break;
                        }
                    case (int)RentalContractStatus.Returned:
                        {
                            color = Brushes.Purple;
                            break;
                        }
                    case (int)RentalContractStatus.Completed:
                        {
                            color = Brushes.Green;
                            break;
                        }
                    case (int)RentalContractStatus.Cancelled:
                        {
                            color = Brushes.Red;
                            break;
                        }
                }
                // ✅ Status (Inline) – giữ màu theo logic switch
                var statusText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                statusText.Inlines.Add(new Run("Status: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                statusText.Inlines.Add(new Run(status)
                {
                    Foreground = color, // ✅ màu logic khầy chọn

                });
                stack.Children.Add(statusText);

                var descText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                descText.Inlines.Add(new Run("Description: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                descText.Inlines.Add(new Run(c.Description)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(descText);


                var btnPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                // ✅ Accept button
                var cancelBtn = new Button
                {
                    Content = "✖",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, true)
                };
                cancelBtn.Click += CancelContract_Click;
                var handoverBtn = new Button
                {
                    Content = "Handover",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, true),
                    Width = 200
                };
                handoverBtn.Click += Customer_Handover_Click;
                if (c.Status != (int)RentalContractStatus.PaymentPending && c.Status != (int)RentalContractStatus.RequestPeding)
                {
                    cancelBtn.Visibility = Visibility.Collapsed;
                }
                if (c.Status != (int)RentalContractStatus.Active || status == "Active In Use")
                {
                    handoverBtn.Visibility = Visibility.Collapsed;
                }
                //add button
                btnPanel.Children.Add(cancelBtn);
                btnPanel.Children.Add(handoverBtn);

                stack.Children.Add(btnPanel);

                border.Child = stack;
                ContractPanel.Children.Add(border);
            }
        }

        private async Task LoadAllContracts()
        {
            var contracts = await _rentalService.GetAllContractsAsync();

            ContractPanel.Children.Clear();

            foreach (var c in contracts)
            {
                var border = new Border
                {
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 6,
                        ShadowDepth = 2,
                        Opacity = 0.2
                    }
                };

                var stack = new StackPanel();


                // ==== INFO ====
                string modelName = c.Vehicle == null ? "Unknown Model" : c.Vehicle.Model?.Name!;

                // ✅ Model Name (title + value)
                var modelNameText = new TextBlock();
                modelNameText.Inlines.Add(new Run("Model: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                modelNameText.Inlines.Add(new Run(modelName)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(modelNameText);


                // ✅ From Date
                var fromText = new TextBlock { Margin = new Thickness(0, 5, 0, 0) };
                fromText.Inlines.Add(new Run("From: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                fromText.Inlines.Add(new Run($"{c.StartDate:dd/MM/yyyy}")
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(fromText);


                // ✅ To Date
                var toText = new TextBlock();
                toText.Inlines.Add(new Run("To: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                toText.Inlines.Add(new Run($"{c.EndDate:dd/MM/yyyy}")
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(toText);


                // ✅ License Plate
                var licensePlate = c.Vehicle == null ? "N/A" : c.Vehicle.LicensePlate;
                var licenseText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                licenseText.Inlines.Add(new Run("License Plate: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                licenseText.Inlines.Add(new Run(licensePlate)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(licenseText);

                string status = "";
                switch (c.Status)
                {
                    case (int)RentalContractStatus.RequestPeding:
                        {
                            status = "Request Pending";
                            break;
                        }
                    case (int)RentalContractStatus.PaymentPending:
                        {
                            status = "Payment Pending";
                            break;
                        }
                    case (int)RentalContractStatus.Active:
                        {
                            status = "Active None Use";
                            if (c.IsSignedByCustomer && c.IsSignedByStaff)
                            {
                                status = "Active In Use";
                            }
                            break;
                        }
                    case (int)RentalContractStatus.Returned:
                        {
                            status = "Returned";
                            break;
                        }
                    case (int)RentalContractStatus.Completed:
                        {
                            status = "Complete";
                            break;
                        }
                    case (int)RentalContractStatus.Cancelled:
                        {
                            status = "Cancel";
                            break;
                        }
                }
                var color = Brushes.Red;
                switch (c.Status)
                {
                    case (int)RentalContractStatus.RequestPeding:
                        {
                            color = Brushes.Green;
                            break;
                        }
                    case (int)RentalContractStatus.PaymentPending:
                        {
                            color = Brushes.Orange;
                            break;
                        }
                    case (int)RentalContractStatus.Active:
                        {
                            color = Brushes.Blue;
                            break;
                        }
                    case (int)RentalContractStatus.Returned:
                        {
                            color = Brushes.Purple;
                            break;
                        }
                    case (int)RentalContractStatus.Completed:
                        {
                            color = Brushes.Green;
                            break;
                        }
                    case (int)RentalContractStatus.Cancelled:
                        {
                            color = Brushes.Red;
                            break;
                        }
                }

                // ✅ Customer Name (Inline)
                var customerText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                customerText.Inlines.Add(new Run("Customer Name: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                customerText.Inlines.Add(new Run($"{c.Customer.FirstName} {c.Customer.LastName}")
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(customerText);


                // ✅ Status (Inline) – giữ màu theo logic switch
                var statusText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                statusText.Inlines.Add(new Run("Status: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                statusText.Inlines.Add(new Run(status)
                {
                    Foreground = color, // ✅ màu logic khầy chọn

                });
                stack.Children.Add(statusText);


                // ✅ Description (Inline)
                var descText = new TextBlock { Margin = new Thickness(0, 5, 0, 10) };
                descText.Inlines.Add(new Run("Description: ")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });
                descText.Inlines.Add(new Run(c.Description)
                {
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#000000"),

                });
                stack.Children.Add(descText);

                // === CUSTOMER SIGN CHECKBOX ===
                var customerSignPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var customerSignCheckbox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var customerSignLabel = new TextBlock();
                customerSignLabel.Inlines.Add(new Run("Customer Sign")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });

                customerSignPanel.Children.Add(customerSignCheckbox);
                customerSignPanel.Children.Add(customerSignLabel);

                stack.Children.Add(customerSignPanel);


                // === STAFF SIGN CHECKBOX ===
                var staffSignPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 15)
                };

                var staffSignCheckbox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var staffSignLabel = new TextBlock();
                staffSignLabel.Inlines.Add(new Run("Staff Sign")
                {
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                });

                staffSignPanel.Children.Add(staffSignCheckbox);
                staffSignPanel.Children.Add(staffSignLabel);

                stack.Children.Add(staffSignPanel);

                if (c.Status != (int)RentalContractStatus.Active || status == "Active In Use")
                {
                    staffSignPanel.Visibility = Visibility.Collapsed;
                    customerSignPanel.Visibility = Visibility.Collapsed;
                }
                // ==== ACTION BUTTONS ====
                var btnPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                // ✅ Accept button
                var acceptBtn = new Button
                {
                    Content = "✔",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, true)
                };
                acceptBtn.Click += AcceptContract_Click;

                // ❌ Reject button
                var rejectBtn = new Button
                {
                    Content = "✖",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, false)
                };
                rejectBtn.Click += RejectContract_Click;

                // Payment invoice button
                var paymentBtn = new Button
                {
                    Content = "💰",
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FFD700")
                    ),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, true)
                };
                paymentBtn.Click += PaymentContract_Click;

                // Payment invoice button
                var handoverBtn = new Button
                {
                    Content = "Handover",
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#10B981")),
                    Style = (Style)FindResource("IconButton"),
                    Tag = new ContractActionData(c.Id, true)
                    {
                        CustomerSignCheckbox = customerSignCheckbox,
                        StaffSignCheckbox = staffSignCheckbox
                    },
                    Width = 200
                };
                handoverBtn.Click += Staff_Handover_Click;

                btnPanel.Children.Add(acceptBtn);
                btnPanel.Children.Add(rejectBtn);
                btnPanel.Children.Add(paymentBtn);
                btnPanel.Children.Add(handoverBtn);

                if (c.Status != (int)RentalContractStatus.PaymentPending)
                {
                    paymentBtn.Visibility = Visibility.Collapsed;
                }
                if (c.Status != (int)RentalContractStatus.RequestPeding)
                {
                    acceptBtn.Visibility = Visibility.Collapsed;
                    rejectBtn.Visibility = Visibility.Collapsed;
                }
                if (c.Status != (int)RentalContractStatus.Active || status == "Active In Use")
                {
                    handoverBtn.Visibility = Visibility.Collapsed;
                }

                stack.Children.Add(btnPanel);
                border.Child = stack;
                ContractPanel.Children.Add(border);
            }
        }
        private async void AcceptContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {
                    var req = new ConfirmReq()
                    {
                        hasVehicle = data.HasVehicle
                    };
                    await _rentalService.ConfirmContractReq(data.ContractId, req);
                    await LoadAllContracts();
                    MessageBox.Show("Contract accepting successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void RejectContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {
                    var req = new ConfirmReq()
                    {
                        hasVehicle = data.HasVehicle,
                        vehicleStatus = 5
                    };
                    await _rentalService.ConfirmContractReq(data.ContractId, req);
                    await LoadAllContracts();
                    MessageBox.Show("Contract rejected successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void PaymentContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {
                    var paymentWindow = new PaymentWindow(data.ContractId)
                    {
                        Owner = this // optional: gán owner để modal đúng parent
                    };
                    paymentWindow.ShowDialog();
                    await _rentalService.UpdateRentalContractStatus(data.ContractId);
                    await LoadAllContracts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private async void CancelContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {
                    await _rentalService.CancleRentalContract(data.ContractId);
                    await LoadMyContracts();
                    MessageBox.Show("Contract cancel successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancel contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void Staff_Handover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {

                    bool staffSign = data.StaffSignCheckbox?.IsChecked ?? false;
                    if (!staffSign)
                    {
                        MessageBox.Show("Pls sign", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool customerSign = data.CustomerSignCheckbox?.IsChecked ?? false;
                    var req = new HandoverContractReq()
                    {
                        IsSignedByCustomer = customerSign,
                        IsSignedByStaff = staffSign
                    };
                    await _rentalService.HandoverRentalContract(data.ContractId, req);
                    await LoadAllContracts();
                    MessageBox.Show("Staff handover successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancel contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void Customer_Handover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is ContractActionData data)
                {
                    var req = new HandoverContractReq()
                    {
                        IsSignedByCustomer = true,
                        IsSignedByStaff = false
                    };
                    await _rentalService.HandoverRentalContract(data.ContractId, req);
                    await LoadMyContracts();
                    MessageBox.Show("Customer handover successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancel contract: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            contractTitle.Visibility = Visibility.Collapsed;
            ContractPanel.Visibility = Visibility.Collapsed;
            mainBorder.Visibility = Visibility.Visible;
            VehiclePanel.Visibility = Visibility.Visible;
            availableVehicles.Visibility = Visibility.Visible;
            sidebarBooking.Background = null;
            sidebarHome.Background = Brushes.Green;
            BtnReload.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            VehiclePanel.Visibility = Visibility.Collapsed;
            availableVehicles.Visibility = Visibility.Collapsed;
            contractTitle.Visibility = Visibility.Visible;
            ContractPanel.Visibility = Visibility.Visible;
            sidebarBooking.Background = Brushes.Green;
            sidebarHome.Background = null;
            mainBorder.Visibility = Visibility.Collapsed;
            BtnReload.Visibility = Visibility.Visible;
        }

        private async void reload_Click(object sender, RoutedEventArgs e)
        {
            if (User.Role.Name == "Staff")
            {
                await LoadAllContracts();
            }
            else
            {
                await LoadMyContracts();
            }
        }
    }
}
