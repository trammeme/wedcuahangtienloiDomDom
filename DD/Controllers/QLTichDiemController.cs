using DD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QLTichDiemController : Controller
    {
        private readonly string _connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=Dom1;Integrated Security=True;";

        private string filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data"), "users.txt");

        // Tải danh sách người dùng từ tệp
        public ActionResult Index()
        {
            var users = LoadUsersFromFile(); // Đọc từ tệp
            return View(users);
        }

        // Tạo người dùng mới
        public ActionResult Create()
        {
            return View(new TichDiem());
        }

        [HttpPost]
        public ActionResult Create(TichDiem model)
        {
            if (ModelState.IsValid)
            {
                var users = LoadUsersFromFile();
                model.ID = users.Count + 1; // Gán ID tự động
                users.Add(model); // Thêm người dùng vào danh sách
                SaveUsersToFile(users); // Lưu vào tệp
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // Phương thức lưu người dùng vào tệp
        private void SaveUsersToFile(List<TichDiem> users)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var user in users)
                    {
                        writer.WriteLine($"{user.ID},{user.maKH},{user.tenKH},{user.diem}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi
                throw new Exception("Có lỗi khi ghi tệp: " + ex.Message);
            }
        }

        // Phương thức tải người dùng từ tệp
        private List<TichDiem> LoadUsersFromFile()
        {
            try
            {
                // Tạo file nếu nó chưa tồn tại
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.Create(filePath).Close(); // Tạo file rỗng
                    return new List<TichDiem>(); // Trả về danh sách rỗng
                }

                var users = new List<TichDiem>();
                var lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 4)
                    {
                        users.Add(new TichDiem
                        {
                            ID = int.Parse(parts[0]),
                            maKH = parts[1],
                            tenKH = parts[2],
                            diem = int.Parse(parts[3])
                        });
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi
                throw new Exception("Có lỗi khi đọc tệp: " + ex.Message);
            }
        }

        public ActionResult Edit(int id)
        {
            var users = LoadUsersFromFile();
            var user = users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return HttpNotFound(); // Trả về 404 nếu không tìm thấy người dùng
            }
            return View(user); // Truyền một đối tượng TichDiem vào view
        }


        [HttpPost]
        public ActionResult Edit(TichDiem model)
        {
            if (ModelState.IsValid)
            {
                var users = LoadUsersFromFile();
                var user = users.FirstOrDefault(u => u.ID == model.ID);
                if (user != null)
                {
                    user.maKH = model.maKH;
                    user.tenKH = model.tenKH;
                    user.diem = model.diem;
                    SaveUsersToFile(users);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        // Xóa người dùng
        public ActionResult Delete(int id)
        {
            var users = LoadUsersFromFile();
            var user = users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return HttpNotFound(); // Trả về 404 nếu không tìm thấy người dùng
            }
            return View(user); // Trả về view để xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var users = LoadUsersFromFile();
            var userToRemove = users.FirstOrDefault(u => u.ID == id);
            if (userToRemove != null)
            {
                users.Remove(userToRemove); // Xóa người dùng khỏi danh sách
                SaveUsersToFile(users); // Lưu danh sách đã cập nhật vào tệp
            }
            return RedirectToAction("Index"); // Quay lại danh sách
        }
    }
}
