namespace CarBook.Models
{
    public class ChartViewModel
    {
        public List<string> Labels { get; set; }  // Ví dụ: Tháng, Tên sản phẩm
        public List<double> Data { get; set; }    // Dữ liệu tương ứng (số lượng hoặc doanh thu)
    }
}
