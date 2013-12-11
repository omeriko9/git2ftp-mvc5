using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using git2ftp_mvc5.Models;

namespace git2ftp_mvc5.Controllers
{
    public class ProjectsController : Controller
    {
        private omeriko9Entities db = new omeriko9Entities();

        // GET: /Projects/
        public async Task<ActionResult> Index()
        {
            var git2ftp_projects = db.git2ftp_Projects.Include(g => g.git2ftp_Users);
            return View(await git2ftp_projects.ToListAsync());
        }

        // GET: /Projects/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            git2ftp_Projects git2ftp_projects = await db.git2ftp_Projects.FindAsync(id);
            if (git2ftp_projects == null)
            {
                return HttpNotFound();
            }
            return View(git2ftp_projects);
        }

        // GET: /Projects/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.git2ftp_Users, "pKey", "Username");
            return View();
        }

        // POST: /Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="pKey,UserID,FTPAddress,FTPUsername,FTPPassword,GitApiKey,GitRepositoryName,GitOwner")] git2ftp_Projects git2ftp_projects)
        {
            if (ModelState.IsValid)
            {
                db.git2ftp_Projects.Add(git2ftp_projects);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.git2ftp_Users, "pKey", "Username", git2ftp_projects.UserID);
            return View(git2ftp_projects);
        }

        // GET: /Projects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            git2ftp_Projects git2ftp_projects = await db.git2ftp_Projects.FindAsync(id);
            if (git2ftp_projects == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.git2ftp_Users, "pKey", "Username", git2ftp_projects.UserID);
            return View(git2ftp_projects);
        }

        // POST: /Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="pKey,UserID,FTPAddress,FTPUsername,FTPPassword,GitApiKey,GitRepositoryName,GitOwner")] git2ftp_Projects git2ftp_projects)
        {
            if (ModelState.IsValid)
            {
                db.Entry(git2ftp_projects).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.git2ftp_Users, "pKey", "Username", git2ftp_projects.UserID);
            return View(git2ftp_projects);
        }

        // GET: /Projects/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            git2ftp_Projects git2ftp_projects = await db.git2ftp_Projects.FindAsync(id);
            if (git2ftp_projects == null)
            {
                return HttpNotFound();
            }
            return View(git2ftp_projects);
        }

        // POST: /Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            git2ftp_Projects git2ftp_projects = await db.git2ftp_Projects.FindAsync(id);
            db.git2ftp_Projects.Remove(git2ftp_projects);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
