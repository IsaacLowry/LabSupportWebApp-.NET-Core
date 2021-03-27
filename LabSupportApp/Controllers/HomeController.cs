using LabSupportApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LabSupportApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMongoCollection<BsonDocument> _adminUsersCollection;
        private IMongoCollection<QueueObject> _queueAdminCollection;
        private IMongoCollection<StudentUser> _studentUsers;
        private IMongoCollection<StudentUser> _studentUsersBackup;
        //private TaskCompletionSource<bool> _pool = new TaskCompletionSource<bool>();

        private string _Message { get; set; }



        public HomeController(IMongoClient client)
        {
            var database = client.GetDatabase("LabSupport");
            _adminUsersCollection = database.GetCollection<BsonDocument>("StaffUsers");
            _queueAdminCollection = database.GetCollection<QueueObject>("QueueCollection");
            _studentUsers = database.GetCollection<StudentUser>("QueueingStudents");
            _studentUsersBackup = database.GetCollection<StudentUser>("QueueingStudentsBackup");


            try
            {
                DateTime now = new DateTime();
                now = DateTime.Now;
                Console.WriteLine(now);
                //var result = _adminUsersCollection.Find(new BsonDocument()).ToList();
                //foreach (var user in result)
                //{
                //    Console.WriteLine(user.ToString());
                //}

                var userUpdate = _studentUsers.Find(studentUser => true).ToList();
                foreach (var sUser in userUpdate)
                {

                    TimeSpan newElapsedTime = (now - sUser.TimeEntered);
                    double elapInMins = newElapsedTime.TotalMinutes;
                    int ElapsedTime = Convert.ToInt32(elapInMins);
                    //Console.WriteLine(ElapsedTime);
                    var filter = Builders<StudentUser>.Filter.Eq("name", sUser.Name);
                    var update = Builders<StudentUser>.Update.Set("elapsedTime", ElapsedTime);
                    var changeElap = _studentUsers.UpdateOne(filter, update);



                    //string newElapsedTime = (sUser.ElapsedTime - now).TotalTime.ToString();
                }
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public IActionResult Index()
        {
            var fellowStudents = _studentUsers.Find(studentUser => true).ToList();
            foreach (var student in fellowStudents)
            {
                if (student.Name == User.Identity.Name)
                {
                    ViewBag.Error = "You are already in a queue!";
                }
            }

            string UserIsAdmin = "null";
            try
            {
                
                var result = _adminUsersCollection.Find(new BsonDocument()).ToList();
                foreach (var user in result)
                {
                    if (user.ToString().IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        UserIsAdmin = User.Identity.Name;
                    }
                }
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine(UserIsAdmin);

            if(UserIsAdmin == User.Identity.Name || User.FindFirst("name").Value.Contains("(Staff)"))
            {
                return View("AdminIndex");
            } else
            {
                return View();
            }

        }

        [HttpPost]
        public IActionResult Index(string NewAdminUser)
        {
            string UserIsAdmin = "null";
            AdminUser adminUser = new AdminUser(NewAdminUser);
            try
            {
                _adminUsersCollection.InsertOne(new BsonDocument("email", NewAdminUser));
                //testCollection.InsertOne(new BsonDocument("email", NewAdminUser));
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
            }

            var result = _adminUsersCollection.Find(new BsonDocument()).ToList();
            foreach (var user in result)
            {
                Console.WriteLine(user.ToString());
                if (user.ToString().IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    UserIsAdmin = User.Identity.Name;
                }
            }

            if (UserIsAdmin == User.Identity.Name || User.FindFirst("name").Value.Contains("(Staff)"))
            {
                return View("AdminIndex");
            }
            else
            {
                return View();
            }

        }

        public IActionResult AdminIndex()
        {
            var fellowStudents = _studentUsers.Find(studentUser => true).ToList();
            foreach (var student in fellowStudents)
            {
                if (student.Name == User.Identity.Name)
                {
                    ViewBag.Error = "You are already in a queue!";
                }
            }

            string UserIsAdmin = "null";
            try
            {

                var result = _adminUsersCollection.Find(new BsonDocument()).ToList();
                foreach (var user in result)
                {
                    if (user.ToString().IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        UserIsAdmin = User.Identity.Name;
                    }
                }
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
            }

            if (UserIsAdmin != User.Identity.Name || !User.FindFirst("name").Value.Contains("(Staff)"))
            {
                return View("Index");
            }
            else
            {
                return View();
            }

        }

        public IActionResult CreateAQueue()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Queue()
        {

            int myCode = 0;
            int studentsAhead = 0;
            int qSize = 0;
            int totalMins = 0;
            int meanElapsedTime;
            var fellowStudents = _studentUsers.Find(studentUser => true).ToList();
            foreach (var student in fellowStudents)
            {
                if (student.Name == User.Identity.Name)
                {
                    myCode = student.Qcode;
                }
            }
            var filter = Builders<StudentUser>.Filter.Where(p => p.Qcode == myCode);
            List<StudentUser> studentUsers = _studentUsers.Find(filter).ToList();
            foreach (var waitingStudent in studentUsers)
            {
                Console.WriteLine(waitingStudent.ElapsedTime);
                totalMins = totalMins + waitingStudent.ElapsedTime;
            }

            studentUsers.Sort((x, y) => DateTime.Compare(x.TimeEntered, y.TimeEntered));
            if (studentUsers.Count != 0)
            {
                foreach (var sUser in studentUsers)
                {
                    if (sUser.Name != User.Identity.Name)
                    {
                        studentsAhead = studentsAhead + 1;
                    }

                    if (sUser.Name == User.Identity.Name)
                    {
                        break;
                    }
                }
            }

                var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.Code == myCode)
                {
                    qSize = singleQ.StudentCount;
                    ViewBag.QueueSize = singleQ.StudentCount;
                    

                    

                    if (qSize != 1)
                    {
                        qSize = 1;
                    }

                    ViewBag.UsersInFront = studentsAhead;
                    meanElapsedTime = totalMins / qSize;
                    ViewBag.AvgWait = meanElapsedTime;
                }

                if (singleQ.StudentAtHead == User.Identity.Name)
                {
                    ViewBag.TeamsLink = singleQ.TeamsLink;
                    return View("HeadOfQueue");
                }

            }

            return View();
        }

        [HttpPost]
        public IActionResult Queue(int UserCode, string Description)
        {
            var fellowStudents = _studentUsers.Find(studentUser => true).ToList();
            foreach (var student in fellowStudents)
            {
                if (student.Name == User.Identity.Name)
                {
                    return RedirectToAction("Index");
                }
            }


            DateTime queueEntryTime = new DateTime();
            queueEntryTime = DateTime.Now;
            string Name = User.Identity.Name;
            int unsetElapsedTime = 0;

            StudentUser studentUser = new StudentUser(UserCode, queueEntryTime, Name, Description, unsetElapsedTime);

            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.Code == UserCode)
                {
                    int studentCounter = singleQ.StudentCount + 1;
                    var filter = Builders<QueueObject>.Filter.Eq("user", singleQ.User);
                    var update = Builders<QueueObject>.Update.Set("studentCount", studentCounter);
                    var result = _queueAdminCollection.UpdateMany(filter, update);
                }
            }

                    foreach (var singleQ in openQueues)
            {
                if (singleQ.Code == UserCode)
                {
                    _studentUsers.InsertOne(studentUser);
                    Console.WriteLine("Succesfully Added");
                    Console.WriteLine(singleQ.LabName);
                    int finalCount = singleQ.StudentCount;
                    

                    int totalTime = 0;
                    foreach (var student in fellowStudents)
                    {

                        if (student.Qcode == UserCode)
                        {
                            totalTime = totalTime + student.ElapsedTime;
                        }
                    }

                    if (finalCount != 0)
                    {
                        finalCount = finalCount + 1;
                    }

                    if (finalCount == 0)
                    {
                        finalCount = 1;
                    }

                    ViewBag.QueueSize = finalCount;
                    int meanElapsedTime = totalTime / finalCount;
                    int usersInFront = finalCount - 1;
                    Console.WriteLine(meanElapsedTime);
                    ViewBag.AvgWait = meanElapsedTime;
                    ViewBag.UsersInFront = usersInFront;

                    return View();
                    

                }
                //else if
                //{
                //    Console.WriteLine("No Queue to add to");
                //    return View("index");
                //}
            }

            return View("AdminIndex");
        }

        public IActionResult LeaveQueue()
        {
            int code = 0;
            int studentCount = 0;
            string studentName = null;
            var filter = Builders<StudentUser>.Filter.Where(p => p.Name == User.Identity.Name);
            List<StudentUser> studentUsers = _studentUsers.Find(filter).ToList();

            foreach (var sUser in studentUsers)
            {
                code = sUser.Qcode;
                studentName = sUser.Name;
                Console.WriteLine(studentName);
            }

            var findCount = Builders<QueueObject>.Filter.Where(p => p.Code == code);
            List<QueueObject> queueObjects = _queueAdminCollection.Find(findCount).ToList();

            foreach (var q in queueObjects)
            {
                studentCount = q.StudentCount;
            }

            if (studentName != null)
            {
                //var findUser = Builders<StudentUser>.Filter.Eq("user", studentName);
                var findUser = Builders<StudentUser>.Filter.Where(p => p.Name == studentName);
                _studentUsers.DeleteOne(findUser);
            }

            if (studentCount != 0)
            {
                studentCount = studentCount - 1;
                var findQ = Builders<QueueObject>.Filter.Eq("code", code);
                var update = Builders<QueueObject>.Update.Set("studentCount", studentCount);
                var result = _queueAdminCollection.UpdateMany(findQ, update);
            }




            return View("index");
        }

        public IActionResult HeadOfQueue()
        {
            //ViewBag.TeamsLink = QueueObject.TeamsLink;

            return View();
        }

        public IActionResult QueueComplete()
        {
            return View();
        }

        public IActionResult QueueList()
        {

            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    ViewBag.code = singleQ.Code;
                    ViewBag.StudentsLeft = singleQ.StudentCount;
                    ViewBag.LabName = singleQ.LabName;
                    return View("QueueManager");
                }
            }

            ViewBag.Error = "Please create a queue or ask an administrator to add you";
                return View("AdminIndex");
        }
        public IActionResult QueueManager()
        {
            int userCode = 0;
            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var q in openQueues)
            {
                if (q.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    ViewBag.code = q.Code;
                    ViewBag.StudentsLeft = q.StudentCount;
                    ViewBag.LabName = q.LabName;
                    userCode = q.Code;
                }
            }

            if (userCode != 0)
            {
                var filter = Builders<StudentUser>.Filter.And(
                    Builders<StudentUser>.Filter.Where(p => p.Qcode == userCode),
                    Builders<StudentUser>.Filter.Where(p => p.Description != null)
                    );
                   
                List<StudentUser> studentUsers = _studentUsers.Find(filter).ToList();
                foreach (var student in studentUsers)
                {
                    Console.WriteLine(student.Name);
                    Console.WriteLine(student.Description);
                }

                if (studentUsers.Count !=0)
                {
                    return View(studentUsers);
                }
                return View();
            }

            if (userCode == 0)
            {
                return View("AdminIndex");
            }
          

            return View();
        }

        [HttpPost]
        public IActionResult QueueManager(string labName, string TeamsLink)
        {

            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    labName = null;
                    TeamsLink = null;
                    ViewBag.Error = "Click View Queue to see your open queue";
                    return View("AdminIndex");
                }

            }

            string empty = "No one";
            string user = User.Identity.Name;
            int Qcount = 0;
            Random QcodeRandomiser = new Random();
            int code = QcodeRandomiser.Next(100000, 1000000);
            QueueObject queueObject = new QueueObject(labName, TeamsLink, code, user, Qcount, empty);


            _queueAdminCollection.InsertOne(queueObject);


            Console.WriteLine(code);
            ViewBag.StudentsLeft = Qcount;
            ViewBag.LabName = labName;
            ViewBag.code = code;




            //QueueObject.labName = labName;
            //QueueObject.TeamsLink = TeamsLink;

            //QueueObject.code = code;
            //Console.WriteLine(QueueObject.labName);
            //Console.WriteLine(QueueObject.TeamsLink);
            //Console.WriteLine(code);


            //ViewBag.code = queueObject.code;
            //Console.WriteLine(QueueObject.code);

            List<StudentUser> studentUsers = new List<StudentUser>();


            if (studentUsers.Count == 0)
            {
                return View(studentUsers);
            }

            return View("QueueManager");



        }

        //[HttpPost]
        //public IActionResult QueueManager(string AddAdminUser, string AddTeamsLink, string RemoveAdminUser)
        //{
        //    return View();
        //}

        public IActionResult NewAdminUser()
        {
            return View();
        }

        public IActionResult RemoveAdminFromQueue()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RemoveAdminFromQueue(string RemoveAdminUser)
        {
            int code = 0;
            string UserMessage = "User Removed!";
            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    code = singleQ.Code;
                }


                if (singleQ.User.IndexOf(RemoveAdminUser, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    if (code == singleQ.Code)
                    {
                        var filter = Builders<QueueObject>.Filter.Eq("user", singleQ.User);
                        _queueAdminCollection.DeleteOne(filter);
                        ViewBag.UserSuccess = UserMessage;
                        return View();

                    }

                }

            }

            UserMessage = "User Is Not Managing This Queue!";
            ViewBag.UserError = UserMessage;
            return View();
        }

        public IActionResult AddAdminToQueue()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddAdminToQueue(string AddAdminUser, string AddTeamsLink)
        {
            string UserExists = "User already in a queue";
            int code;
            string LabName;
            string studentAtHead;
            int StudentCount;
            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(AddAdminUser, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    ViewBag.UserError = UserExists;
                    return View();
                }
                //if (singleQ.User == User.Identity.Name)
                //{
                //    labName = null;
                //    TeamsLink = null;
                //    ViewBag.Error = "Click View Queue to see your open queue";
                //    return View("AdminIndex");
                //}

            }


            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    
                    code = singleQ.Code;
                    LabName = singleQ.LabName;
                    StudentCount = singleQ.StudentCount;
                    studentAtHead = singleQ.StudentAtHead;

                    QueueObject queueObject = new QueueObject(LabName, AddTeamsLink, code, AddAdminUser, StudentCount, studentAtHead);
                    _queueAdminCollection.InsertOne(queueObject);
                    ViewBag.UserSuccess = "Admin Added To Queue!";
                    return View();
                }
            }

            return View();
        }

        public IActionResult PullToHead (string Name)
        {
            int code = 0;
            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            var sUsers = _studentUsers.Find(studentUser => true).ToList();
            var copyUsers = _studentUsersBackup.Find(studentUser => true).ToList();
            foreach (var student in sUsers)
            {
                if(student.Name == Name)
                {
                    code = student.Qcode;
                    foreach (var q in openQueues)
                    {
                        if (q.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            int count = q.StudentCount - 1;
                            var filter = Builders<QueueObject>.Filter.Eq("code", code);
                            var update = Builders<QueueObject>.Update.Set("studentCount", count);
                            var result = _queueAdminCollection.UpdateMany(filter, update);

                            var findAdmin = Builders<QueueObject>.Filter.Eq("user", User.Identity.Name);
                            var headUpdate = Builders<QueueObject>.Update.Set("studentAtHead", student.Name);
                            var result2 = _queueAdminCollection.UpdateOne(findAdmin, headUpdate);
                            _studentUsersBackup.InsertOne(student);
                            Console.WriteLine(student.Name);
                            var deleteFilter = Builders<StudentUser>.Filter.Eq("name", student.Name);
                            _studentUsers.DeleteOne(deleteFilter);

                            ViewBag.StudentsLeft = count;
                            ViewBag.LabName = q.LabName;
                            ViewBag.code = q.Code;



                            return View("QueueManager");
                        }
                    }
                }

            }

            return View("QueueManager");
        }


        public IActionResult NextStudent()
        {
            string LabName = null;
            int labCount = 0;
            bool queueEmpty = true;
            int code = 0;
            var openQueues = _queueAdminCollection.Find(queueObject => true).ToList();
            var sUsers = _studentUsers.Find(studentUser => true).ToList();
            var copyUsers = _studentUsersBackup.Find(studentUser => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    code = singleQ.Code;
                    LabName = singleQ.LabName;
                    labCount = singleQ.StudentCount;

                    var filter = Builders<StudentUser>.Filter.Where(p => p.Qcode == singleQ.Code);
                    List<StudentUser> studentUsers = _studentUsers.Find(filter).ToList();
                    studentUsers.Sort((x, y) => DateTime.Compare(x.TimeEntered, y.TimeEntered));
                    if (studentUsers.Count != 0)
                    {

                        queueEmpty = false;
                        var fifoStudent = studentUsers.First();
                        var findAdmin = Builders<QueueObject>.Filter.Eq("user", User.Identity.Name);
                        var update = Builders<QueueObject>.Update.Set("studentAtHead", fifoStudent.Name);
                        var result = _queueAdminCollection.UpdateOne(findAdmin, update);
                        _studentUsersBackup.InsertOne(fifoStudent);
                        Console.WriteLine(fifoStudent.Name);
                        var fifoFilter = Builders<StudentUser>.Filter.Eq("name", fifoStudent.Name);
                        _studentUsers.DeleteOne(fifoFilter);
                    }
                }

                if (queueEmpty == false)
                {
                    int studentCounter = singleQ.StudentCount - 1;
                    var countFilter = Builders<QueueObject>.Filter.Eq("code", singleQ.Code);
                    var update = Builders<QueueObject>.Update.Set("studentCount", studentCounter);
                    var result = _queueAdminCollection.UpdateMany(countFilter, update);

                    ViewBag.StudentsLeft = studentCounter;
                    ViewBag.LabName = singleQ.LabName;
                    ViewBag.code = singleQ.Code;

                    return View("QueueManager");
                }

            }

            if (code != 0)
            {
                ViewBag.StudentsLeft = labCount;
                ViewBag.LabName = LabName;
                ViewBag.code = code;
            }


                return View("QueueManager");
        }

        public IActionResult EndSession()
        {
            int currentQcode = 0;
            var openQueues = _queueAdminCollection.Find(studentUser => true).ToList();
            foreach (var singleQ in openQueues)
            {
                if (singleQ.User.IndexOf(User.Identity.Name, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    currentQcode = singleQ.Code;
                }
            }

            if (currentQcode != 0)
            {
                var filter = Builders<QueueObject>.Filter.Where(p => p.Code == currentQcode);
                List<QueueObject> queueObjects = _queueAdminCollection.Find(filter).ToList();
                if (queueObjects.Count == 1)
                {
                    var fifoFilter = Builders<StudentUser>.Filter.Eq("qcode", currentQcode);
                    _studentUsers.DeleteMany(fifoFilter);
                }
            }


            var findAdmin = Builders<QueueObject>.Filter.Eq("user", User.Identity.Name);
            _queueAdminCollection.DeleteOne(findAdmin);

            return View("AdminIndex");
        }

            //public async Task<string> WaitAsync()
            //{
            //    await Task.WhenAny(_pool.Task, Task.Delay(10));

            //    return this._Message;
            //}



            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
