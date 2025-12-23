using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace QL_NhaTro_Server.Services
{
    public class VNPayService
    {
        public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string returnUrl, string tmnCode, string hashSecret)
        {
            Console.WriteLine("\n=== VNPAY CREATE PAYMENT URL ===");
            Console.WriteLine($"OrderId: {orderId}");
            Console.WriteLine($"Amount: {amount}");
            Console.WriteLine($"TmnCode: {tmnCode}");
            Console.WriteLine($"HashSecret: {hashSecret}");
            
            var vnpay = new VNPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); // VNPay uses smallest currency unit
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);

            var baseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            
            Console.WriteLine($"\nGenerated Payment URL:");
            Console.WriteLine(paymentUrl);
            Console.WriteLine("=== END VNPAY ===");
            
            return paymentUrl;
        }


        public bool ValidateSignature(IQueryCollection queryParams, string hashSecret)
        {
            var vnpay = new VNPayLibrary();
            
            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    var val = value.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        vnpay.AddResponseData(key, val);
                    }
                }
            }

            var vnp_SecureHash = queryParams["vnp_SecureHash"].FirstOrDefault() ?? "";
            var checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

            return checkSignature;
        }
    }

    // VNPay Library Helper
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayComparer());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayComparer());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var dataForUrl = new StringBuilder();
            var dataForHash = new StringBuilder();
            
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // VNPAY requires hash to be computed on the SAME string as URL
                    // So we use URL-encoded value for BOTH
                    var encodedValue = WebUtility.UrlEncode(kv.Value);
                    dataForUrl.Append(kv.Key + "=" + encodedValue + "&");
                    dataForHash.Append(kv.Key + "=" + encodedValue + "&");
                }
            }


            var queryString = dataForUrl.ToString();
            var hashData_string = dataForHash.ToString();
            
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1);
            }
            
            if (hashData_string.Length > 0)
            {
                hashData_string = hashData_string.Remove(hashData_string.Length - 1);
            }

            Console.WriteLine($"\nHash Data String (for signature):");
            Console.WriteLine(hashData_string);
            
            var hashData = HmacSHA512(vnp_HashSecret, hashData_string);
            
            Console.WriteLine($"\nGenerated Signature:");
            Console.WriteLine(hashData);
            
            // VNPAY requires vnp_SecureHashType before vnp_SecureHash
            var paymentUrl = baseUrl + "?" + queryString + "&vnp_SecureHashType=SHA512&vnp_SecureHash=" + hashData;

            return paymentUrl;
        }



        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var data = new StringBuilder();
            
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }

            var hashData = data.ToString();
            if (hashData.Length > 0)
            {
                hashData = hashData.Remove(hashData.Length - 1);
            }

            var checkSum = HmacSHA512(secretKey, hashData);
            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }

    public class VNPayComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
