using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recharge_Test.Models;
using Recharge_Test.PaymentLibrary.MoMo;
using Recharge_Test.Repository;
using Recharge_Test.Repository.Models;
using Recharge_Test.Repository.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Recharge_Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly EVDbContext _context;
        private readonly ILogger<HomeController> _logger;
        public INotyfService _notifyService { get; }

        public HomeController(ILogger<HomeController> logger, EVDbContext context, INotyfService notifyService)
        {
            _context = context;
            _logger = logger;
            _notifyService = notifyService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deposit(DepositViewModel depositVM, string totalExchange)
        {
            if (depositVM.Amount != null && depositVM.ForeignCurrency != null && depositVM.PaymentContent != null && depositVM.Payment != null)
            {
                string userId = HttpContext.Request.Cookies["UserId"];
                var addInvoice = new Invoice
                {
                    CustomerId = int.Parse(userId),
                    ForeignCurrency = depositVM.ForeignCurrency,
                    TotalAmount = double.Parse(depositVM.Amount),
                    TotalExchange = totalExchange,
                    Note = depositVM.PaymentContent,
                    CreatedDate = DateTime.Now,
                };
                // Lưu data để thanh toán cho momo
                TempData["DataInvoice"] = JsonConvert.SerializeObject(addInvoice);
                return RedirectToAction("PaymentMomo", "Home");
            }
            _notifyService.Warning("Chưa nhập đầy đủ thông tin.", 3);
            return View(depositVM);
        }

        public IActionResult PaymentMomo()
        {
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMOFJO620220613";
            string accessKey = "3yQUosJ09F6yaAXm";
            string serectkey = "rq6caxakKTyJJGdamOCFJWjdiodI4zVU";
            string orderInfo = "Giao dịch thanh toán hoá đơn EV";
            string redirectUrl = "https://localhost:44314/Home/ConfirmPaymentClient";
            string ipnUrl = "https://9322-116-102-185-51.ap.ngrok.io/Carts/OrderSuccess";
            string requestType = "captureWallet";

            //thêm hóa đơn
            var dataJson = TempData["DataInvoice"] as string;
            var dataHoaDon = JsonConvert.DeserializeObject<Invoice>(dataJson);
            string amount = dataHoaDon.TotalAmount.ToString();
            DateTime now = DateTime.Now;
            string orderId = "EV" + now.ToString("yyMMddhhmmss").ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTest" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "vi" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        public IActionResult ConfirmPaymentClient()
        {
            string username = HttpContext.Request.Cookies["Username"];
            //string reorderId = TempData["OrderId"] as string;
            //string rerequestId = TempData["RequestId"] as string;

            string rsCode = (Request.Query["resultCode"]);
            //string errorCode = (Request.Query["errorCode"]);
            //string orderId = (Request.Query["orderId"]);

            //string endpoint = "https://test-payment.momo.vn/v2/gateway/api/query";
            //string accessKey = "3yQUosJ09F6yaAXm";
            //string serectkey = "rq6caxakKTyJJGdamOCFJWjdiodI4zVU";
            //string partnerCode = "MOMOFJO620220613";
            //string requestId = rerequestId;
            //string orderId = reorderId;

            ////Before sign HMAC SHA256 signature
            //string rawHash = "accessKey=" + accessKey +
            //    "&partnerCode=" + partnerCode +
            //    "&requestId=" + requestId +
            //    "&orderId=" + orderId
            //    ;

            //MoMoSecurity crypto = new MoMoSecurity();
            ////sign signature SHA256
            //string signature = crypto.signSHA256(rawHash, serectkey);

            ////build body json request
            //JObject jsonRequest = new JObject
            //{
            //    { "partnerCode", partnerCode },
            //    { "requestId", requestId },
            //    { "orderId", orderId },
            //    { "signature", signature },
            //    { "lang", "vi" },
            //};

            //response from MoMo
            //string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, jsonRequest.ToString());
            //JObject jmessage = JObject.Parse(responseFromMomo);

            if (rsCode == "0")
            {
                if (TempData.ContainsKey("DataInvoice"))
                {
                    var dataJson = TempData["DataInvoice"] as string;
                    var dataHoaDon = JsonConvert.DeserializeObject<Invoice>(dataJson);
                    Invoice inv = new Invoice();
                    inv.CustomerId = dataHoaDon.CustomerId;
                    inv.ForeignCurrency = dataHoaDon.ForeignCurrency;
                    inv.TotalAmount = dataHoaDon.TotalAmount;
                    inv.TotalExchange = dataHoaDon.TotalExchange;
                    inv.StatusId = 0; // Success
                    inv.Note = dataHoaDon.Note;
                    inv.CreatedDate = dataHoaDon.CreatedDate;
                    _context.Add(inv);
                    _context.SaveChanges();

                    // Thêm CT hoá đơn
                    InvoiceDetail invoiceDetail = new InvoiceDetail();
                    invoiceDetail.DetailId = Guid.NewGuid();
                    invoiceDetail.InvoicesId = inv.InvoicesId;
                    invoiceDetail.OrderId = (Request.Query["orderId"]);
                    invoiceDetail.Payment = "MoMo";
                    invoiceDetail.CustomerName = username;
                    invoiceDetail.Description = dataHoaDon.Note;
                    invoiceDetail.StatusDescription = "Success";
                    invoiceDetail.CreatedDate = DateTime.Now;
                    _context.Add(invoiceDetail);
                    _context.SaveChanges();
                    _notifyService.Success($"Thanh toán hoá đơn {(Request.Query["orderId"])} qua MoMo thành công.", 5);
                    return RedirectToAction("StatisticalInvoices", "Home");

                    //Thêm số dư

                }
            }

            if (rsCode == "1004" || rsCode == "99" || rsCode == "1003" || rsCode == "1005" || rsCode == "1006" || rsCode == "42")
            {
                //Hoa don huy tra ve index
                if (TempData.ContainsKey("DataInvoice"))
                {
                    var dataJson = TempData["DataInvoice"] as string;
                    var dataHoaDon = JsonConvert.DeserializeObject<Invoice>(dataJson);
                    Invoice inv = new Invoice();
                    inv.CustomerId = dataHoaDon.CustomerId;
                    inv.ForeignCurrency = dataHoaDon.ForeignCurrency;
                    inv.TotalAmount = dataHoaDon.TotalAmount;
                    inv.TotalExchange = dataHoaDon.TotalExchange;
                    inv.StatusId = 1; // Failed
                    inv.Note = dataHoaDon.Note;
                    inv.CreatedDate = dataHoaDon.CreatedDate;
                    _context.Add(inv);
                    _context.SaveChanges();

                    // Thêm CT hoá đơn
                    InvoiceDetail invoiceDetail = new InvoiceDetail();
                    invoiceDetail.DetailId = Guid.NewGuid();
                    invoiceDetail.InvoicesId = inv.InvoicesId;
                    invoiceDetail.Payment = "MoMo";
                    invoiceDetail.CustomerName = username;
                    invoiceDetail.Description = dataHoaDon.Note;
                    invoiceDetail.StatusDescription = "Failed";
                    invoiceDetail.CreatedDate = DateTime.Now;
                    _context.Add(invoiceDetail);
                    _context.SaveChanges();
                }
                _notifyService.Warning("Thanh toán Momo không thành công. Thử lại!", 5);
                return RedirectToAction("Deposit", "Home");
            }
            return RedirectToAction("Deposit", "Home");
        }

        [Authorize]
        public async Task<IActionResult> StatisticalInvoices()
        {
            string userId = HttpContext.Request.Cookies["UserId"];
            var data = _context.Invoices.Include(i => i.Customer).Where(x => x.CustomerId == int.Parse(userId));
            return View(await data.ToListAsync());
        }

        [Authorize]
        public IActionResult InvoiceDetail(int? id)
        {
            var data = _context.InvoiceDetails.Include(a => a.Invoices).FirstOrDefault(a => a.InvoicesId == id);
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
