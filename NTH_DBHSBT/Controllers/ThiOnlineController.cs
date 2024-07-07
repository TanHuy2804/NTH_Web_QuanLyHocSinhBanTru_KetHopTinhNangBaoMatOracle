using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class ThiOnlineController : Controller
    {
        //string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        private List<MonHoc> LayDanhSachMonHoc()
        {
            List<MonHoc> danhSachMonHoc = new List<MonHoc>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.MonHoc_SelectAll", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MonHoc monHoc = new MonHoc
                                {
                                    MaMH = reader["MaMH"].ToString(),
                                    TenMH = reader["TenMH"].ToString()
                                };
                                danhSachMonHoc.Add(monHoc);
                            }

                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                    // Ví dụ: Ghi log, hiển thị thông báo lỗi, v.v.
                    Console.WriteLine(ex.Message);
                }
            }

            return danhSachMonHoc;
        }

        public JsonResult LayDanhSachDeThiTheoMon(string maMonHoc)
        {
            List<DeThi> danhSachDeThi = new List<DeThi>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.DeThi_Where_MaMonHoc", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        // Input parameter
                        //OracleParameter maMonHocParam = new OracleParameter();
                        //maMonHocParam.ParameterName = "maMonHoc";
                        //maMonHocParam.OracleDbType = OracleDbType.NVarchar2;
                        //maMonHocParam.Direction = ParameterDirection.Input;
                        //maMonHocParam.Value = maMonHoc;
                        //command.Parameters.Add(maMonHocParam);

                        command.Parameters.Add("maMonHoc", OracleDbType.NVarchar2, maMonHoc, ParameterDirection.Input);
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeThi deThi = new DeThi
                                {
                                    MaDeThi = reader["MaDeThi"].ToString(),
                                    // Add other fields if necessary
                                };
                                danhSachDeThi.Add(deThi);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                    // Ví dụ: Ghi log, hiển thị thông báo lỗi, v.v.
                    Console.WriteLine(ex.Message);
                }
            }

            return Json(danhSachDeThi, JsonRequestBehavior.AllowGet);
        }
        private List<DeThi> LayDanhSachDeThi()
        {
            List<DeThi> danhSachDeThi = new List<DeThi>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.DeThi_SelectAll", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DeThi deThi = new DeThi
                                {
                                    MaDeThi = reader["MaDeThi"].ToString(),
                                    // Lấy các trường dữ liệu khác tương ứng
                                    // Ví dụ: TenDeThi = reader["TenDeThi"].ToString(),
                                    // NgayThi = Convert.ToDateTime(reader["NgayThi"])
                                };
                                danhSachDeThi.Add(deThi);
                            }

                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                    // Ví dụ: Ghi log, hiển thị thông báo lỗi, v.v.
                    Console.WriteLine(ex.Message);
                }
            }

            return danhSachDeThi;
        }
        private List<CauHoiModel> TronCauHoi(List<CauHoiModel> danhSachCauHoi)
        {
            // Sử dụng phương thức trộn ngẫu nhiên của LINQ để trộn danh sách câu hỏi.
            Random rng = new Random();
            return danhSachCauHoi.OrderBy(x => rng.Next()).ToList();
        }
        public ActionResult Thi(string maMonHoc, string maDeThi, string maHocSinh)
        {
            // Trả về view Thi và truyền các giá trị đã chọn
            ViewBag.MaMonHoc = maMonHoc;
            ViewBag.MaDeThi = maDeThi;
            ViewBag.MaHocSinh = maHocSinh;
            // Lấy Thời gian thi từ cơ sở dữ liệu bằng cách gọi hàm đã tạo
            string thoiGianThi = LayThoiGianThiTuCSDL(maDeThi);
            // Truyền thời gian thi vào view
            ViewBag.ThoiGianThi = thoiGianThi;

            List<CauHoiModel> danhSachCauHoi = new List<CauHoiModel>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    // Sử dụng thủ tục đã tạo để lấy danh sách câu hỏi
                    using (OracleCommand command = new OracleCommand("NV001.CauHoi_Where_MaDeThi", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("maDeThi", OracleDbType.NVarchar2, maDeThi, ParameterDirection.Input);
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CauHoiModel cauHoi = new CauHoiModel
                                {
                                    MaCauHoi = reader["MaCauHoi"].ToString(),
                                    CauHoi = reader["CauHoi"].ToString(),
                                    DapAn_A = reader["DapAn_A"].ToString(),
                                    DapAn_B = reader["DapAn_B"].ToString(),
                                    DapAn_C = reader["DapAn_C"].ToString(),
                                    DapAn_D = reader["DapAn_D"].ToString(),
                                    DapAnDung = reader["DapAnDung"].ToString(),
                                    MaMH = reader["MaMH"].ToString()
                                };
                                danhSachCauHoi.Add(cauHoi);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                }
            }
            // Trộn danh sách câu hỏi
            danhSachCauHoi = TronCauHoi(danhSachCauHoi);

            // Check if danhSachCauHoi is empty
            if (!danhSachCauHoi.Any())
            {
                ViewBag.ErrorMessage = "Đề thi hiện tại chưa được mở.";
                return View("Thi");
            }

            // Truyền danh sách câu hỏi đã được trộn vào view
            return View("Thi", danhSachCauHoi);
        }
        private string LayThoiGianThiTuCSDL(string maDeThi)
        {
            string thoiGianThi = "";
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    // Tạo một đối tượng OracleCommand để gọi stored procedure
                    using (OracleCommand command = new OracleCommand("NV001.ThoiGianThi_Where_MaDeThi", connection))
                    {
                        // Xác định kiểu của command là StoredProcedure
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm tham số cho stored procedure
                        command.Parameters.Add("maDeThi", OracleDbType.NVarchar2).Value = maDeThi;

                        // Thêm tham số đầu ra cho stored procedure
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        // Thực thi stored procedure
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            // Đọc kết quả trả về từ stored procedure
                            if (reader.Read())
                            {
                                thoiGianThi = reader["ThoiGianThi"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                }
            }
            return thoiGianThi;
        }



        [HttpPost]
        public ActionResult Submit(string MaHocSinh, string MaDeThi, List<DapAn> answers)
        {
            // Debug: kiểm tra giá trị của answers
            foreach (var answer in answers)
            {
                System.Diagnostics.Debug.WriteLine($"MaCauHoi: {answer.MaCauHoi}, DapAnDung: {answer.DapAnDung}");
            }

            if (answers == null || !answers.Any())
            {
                ViewBag.SoCauDung = 0;
                ViewBag.Diem = 0;
                return View("Result");
            }

            int soCauDung = TinhSoCauDung(answers);
            double diem = TinhDiem(soCauDung, answers.Count);

            // Lưu kết quả bài làm vào cơ sở dữ liệu
            LuuKetQuaBaiLam(MaHocSinh, MaDeThi, soCauDung.ToString(), diem.ToString());

            ViewBag.SoCauDung = soCauDung;
            ViewBag.Diem = diem;

            return View("Result");
        }
        private void LuuKetQuaBaiLam(string maHocSinh, string maDeThi, string soCauDung, string diemSo)
        {
            string query = "BEGIN NV001.ThemKetQua(:maHocSinh, :maDeThi, :soCauDung, :diemSo); END;";

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":maHocSinh", maHocSinh);
                    command.Parameters.Add(":maDeThi", maDeThi);
                    command.Parameters.Add(":soCauDung", soCauDung);
                    command.Parameters.Add(":diemSo", diemSo);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Xử lý exception
                    }
                }
            }
        }

        private int TinhSoCauDung(List<DapAn> answers)
        {
            int soCauDung = 0;

            foreach (var answer in answers)
            {
                string dapAnDung = GetDapAnDung(answer.MaCauHoi);

                if (!string.IsNullOrEmpty(dapAnDung) && !string.IsNullOrEmpty(answer.DapAnDung))
                {
                    if (string.Equals(dapAnDung, answer.DapAnDung, StringComparison.OrdinalIgnoreCase))
                    {
                        soCauDung++;
                    }
                }
            }

            return soCauDung;
        }

        private double TinhDiem(int soCauDung, int tongSoCau)
        {
            return (double)soCauDung / tongSoCau * 10;
        }

        private string GetDapAnDung(string maCauHoi)
        {
            string dapAnDung = null;

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.CauHoi_Select_DapAnDung", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("maCauHoi", OracleDbType.NVarchar2).Value = maCauHoi;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                dapAnDung = reader["DapAnDung"].ToString();
                            }

                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                    // Ví dụ: Ghi log, hiển thị thông báo lỗi, v.v.
                    Console.WriteLine(ex.Message);
                }
            }

            return dapAnDung;
        }


        public ActionResult ThongTinBaiThi()
        {
            List<MonHoc> danhSachMonHoc = LayDanhSachMonHoc();
            ViewBag.DanhSachMonHoc = danhSachMonHoc;

            List<DeThi> danhSachDeThi = LayDanhSachDeThi();
            ViewBag.DanhSachDeThi = danhSachDeThi;

            return View();
        }

        public ActionResult Result()
        {
            return View();
        }
    }
}