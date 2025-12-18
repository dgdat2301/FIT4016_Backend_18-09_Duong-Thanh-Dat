using System;

namespace StudentManagementSystem
{
    // ============================================
    // TODO 6.1: LỚP STUDENT
    // ============================================
    public class Student
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public double Score { get; set; }

        // Constructor với validation
        public Student(string id, string name, double score)
        {
            // Validation cho StudentId
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Mã sinh viên không được để trống");

            // Validation cho Name
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tên sinh viên không được để trống");

            // Validation cho Score
            if (score < 0 || score > 10)
                throw new ArgumentException("Điểm phải từ 0 đến 10");

            StudentId = id;
            Name = name;
            Score = score;
        }

        // Phương thức in thông tin
        public void Display()
        {
            Console.WriteLine($"🎓 ID: {StudentId} | Tên: {Name} | Điểm: {Score:F2}");
        }
    }

    // ============================================
    // TODO 6.2: LỚP STUDENT MANAGER
    // ============================================
    public class StudentManager
    {
        private Student[] students = new Student[50];
        private int count = 0; // Số lượng sinh viên hiện tại

        // Thêm sinh viên mới
        public bool AddStudent(string id, string name, double score)
        {
            try
            {
                // Kiểm tra nếu đã đầy
                if (count >= students.Length)
                {
                    Console.WriteLine("❌ Danh sách đã đầy! Không thể thêm sinh viên mới.");
                    return false;
                }

                // Kiểm tra trùng ID
                if (FindStudentById(id) != null)
                {
                    Console.WriteLine($"❌ Đã tồn tại sinh viên với ID: {id}");
                    return false;
                }

                // Tạo sinh viên mới (validation trong constructor)
                Student newStudent = new Student(id, name, score);
                students[count] = newStudent;
                count++;

                Console.WriteLine($"✅ Đã thêm sinh viên: {name} (ID: {id})");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
                return false;
            }
        }

        // Xóa sinh viên theo ID
        public bool RemoveStudent(string id)
        {
            for (int i = 0; i < count; i++)
            {
                if (students[i].StudentId == id)
                {
                    // Dịch chuyển các phần tử phía sau lên
                    for (int j = i; j < count - 1; j++)
                    {
                        students[j] = students[j + 1];
                    }
                    students[count - 1] = null;
                    count--;

                    Console.WriteLine($"✅ Đã xóa sinh viên có ID: {id}");
                    return true;
                }
            }

            Console.WriteLine($"❌ Không tìm thấy sinh viên với ID: {id}");
            return false;
        }

        // Cập nhật điểm
        public bool UpdateScore(string id, double newScore)
        {
            try
            {
                // Validation cho điểm mới
                if (newScore < 0 || newScore > 10)
                    throw new ArgumentException("Điểm phải từ 0 đến 10");

                Student student = FindStudentById(id);
                if (student != null)
                {
                    double oldScore = student.Score;
                    student.Score = newScore;
                    Console.WriteLine($"✅ Đã cập nhật điểm cho {student.Name}: {oldScore:F2} → {newScore:F2}");
                    return true;
                }

                Console.WriteLine($"❌ Không tìm thấy sinh viên với ID: {id}");
                return false;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
                return false;
            }
        }

        // Tính điểm trung bình
        public double GetAverageScore()
        {
            if (count == 0)
                return 0;

            double total = 0;
            for (int i = 0; i < count; i++)
            {
                total += students[i].Score;
            }
            return total / count;
        }

        // Tìm điểm cao nhất
        public (Student student, double maxScore) GetMaxScore()
        {
            if (count == 0)
                return (null, 0);

            double max = students[0].Score;
            Student topStudent = students[0];

            for (int i = 1; i < count; i++)
            {
                if (students[i].Score > max)
                {
                    max = students[i].Score;
                    topStudent = students[i];
                }
            }
            return (topStudent, max);
        }

        // Tìm điểm thấp nhất
        public (Student student, double minScore) GetMinScore()
        {
            if (count == 0)
                return (null, 0);

            double min = students[0].Score;
            Student bottomStudent = students[0];

            for (int i = 1; i < count; i++)
            {
                if (students[i].Score < min)
                {
                    min = students[i].Score;
                    bottomStudent = students[i];
                }
            }
            return (bottomStudent, min);
        }

        // Tìm sinh viên theo ID
        public Student FindStudentById(string id)
        {
            for (int i = 0; i < count; i++)
            {
                if (students[i].StudentId == id)
                    return students[i];
            }
            return null;
        }

        // In danh sách tất cả sinh viên
        public void DisplayAllStudents()
        {
            if (count == 0)
            {
                Console.WriteLine("📭 Danh sách sinh viên trống!");
                return;
            }

            Console.WriteLine($"\n📋 DANH SÁCH SINH VIÊN ({count} sinh viên)");
            Console.WriteLine(new string('=', 50));

            for (int i = 0; i < count; i++)
            {
                Console.Write($"{i + 1}. ");
                students[i].Display();
            }

            // Hiển thị thống kê
            Console.WriteLine(new string('-', 50));
            var (topStudent, maxScore) = GetMaxScore();
            var (bottomStudent, minScore) = GetMinScore();
            double average = GetAverageScore();

            if (topStudent != null)
            {
                Console.WriteLine($"📊 Điểm cao nhất: {maxScore:F2} - {topStudent.Name}");
                Console.WriteLine($"📊 Điểm thấp nhất: {minScore:F2} - {bottomStudent.Name}");
                Console.WriteLine($"📊 Điểm trung bình: {average:F2}");
            }
        }

        // Thêm phương thức đếm số sinh viên
        public int GetStudentCount()
        {
            return count;
        }
    }

    // ============================================
    // TODO 6.3: CHƯƠNG TRÌNH CHÍNH
    // ============================================
    class Program
    {
        static void Main(string[] args)
        {
            StudentManager manager = new StudentManager();
            bool running = true;

            // Thêm một số sinh viên mẫu
            manager.AddStudent("SV001", "Nguyễn Văn A", 8.5);
            manager.AddStudent("SV002", "Trần Thị B", 7.2);
            manager.AddStudent("SV003", "Lê Văn C", 9.0);

            Console.WriteLine("🎓 HỆ THỐNG QUẢN LÝ SINH VIÊN");
            Console.WriteLine("====================================");

            while (running)
            {
                try
                {
                    // In menu
                    Console.WriteLine("\n========== MENU CHÍNH ==========");
                    Console.WriteLine("1. 📝 Thêm sinh viên");
                    Console.WriteLine("2. 🗑️  Xóa sinh viên");
                    Console.WriteLine("3. 🔄 Cập nhật điểm");
                    Console.WriteLine("4. 📋 In danh sách sinh viên");
                    Console.WriteLine("5. 📊 Tính điểm trung bình");
                    Console.WriteLine("6. 🏆 Tìm điểm cao nhất/thấp nhất");
                    Console.WriteLine("7. 🔍 Tìm sinh viên theo ID");
                    Console.WriteLine("8. 📈 Thống kê");
                    Console.WriteLine("0. 🚪 Thoát");
                    Console.WriteLine("================================");
                    Console.Write("👉 Chọn chức năng: ");

                    // Nhận lựa chọn từ người dùng
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1": // Thêm sinh viên
                            Console.Write("Nhập mã sinh viên: ");
                            string id = Console.ReadLine();
                            Console.Write("Nhập tên sinh viên: ");
                            string name = Console.ReadLine();
                            Console.Write("Nhập điểm (0-10): ");
                            if (double.TryParse(Console.ReadLine(), out double score))
                                manager.AddStudent(id, name, score);
                            else
                                Console.WriteLine("❌ Điểm không hợp lệ!");
                            break;

                        case "2": // Xóa sinh viên
                            Console.Write("Nhập mã sinh viên cần xóa: ");
                            string removeId = Console.ReadLine();
                            manager.RemoveStudent(removeId);
                            break;

                        case "3": // Cập nhật điểm
                            Console.Write("Nhập mã sinh viên: ");
                            string updateId = Console.ReadLine();
                            Console.Write("Nhập điểm mới (0-10): ");
                            if (double.TryParse(Console.ReadLine(), out double newScore))
                                manager.UpdateScore(updateId, newScore);
                            else
                                Console.WriteLine("❌ Điểm không hợp lệ!");
                            break;

                        case "4": // In danh sách
                            manager.DisplayAllStudents();
                            break;

                        case "5": // Tính điểm trung bình
                            double average = manager.GetAverageScore();
                            Console.WriteLine($"📊 Điểm trung bình của tất cả sinh viên: {average:F2}");
                            break;

                        case "6": // Tìm điểm cao nhất/thấp nhất
                            var (topStudent, maxScore) = manager.GetMaxScore();
                            var (bottomStudent, minScore) = manager.GetMinScore();

                            if (topStudent != null)
                            {
                                Console.WriteLine("🏆 ĐIỂM CAO NHẤT:");
                                Console.WriteLine($"  Sinh viên: {topStudent.Name} (ID: {topStudent.StudentId})");
                                Console.WriteLine($"  Điểm: {maxScore:F2}");
                                Console.WriteLine("\n📉 ĐIỂM THẤP NHẤT:");
                                Console.WriteLine($"  Sinh viên: {bottomStudent.Name} (ID: {bottomStudent.StudentId})");
                                Console.WriteLine($"  Điểm: {minScore:F2}");
                            }
                            else
                            {
                                Console.WriteLine("📭 Không có sinh viên nào trong danh sách!");
                            }
                            break;

                        case "7": // Tìm sinh viên theo ID
                            Console.Write("Nhập mã sinh viên cần tìm: ");
                            string findId = Console.ReadLine();
                            Student found = manager.FindStudentById(findId);
                            if (found != null)
                            {
                                Console.WriteLine("✅ Tìm thấy sinh viên:");
                                found.Display();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Không tìm thấy sinh viên với ID: {findId}");
                            }
                            break;

                        case "8": // Thống kê
                            Console.WriteLine("📈 THỐNG KÊ HỆ THỐNG");
                            Console.WriteLine(new string('=', 30));
                            Console.WriteLine($"Số lượng sinh viên: {manager.GetStudentCount()}/50");

                            double avg = manager.GetAverageScore();
                            var (top, max) = manager.GetMaxScore();
                            var (bottom, min) = manager.GetMinScore();

                            Console.WriteLine($"Điểm trung bình: {avg:F2}");
                            if (top != null)
                            {
                                Console.WriteLine($"Điểm cao nhất: {max:F2} ({top.Name})");
                                Console.WriteLine($"Điểm thấp nhất: {min:F2} ({bottom.Name})");
                                Console.WriteLine($"Khoảng cách điểm: {max - min:F2}");
                            }
                            break;

                        case "0": // Thoát
                            Console.WriteLine("👋 Cảm ơn đã sử dụng hệ thống!");
                            running = false;
                            break;

                        default:
                            Console.WriteLine("❌ Lựa chọn không hợp lệ! Vui lòng chọn từ 0-8.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Đã xảy ra lỗi: {ex.Message}");
                    Console.WriteLine("Vui lòng thử lại!");
                }
            }
        }
    }
}