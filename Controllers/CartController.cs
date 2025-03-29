using CarBook.Data;
using CarBook.Models;
using CarBook.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PayPal.Api;
using System.Security.Claims;

namespace CarBook.Controllers
{
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IVnPayService _vnPayservice;
        private readonly PaypalClient _paypalClient;
        private readonly UserManager<ApplicationUser> _userManager;


        public CartController(ApplicationDbContext db, IVnPayService vnPayservice, PaypalClient paypalClient, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _vnPayservice = vnPayservice;
            _paypalClient = paypalClient;
            _userManager = userManager;
        }
        [Route("index")]
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cart = _db.Carts
                      .Include(c => c.Products)
                      .ThenInclude(p => p.Discounts)
                      .Where(x => x.ApplicationUserId == claim);

            List<CartViewModel> cartViewModels = new List<CartViewModel>();
            double totalAllCart = 0.0;

            foreach (var item in cart)
            {
                int days = (item.EndDate - item.StartDate).Days + 1 > 0 ? ((item.EndDate - item.StartDate)).Days : 1;

                double productPrice = (double)item.Products.Price;


                if (item.Products.Discounts != null && item.Products.Discounts.Count > 0)
                {
                    var currentDate = DateTime.Now;
                    var discount = item.Products.Discounts
                                        .Where(d => d.IsActive)
                                        .OrderByDescending(d => d.Percentage) 
                                        .FirstOrDefault();
                  
                    if (discount != null)
                    {
                        var discountPercentage = discount.Percentage;
                        decimal priceAfterDiscount = item.Products.Price * (1 - (decimal)discountPercentage / 100);
                        productPrice = (double)priceAfterDiscount;
                    }
                }

                double totalPrice = days * productPrice;
                totalAllCart += totalPrice;

                cartViewModels.Add(new CartViewModel
                {
                    Cart = item,
                    TotalPrice = totalPrice
                });
            }

            ViewBag.Data = cartViewModels;
            ViewBag.Bill = new Bill { Total = totalAllCart };

