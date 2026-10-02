using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoSpeedLogistics
{

    public abstract class PhuongTien
    {
        private string _maPT;
        private string _tenHang;
        private int _namSanXuat;
        private decimal _giaGoc;

        public string MaPT
        {
            get => _maPT;
            set => _maPT = string.IsNullOrWhiteSpace(value) ? "PT000" : value;
        }

        public string TenHang
        {
            get => _tenHang;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên hãng không được để trống.");
                _tenHang = value;
            }
        }

        public int NamSanXuat
        {
            get => _namSanXuat;
            set
            {
                int currentYear = DateTime.Now.Year;
                if (value < 1900 || value > currentYear)
                    throw new ArgumentException($"Năm sản xuất phải từ 1900 đến {currentYear}.");
                _namSanXuat = value;
            }
        }

        public decimal GiaGoc
        {
            get => _giaGoc;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Giá gốc phải lớn hơn 0.");
                _giaGoc = value;
            }
        }

        protected PhuongTien(string maPT, string tenHang, int namSanXuat, decimal giaGoc)
        {
            MaPT = maPT;
            TenHang = tenHang;
            NamSanXuat = namSanXuat;
            GiaGoc = giaGoc;
        }

        public abstract decimal TinhGiaLanBanh();

        public virtual string GetInfo()
        {
            return $"Mã PT: {MaPT,-8} | Hãng: {TenHang,-12} | Năm SX: {NamSanXuat} | Giá gốc: {GiaGoc:N0} VNĐ";
        }
    }
    public class OTo : PhuongTien
    {
        private int _soChoNgoi;
        private double _dungTichDongCo;

        public int SoChoNgoi
        {
            get => _soChoNgoi;
            set
            {
                if (value <= 0) throw new ArgumentException("Số chỗ ngồi phải lớn hơn 0.");
                _soChoNgoi = value;
            }
        }

        public double DungTichDongCo
        {
            get => _dungTichDongCo;
            set
            {
                if (value <= 0) throw new ArgumentException("Dung tích động cơ phải lớn hơn 0.");
                _dungTichDongCo = value;
            }
        }

        public OTo(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int soChoNgoi, double dungTichDongCo)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            SoChoNgoi = soChoNgoi;
            DungTichDongCo = dungTichDongCo;
        }

        public override decimal TinhGiaLanBanh()
        {
            decimal thueTruocBa = SoChoNgoi <= 9 ? GiaGoc * 0.12m : GiaGoc * 0.10m;
            decimal thueTieuThuDacBiet = SoChoNgoi <= 9 ? GiaGoc * 0.30m : 0m;
            return GiaGoc + thueTruocBa + thueTieuThuDacBiet;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $" | Loại: Ô tô   | Chỗ: {SoChoNgoi,-2} | Động cơ: {DungTichDongCo}L";
        }
    }
    public class XeMay : PhuongTien
    {
        private int _dungTichXylanh;

        public int DungTichXylanh
        {
            get => _dungTichXylanh;
            set
            {
                if (value <= 0) throw new ArgumentException("Dung tích xy-lanh phải lớn hơn 0.");
                _dungTichXylanh = value;
            }
        }

        public XeMay(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int dungTichXylanh)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            DungTichXylanh = dungTichXylanh;
        }

        public override decimal TinhGiaLanBanh()
        {
            decimal thueTruocBa = DungTichXylanh < 175 ? GiaGoc * 0.02m : GiaGoc * 0.05m;
            return GiaGoc + thueTruocBa;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $" | Loại: Xe máy  | Phân khối: {DungTichXylanh}cc";
        }
    }
    public class QuanLyPhuongTien
    {
        private List<PhuongTien> _danhSachPT = new List<PhuongTien>();

        public void AddPhuongTien(PhuongTien pt)
        {
            if (pt != null)
            {
                _danhSachPT.Add(pt);
                Console.WriteLine("\n>> Thêm phương tiện thành công!");
            }
        }

        public void DisplayAll()
        {
            Console.WriteLine("\n=================== DANH SÁCH PHƯƠNG TIỆN ===================");
            if (!_danhSachPT.Any())
            {
                Console.WriteLine("Danh sách hiện đang trống.");
                return;
            }

            foreach (var pt in _danhSachPT)
            {
                Console.WriteLine($"{pt.GetInfo()} => Giá lăn bánh: {pt.TinhGiaLanBanh():N0} VNĐ");
            }
        }

        public PhuongTien FindMaxGiaLanBanh()
        {
            if (!_danhSachPT.Any()) return null;
            return _danhSachPT.OrderByDescending(p => p.TinhGiaLanBanh()).FirstOrDefault();
        }

        public List<PhuongTien> SearchByName(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<PhuongTien>();
            return _danhSachPT
                .Where(p => p.TenHang.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            QuanLyPhuongTien ql = new QuanLyPhuongTien();
            bool running = true;

            while (running)
            {
                Console.WriteLine("   HỆ THỐNG QUẢN LÝ PHƯƠNG TIỆN - AUTOSPEED");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Thêm mới Ô tô");
                Console.WriteLine("2. Thêm mới Xe máy");
                Console.WriteLine("3. Hiển thị danh sách toàn bộ phương tiện");
                Console.WriteLine("4. Tìm phương tiện có Giá lăn bánh cao nhất");
                Console.WriteLine("5. Tìm kiếm phương tiện theo Tên hãng");
                Console.WriteLine("0. Thoát chương trình");
                Console.WriteLine("==================================================");
                Console.Write("Chọm chức năng (0-5): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        NhapOTo(ql);
                        break;
                    case "2":
                        NhapXeMay(ql);
                        break;
                    case "3":
                        ql.DisplayAll();
                        break;
                    case "4":
                        var maxPt = ql.FindMaxGiaLanBanh();
                        if (maxPt != null)
                        {
                            Console.WriteLine("\n--- PHƯƠNG TIỆN CÓ GIÁ LĂN BÁNH CAO NHẤT ---");
                            Console.WriteLine($"{maxPt.GetInfo()} => Giá lăn bánh: {maxPt.TinhGiaLanBanh():N0} VNĐ");
                        }
                        else
                        {
                            Console.WriteLine("\nDanh sách trống!");
                        }
                        break;
                    case "5":
                        Console.Write("\nNhập tên hãng cần tìm: ");
                        string keyword = Console.ReadLine();
                        var ketQua = ql.SearchByName(keyword);
                        Console.WriteLine($"\n--- KẾT QUẢ TÌM KIẾM THEO TỪ KHÓA '{keyword}' ---");
                        if (ketQua.Any())
                        {
                            foreach (var pt in ketQua)
                            {
                                Console.WriteLine($"{pt.GetInfo()} => Giá lăn bánh: {pt.TinhGiaLanBanh():N0} VNĐ");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy phương tiện phù hợp.");
                        }
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("\nĐã thoát chương trình.");
                        break;
                    default:
                        Console.WriteLine("\nLựa chọn không hợp lệ, vui lòng chọn lại!");
                        break;
                }
            }
        }
        private static void NhapOTo(QuanLyPhuongTien ql)
        {
            try
            {
                Console.WriteLine("\n--- NHẬP THÔNG TIN Ô TÔ ---");
                Console.Write("Mã phương tiện: ");
                string maPT = Console.ReadLine();

                Console.Write("Tên hãng: ");
                string tenHang = Console.ReadLine();

                Console.Write("Năm sản xuất: ");
                int namSX = int.Parse(Console.ReadLine());

                Console.Write("Giá gốc (VNĐ): ");
                decimal giaGoc = decimal.Parse(Console.ReadLine());

                Console.Write("Số chỗ ngồi: ");
                int soCho = int.Parse(Console.ReadLine());

                Console.Write("Dung tích động cơ (Lít, ví dụ: 2.0): ");
                double dungTich = double.Parse(Console.ReadLine());

                OTo oto = new OTo(maPT, tenHang, namSX, giaGoc, soCho, dungTich);
                ql.AddPhuongTien(oto);
            }
            catch (FormatException)
            {
                Console.WriteLine("\n[LỖI] Dữ liệu số nhập vào không đúng định dạng!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n[LỖI DỮ LIỆU] {ex.Message}");
            }
        }
        private static void NhapXeMay(QuanLyPhuongTien ql)
        {
            try
            {
                Console.WriteLine("\n--- NHẬP THÔNG TIN XE MÁY ---");
                Console.Write("Mã phương tiện: ");
                string maPT = Console.ReadLine();

                Console.Write("Tên hãng: ");
                string tenHang = Console.ReadLine();

                Console.Write("Năm sản xuất: ");
                int namSX = int.Parse(Console.ReadLine());

                Console.Write("Giá gốc (VNĐ): ");
                decimal giaGoc = decimal.Parse(Console.ReadLine());

                Console.Write("Dung tích Xy-lanh (cc): ");
                int dungTichXylanh = int.Parse(Console.ReadLine());

                XeMay xeMay = new XeMay(maPT, tenHang, namSX, giaGoc, dungTichXylanh);
                ql.AddPhuongTien(xeMay);
            }
            catch (FormatException)
            {
                Console.WriteLine("\n[LỖI] Dữ liệu số nhập vào không đúng định dạng!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n[LỖI DỮ LIỆU] {ex.Message}");
            }
        }
    }
}