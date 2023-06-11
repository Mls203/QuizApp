using QuizApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuizApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        QuizAppEntities db = new QuizAppEntities();


        public ActionResult Index()
        {

            if (Session["ad_id"] != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }


        [HttpGet]
        public ActionResult TLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TLogin(tbl_admin ad)
        {
            tbl_admin a = db.tbl_admin.Where(x => x.ad_name == ad.ad_name && x.ad_pass == ad.ad_pass).SingleOrDefault();

            if (a != null)
            {
                Session["ad_id"] = a.ad_id;
                int deneme = Convert.ToInt32(Session["ad_id"]);
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.msg = "Invalid User Name or Password";
            }
            return View();
        }


        public ActionResult LogOut()
        {
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult SLogin()
        {

            return View();
        }

        [HttpPost]
        public ActionResult SLogin(student s)
        {
            student sv = db.student.Where(x => x.std_name == s.std_name && x.std_password == s.std_password).SingleOrDefault();


            if (sv == null)
            {
                ViewBag.msg = "Invalid User Name or Password";
                return View();
            }
            else
            {

                Session["std_id"] = sv.std_id;
                Session["std_name"] = sv.std_name;
                return RedirectToAction("ExamDashBoard");
            }


        }
        [HttpGet]
        public ActionResult ExamDashBoard()
        {

            if (Session["std_id"] == null)
            {
                return RedirectToAction("SLogin");//? SLogin
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult ExamDashBoard(string room)
        {
            List<tbl_category> list = db.tbl_category.ToList();

            foreach (var item in list)
            {
                if (item.cat_encrytped_string == room)
                {
                    Session["cat_encrytped_string"] = item.cat_encrytped_string;
                    Session["cat_name"] = item.cat_name;
                    Session["cat_id"] = item.cat_id;
                    List<tbl_questions> li = db.tbl_questions.Where(x => x.q_fk_catid == item.cat_id).ToList();
                    Queue<tbl_questions> queue = new Queue<tbl_questions>();
                    foreach (tbl_questions a in li)
                    {
                        queue.Enqueue(a);

                    }

                    TempData["examid"] = item.cat_fk_ad_id;
                    TempData["questions"] = queue;
                    TempData.Keep();
                    Session["scroreExam"] = 0;
                    return RedirectToAction("StartExam");
                }

            }

            ViewBag.msg = "No Room Found...";
            return View();
        }


        public ActionResult StartExam()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("SLogin");
            }

            tbl_questions q = null;
            if (TempData["questions"] != null)
            {
                Queue<tbl_questions> qlist = (Queue<tbl_questions>)TempData["questions"];
                if (qlist.Count > 0)
                {
                    q = qlist.Peek();
                    Session["QCorrectAns"] = q.QCorrectAns;
                    qlist.Dequeue();

                    TempData["questions"] = qlist;
                    TempData.Keep();
                }
                else
                {
                    return RedirectToAction("EndExam");
                }
            }
            else
            {
                return RedirectToAction("ExamDashboard");
            }

            TempData.Keep();
            return View(q);
        }



        [HttpPost]
        public ActionResult StartExam(tbl_questions q, string answer)
        {
            string correctAns = Session["QCorrectAns"].ToString();


            if ((correctAns == answer))
            {
                Session["scroreExam"] = (int)Session["scroreExam"] + 1;

            }

            TempData.Keep();

            return RedirectToAction("StartExam");
        }


        public ActionResult ViewAllQuestions(int? id)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("TLogin");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }
            Session["q_fk_catid"] = id;
            return View(db.tbl_questions.Where(x => x.q_fk_catid == id).ToList());
        }

        [HttpGet]
        public ActionResult SRegister()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SRegister(student s, HttpPostedFileBase imageFile)
        {
            student new_student = new student();
            try
            {
                new_student.std_name = s.std_name;
                new_student.std_password = s.std_password;
                new_student.std_image = UploadImage(imageFile);

                if (new_student.std_image == "-1")
                {
                    ViewBag.msg = "An error occurred.";
                }
                else if (new_student.std_image == "-2")
                {
                    ViewBag.msg = "Only jpg,jpeg or png formats are acceptable....";
                }
                else if (new_student.std_image == "-3")
                {
                    ViewBag.msg = "Please select a file";
                }
                else
                {
                    db.student.Add(new_student);
                    db.SaveChanges();

                    return RedirectToAction("SLogin");

                }

            }
            catch (Exception)
            {
                ViewBag.msg = "Data could not be inserted...";
            }

            return View();
        }

        public string UploadImage(HttpPostedFileBase file)
        {

            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("/Content/img"), random + Path.GetFileName(file.FileName));

                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception)
                    {
                        path = "-1";
                        //error
                    }

                }
                else
                {
                    path = "-2";
                    /* "Only jpg,jpeg or png formats are acceptable...."*/
                }

            }
            else
            {
                path = "no file";
                //"Please select a file"  
            }

            return path;

        }


        public ActionResult Dashboard()
        {


            return View();
        }


        public ActionResult EndExam()
        {
            if (Session["scroreExam"] == null)
            {
                Session["scroreExam"] = 0;
            }
            TempData["Score"] = Session["scroreExam"].ToString();

            report std_reposrt = new report();

            std_reposrt.cat_encrytped_string = Session["cat_encrytped_string"].ToString();

            std_reposrt.std_id = (int)Session["std_id"];
            std_reposrt.cat_name = Session["cat_name"].ToString();
            std_reposrt.std_name = Session["std_name"].ToString();
            std_reposrt.std_score = Session["scroreExam"].ToString();
            std_reposrt.cat_id = (int)Session["cat_id"];
            db.report.Add(std_reposrt);

            db.SaveChanges();

            Session.RemoveAll();
            return View();
        }

        [HttpGet]
        public ActionResult Add_Category()
        {
            int ad_id = Convert.ToInt32(Session["ad_id"].ToString());
            List<tbl_category> cat_list = db.tbl_category.Where(x => x.cat_fk_ad_id == ad_id).OrderByDescending(y => y.cat_id).ToList();
            ViewData["CategoryList"] = cat_list;
            return View();
        }

        [HttpPost]
        public ActionResult Add_Category(tbl_category cat)
        {

            List<tbl_category> cat_list = db.tbl_category.OrderByDescending(x => x.cat_id).ToList();
            ViewData["CategoryList"] = cat_list;

            tbl_category new_cat = new tbl_category();

            Random r = new Random();
            int number = r.Next();
            new_cat.cat_name = cat.cat_name;
            new_cat.cat_fk_ad_id = Convert.ToInt32(Session["ad_id"].ToString());

            new_cat.cat_encrytped_string = crypt.Encrypt(cat.cat_name.Trim() + number.ToString(), true);

            db.tbl_category.Add(new_cat);
            db.SaveChanges();

            return RedirectToAction("Add_Category");
        }

        [HttpGet]
        public ActionResult AddQuestions()
        {
            int sid = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_fk_ad_id == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult AddQuestions(tbl_questions question)
        {

            int sid = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_fk_ad_id == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");

            tbl_questions new_question = new tbl_questions();

            new_question.q_text = question.q_text;
            new_question.QA = question.QA;
            new_question.QB = question.QB;
            new_question.QC = question.QC;
            new_question.QD = question.QD;
            new_question.QCorrectAns = question.QCorrectAns;
            new_question.q_fk_catid = question.q_fk_catid;

            db.tbl_questions.Add(new_question);
            db.SaveChanges();
            TempData["msg"] = "Question Successfully Added";
            TempData.Keep();

            return RedirectToAction("AddQuestions");
        }


        [HttpPost]
        public PartialViewResult Edit(int? q_id)
        {
            var sonuc = (from k in db.tbl_questions
                         where k.q_id == q_id
                         select k).FirstOrDefault();

            return PartialView("_PartialPageEditModal", (from k in db.tbl_questions
                                                         where k.q_id == q_id
                                                         select k).FirstOrDefault());

        }

        public JsonResult Save(tbl_questions question)
        {

            var entity = db.Entry(question);
            entity.State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { status = "S" });
            //save
        }


        [HttpPost]
        public ActionResult Delete(tbl_questions questions)
        {

            var entity = db.Entry(questions);
            entity.State = EntityState.Deleted;
            db.SaveChanges();
            return Json(new { status = "S" });

        }


        [HttpGet]
        public ActionResult Report()
        {
            Session["report_id"] = 0;
            int ad_id = Convert.ToInt32(Session["ad_id"].ToString());
            List<tbl_category> cat_list = db.tbl_category.Where(x => x.cat_fk_ad_id == ad_id).OrderByDescending(y => y.cat_id).ToList();
            ViewData["CategoryList"] = cat_list;
            return View();
        }

        [HttpPost]
        public ActionResult Report(int id)
        {
            Session["report_id"] = id;
            List<report> report_list = db.report.Where(x => x.cat_id == id).ToList();
            TempData["ResultsExam"] = report_list;
            return Json(new { status = "S" });

        }

        public ActionResult ResultsExam()
        {
            if ((int)Session["report_id"] != 0)
            {
                int id = (int)Session["report_id"];
                List<report> report_list = db.report.Where(x => x.cat_id == id).ToList();
                TempData["ResultsExam"] = report_list;
            }
            return View();
        }
    }
}