namespace CarBook.Models
{
    public class ThanhToanViewModel
    {
        public List<CartViewModel> CartItems { get; set; }
        public double TotalPrice { get; set; }
        public string PaypalClientId { get; set; }

        public string UserName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

}
