using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class TrangChuController : Controller
    {
        public ActionResult Index()
        {
            //Debug.Write("Connectring: " + connectionString);
            return View();
        }
    }
}