            return View("Index");
        }

        [Route("Insert/{id}")]
        [Authorize]
        public async Task<IActionResult> Insert(int id, DateTime startDate, DateTime endDate)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity.FindFirst(ClaimTypes.NameIdentifier);

            var data = await _db.Carts.FirstOrDefaultAsync(c => c.ProductId == id && c.ApplicationUserId == claim.Value);

            if (data == null)
            {
                var cart = new Cart()
                {
                    ProductId = id,
                    StartDate = startDate,
                    EndDate = endDate,
                    ApplicationUserId = claim.Value
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }
            else
            {
                data.StartDate = startDate;
                data.EndDate = endDate;
                _db.Entry(data).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
        [Route("Xoa")]
        [Authorize]
        public IActionResult Xoa(int cartId)
        {
            var cart = _db.Carts.FirstOrDefault(c => c.Id == cartId);
            if (cart != null)
            {
                _db.Carts.Remove(cart);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("ThanhToan")]
        [Authorize]
        public async Task<IActionResult> ThanhToan()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(claim);
            var cartItems = await _db.Carts.Include(c => c.Products)
                                           .Where(c => c.ApplicationUserId == claim)
                                           .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            double total = 0;

            foreach (var item in cartItems)
            {
                int days = (item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1;
                total += days * (double)item.Products.FinalPrice;
            }

            TempData["CustomerName"] = user.Name;
            TempData["CustomerAddress"] = user.Address;
            TempData["CustomerPhoneNumber"] = user.PhoneNumber;
            var viewModel = new ThanhToanViewModel
            {
                CartItems = cartItems.Select(item => new CartViewModel
                {
                    Cart = item,
                    TotalPrice = ((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1)
                                 * (double)item.Products.FinalPrice
                }).ToList(),

                TotalPrice = total,
                PaypalClientId = _paypalClient.ClientId,
                UserName = user.Name,
                Address = user.Address, 
                PhoneNumber = user.PhoneNumber 
            };
            ViewBag.TotalPrice = total;
 
            var bill = new Bill
            {
                ApplicationUserId = claim,
                Total = total,
                OrderDate = DateTime.Now,
                OrderStatus = "Đang xác nhận"
            };
            ViewBag.Bill = bill;
            ViewBag.PaypalClientdId = _paypalClient.ClientId;

            return View(viewModel);
        }

        [HttpPost]
        [Route("ThanhToanSubmit")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ThanhToanSubmit(CancellationToken cancellationToken, string UserName, string Address, string PhoneNumber, CartViewModel cartViewModel, string payment )
        {

            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(claim);
            var cartItems = _db.Carts.Include(c => c.Products)
                                          .Where(c => c.ApplicationUserId == claim)
                                          .ToList();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }


            double total = cartItems.Sum(item => ((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1) * (double)item.Products.FinalPrice);
            TempData["CustomerName"] = UserName;
            TempData["CustomerAddress"] =Address;
            TempData["CustomerPhoneNumber"] = PhoneNumber;
            if (payment == "Thanh toán VNPay")
            {
                try
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = total,
                        CreatedDate = DateTime.Now,
                        Description = $"{UserName} - {PhoneNumber}",
                        FullName = UserName,
                        OrderId = new Random().Next(1000, 100000)
                    };

                    var paymentUrl = _vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel);
                    return Redirect(paymentUrl);
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Lỗi trong quá trình tạo đơn hàng VNPay: " + ex.Message;
                    return RedirectToAction("PaymentFail");
                }
            }
            var bill = new Bill()
            {
                ApplicationUserId = claim,
                Total = total,
                OrderDate = DateTime.Now,
                OrderStatus = "Đang xác nhận",
                StartDate = cartItems.Min(item => item.StartDate), 
                EndDate = cartItems.Max(item => item.EndDate),
                Name = UserName,  
                Address = Address, 
                PhoneNumber = PhoneNumber
            };

            _db.Bills.Add(bill);
            await _db.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product != null)
                {
                    // Kiểm tra nếu sản phẩm đang "Available" thì cập nhật thành "Booked"
                    if (product.Status == "Available")
                    {
                        product.Status = "Booked"; // Cập nhật trạng thái xe
                        _db.Products.Update(product);
                    }
                    else
                    {
                        // Nếu sản phẩm đã được đặt, thông báo lỗi
                        TempData["Message"] = $"Xe {product.Name} đã được đặt. Vui lòng chọn xe khác.";
                        return RedirectToAction("Index", "Cart");
                    }
                }
                var detailBill = new DetailBill()
                {
                    ProductId = item.ProductId,
                    BillId = bill.Id,
                    ProductPrice = (double)(((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1) * item.Products.FinalPrice),
                    Days = ((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1)
                };
                _db.DetailBills.Add(detailBill);
            }
            await _db.SaveChangesAsync();
            _db.Carts.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            return RedirectToAction("PaymentSuccess");
        }

        [Authorize]
        [Route("PaymentCallBack")]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode ?? "Không rõ"}";
                return RedirectToAction("PaymentFail");
            }

            try
            {
                var orderId = Request.Query["vnp_TxnRef"].ToString();
                var transactionId = Request.Query["vnp_TransactionNo"].ToString();
                var amount = decimal.Parse(Request.Query["vnp_Amount"].ToString()) / 100;
                var paymentDate = DateTime.ParseExact(Request.Query["vnp_PayDate"], "yyyyMMddHHmmss", null);
                var name = TempData["CustomerName"]?.ToString();
                var address = TempData["CustomerAddress"]?.ToString();
                var phoneNumber = TempData["CustomerPhoneNumber"]?.ToString();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phoneNumber))
                {
                    TempData["Message"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("PaymentFail");
                }

                var claim = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier);
                var userId = claim?.Value;
                var cartItems = _db.Carts.Include(c => c.Products)
                                         .Where(c => c.ApplicationUserId == userId)
                                         .ToList();

                if (!cartItems.Any())
                {
                    TempData["Message"] = "Lỗi: Không tìm thấy sản phẩm trong giỏ hàng.";
                    return RedirectToAction("PaymentFail");
                }
                var bill = new Bill
                {
                    ApplicationUserId = userId,
                    Total = (double)amount,
                    OrderDate = paymentDate,
                    OrderStatus = "Đã thanh toán VnPay",
                    StartDate = cartItems.Min(item => item.StartDate), 
                    EndDate = cartItems.Max(item => item.EndDate),
                    Name = name,
                    Address = address,
                    PhoneNumber = phoneNumber
                };

                _db.Bills.Add(bill);
                _db.SaveChanges();
        

                foreach (var item in cartItems)
                {
                    var product = _db.Products.FirstOrDefault(p => p.Id == item.ProductId); // Sử dụng FirstOrDefault thay cho FirstOrDefaultAsync
                    if (product != null)
                    {
                        // Kiểm tra nếu sản phẩm đang "Available" thì cập nhật thành "Booked"
                        if (product.Status.ToString() == "Available")
                        {
                            product.Status ="Booked"; // Cập nhật trực tiếp trạng thái xe
                            _db.Products.Update(product);
                        }
                        else
                        {
                            // Nếu sản phẩm đã được đặt, thông báo lỗi
                            TempData["Message"] = $"Xe {product.Name} đã được đặt. Vui lòng chọn xe khác.";
                            return RedirectToAction("Index", "Cart");
                        }
                    }
                        var detailBill = new DetailBill
                    {
                        ProductId = item.ProductId,
                        BillId = bill.Id,
                        ProductPrice = (double)(((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1) * item.Products.FinalPrice),
                        Days = ((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1)
                    };

                    _db.DetailBills.Add(detailBill);
                }
                _db.SaveChanges();
                _db.Carts.RemoveRange(cartItems);
                _db.SaveChanges();

                TempData["Message"] = "Thanh toán VNPay thành công! Đơn hàng của bạn đã được lưu.";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi khi lưu đơn hàng: " + ex.Message;
                return RedirectToAction("PaymentFail");
            }
        }
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity.FindFirst(ClaimTypes.NameIdentifier);
            var cartItems = _db.Carts.Include(c => c.Products)
                                     .Where(c => c.ApplicationUserId == claim.Value)
                                     .ToList();

            double totalVND = 0;
            foreach (var item in cartItems)
            {
                totalVND += ((item.EndDate - item.StartDate).Days > 0 ? (item.EndDate - item.StartDate).Days : 1) * (double)item.Products.FinalPrice;
            }

            double exchangeRate = await GetExchangeRateAsync("USD", "VND");
            double totalUSD = totalVND / exchangeRate;
            string tongTien = totalUSD.ToString("F2");
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, string UserName, string Address, string PhoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                var identity = (ClaimsIdentity)User.Identity;
                var claim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(claim);
                var cartItems = await _db.Carts.Include(c => c.Products)
                                               .Where(c => c.ApplicationUserId == claim)
                                               .ToListAsync();

                if (!cartItems.Any())
                {
                    return BadRequest(new { Message = "Giỏ hàng của bạn đang trống." });
                }

                double total = cartItems.Sum(item => ((item.EndDate - item.StartDate).Days + 1 > 0 ? (item.EndDate - item.StartDate).Days + 1 : 1) * (double)item.Products.FinalPrice);

                //var name = TempData["CustomerName"]?.ToString();
                //var address = TempData["CustomerAddress"]?.ToString();
                //var phoneNumber = TempData["CustomerPhoneNumber"]?.ToString();
                var name = user.Name;
                var address =user.Address;
                var phoneNumber = user.PhoneNumber;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phoneNumber))
                {
                    TempData["Message"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("PaymentFail");
                }
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var bill = new Bill
                        {
                            ApplicationUserId = claim,
                            Total = total,
                            OrderDate = DateTime.Now,
                            OrderStatus = "Đã thanh toán qua PayPal",
                            StartDate = cartItems.Min(item => item.StartDate), 
                            EndDate = cartItems.Max(item => item.EndDate),
                            Name = name,
                            Address = address,
                            PhoneNumber = phoneNumber
                        };

                        _db.Bills.Add(bill);
                        await _db.SaveChangesAsync(cancellationToken);
                        foreach (var item in cartItems)
                        {
                            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            if (product != null)
                            {
                                // Kiểm tra nếu sản phẩm đang "Available" thì cập nhật thành "Booked"
                                if (product.Status == "Available")
                                {
                                    product.Status = "Booked"; // Cập nhật trạng thái xe
                                    _db.Products.Update(product);
                                }
                                else
                                {
                                    // Nếu sản phẩm đã được đặt, thông báo lỗi
                                    TempData["Message"] = $"Xe {product.Name} đã được đặt. Vui lòng chọn xe khác.";
                                    return RedirectToAction("Index", "Cart");
                                }
                            }
                            var detailBill = new DetailBill
                            {
                                ProductId = item.ProductId,
                                BillId = bill.Id,
                                ProductPrice = (double)(((item.EndDate - item.StartDate).Days + 1 > 0 ? (item.EndDate - item.StartDate).Days : 1) * item.Products.FinalPrice),
                                Days = ((item.EndDate - item.StartDate).Days + 1 > 0 ? (item.EndDate - item.StartDate).Days + 1 : 1)
                            };
                            _db.DetailBills.Add(detailBill);
                        }
                        await _db.SaveChangesAsync(cancellationToken);
                        _db.Carts.RemoveRange(cartItems);
                        await _db.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync();

                        return Ok(new { Message = "Thanh toán PayPal thành công. Đơn hàng đã được lưu." });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        var error = new { Message = "Lỗi khi xử lý thanh toán PayPal: " + ex.GetBaseException().Message };
                        return BadRequest(error);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = new { Message = "Lỗi khi xử lý thanh toán PayPal: " + ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        [Route("UpdateDates")]
        public IActionResult UpdateDates(int cartId, DateTime startDate, DateTime endDate)
        {
            var cart = _db.Carts.FirstOrDefault(c => c.Id == cartId);
            if (cart != null)
            {
                if (startDate > endDate)
                {
                    return BadRequest("Ngày trả phải sau ngày thuê.");
                }

                cart.StartDate = startDate;
                cart.EndDate = endDate;
                _db.SaveChanges();
                return Ok();
            }

            return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");
        }

        public async Task<double> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            string apiKey = "56967db4642a9334602cfa2dd1395c5c";
            string url = $"https://api.currencylayer.com/live?access_key={apiKey}&source={fromCurrency}&currencies={toCurrency}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(url);
                var jsonResponse = JObject.Parse(response);
                return jsonResponse["quotes"][$"{fromCurrency}{toCurrency}"].Value<double>();
            }
        }


        [Authorize]

        [Route("Success")]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }
        [Authorize]

        [Route("Fail")]
        public IActionResult PaymentFail()
        {
            return View();
        }



    }

}

